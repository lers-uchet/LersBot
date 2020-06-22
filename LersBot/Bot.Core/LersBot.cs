using LersBot.Bot.Core;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;

namespace LersBot
{
	using CommandHandler = Action<User, string[]>;

	/// <summary>
	/// Класс для реализации механизма бота.
	/// </summary>
	class LersBot
	{
		private readonly Telegram.Bot.TelegramBotClient _bot;
		private readonly UsersService _users;
		private Dictionary<string, CommandHandler> commandHandlers = new Dictionary<string, CommandHandler>();

		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


		public async Task<string> GetUserName() => (await _bot.GetMeAsync()).Username;

		
		public LersBot(IOptionsSnapshot<Config> optionsSnapshot, UsersService users)
		{
			var options = optionsSnapshot.Value;

			_bot = new TelegramBotClient(options.Token);
			_users = users;
		}

		
		/// <summary>
		/// Запускает бота.
		/// </summary>
		internal void Start()
		{
			_bot.OnMessage += Bot_OnMessage;

			// Инициируем подключения к серверу
			/*
			foreach (var user in User.Where(x => x.Context != null))
			{
				user.Connect();
			}*/

			_bot.StartReceiving();
		}

		
		private async void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
		{
			// Обрабатываем только текстовые сообщения.
			if (e.Message.Type != Telegram.Bot.Types.Enums.MessageType.Text)
			{
				return;
			}

			// Проверим от кого поступила команда.

			long chatId = e.Message.Chat.Id;

			try
			{
				var user = _users.Where(u => u.TelegramUserId == e.Message.From.Id).FirstOrDefault();

				if (user == null)
				{
					user = new User
					{
						ChatId = chatId,
						TelegramUserId = e.Message.From.Id
					};

					_users.Add(user);
				}

				// Обрабатываем команду.
				ProcessCommand(user, e.Message.Text);
			}
			catch (Exception exc)
			{
				var message = $"Ошибка обработки команды. {exc.Message}";

				await _bot.SendTextMessageAsync(chatId, message);

				logger.Error(message);
			}
		}

		
		private void ProcessCommand(User user, string text)
		{
			// Разделяем сообщение на аргументы.
			string[] commandFields = CommandArguments.Split(text);

			if (commandFields.Length == 0)
			{
				throw new Exception("Неверный формат запроса");
			}

			// Получим аргументы запроса

			string command;
			string[] arguments;
			CommandHandler handler;

			// Пользователь может уже обрабатывать команду. В этом случае передадим в качестве
			// текста команды выполняющуюся команду, а в качестве аргументов весь текст сообщения.

			if (user.CommandContext != null)
			{
				command = user.CommandContext.Text;
				arguments = new string[] { text };
			}
			else
			{
				command = commandFields.FirstOrDefault();
				arguments = new string[commandFields.Length - 1];
				Array.Copy(commandFields, 1, arguments, 0, arguments.Length);
			}

			if (this.commandHandlers.TryGetValue(command, out handler))
			{
				if (IsAuthorizeRequired(handler))
				{
					if (user.Context != null)
					{
						// Подключаемся к серверу.

						user.Connect();
					}
					else
					{
						throw new UnauthorizedCommandException(command);
					}
				}

				handler(user, arguments);
			}
			else
			{
				SendText(user.ChatId, "Неизвестная команда");
			}
		}

		public void SendText(long chatId, string message)
		{
			SendTextAsync(chatId, message).Wait();
		}

		/// <summary>
		/// Асинхронно отправляет сообщение пользователю.
		/// </summary>
		/// <param name="chatId"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public async Task SendTextAsync(long chatId, string message)
		{
			try
			{
				await _bot.SendTextMessageAsync(chatId, message);
			}
			catch (Telegram.Bot.Exceptions.ApiRequestException exc) when (exc.ErrorCode == 403)
			{
				var user = _users.FirstOrDefault(x => x.ChatId == chatId);

				if (user != null)
				{
					logger.Error($"Пользователь {user.Context.Login} запретил боту отправлять сообщения. Пользователь удаляется из списка.");

					_users.Remove(user);
				}
			}
		}

		public void AddCommandHandler(CommandHandler handler, params string[] commandNames)
		{
			foreach (string cmd in commandNames)
			{
				this.commandHandlers[cmd] = handler;
			}
		}

		internal async Task SendDocument(long chatId, MemoryStream stream, string caption, string fileName)
		{
			stream.Seek(0, SeekOrigin.Begin);

			var document = new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream, fileName);

			await _bot.SendDocumentAsync(chatId, document, caption);
		}

		private static bool IsAuthorizeRequired(CommandHandler handler)
		{
			bool required = true;

			var methodInfo = handler.Method;

			var customAttribute = (AuthorizeAttribute)methodInfo.GetCustomAttributes(typeof(AuthorizeAttribute), true).FirstOrDefault();

			if (customAttribute != null)
			{
				required = customAttribute.IsAuthorizeRequired;
			}

			return required;
		}
	}
}
