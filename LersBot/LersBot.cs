using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LersBot
{
	/// <summary>
	/// Класс для реализации механизма бота.
	/// </summary>
	class LersBot
	{
		private Telegram.Bot.TelegramBotClient bot;

		private Dictionary<string, Action<User, string[]>> commandHandlers = new Dictionary<string, Action<User, string[]>>();

		public string UserName
		{
			get
			{
				return this.bot.GetMeAsync().Result.Username;
			}
		}

		public LersBot()
		{
			this.bot = new Telegram.Bot.TelegramBotClient(Config.Instance.Token);

			this.bot.OnMessage += Bot_OnMessage;
		}

		/// <summary>
		/// Запускает бота.
		/// </summary>
		internal void Start()
		{
			// Инициируем подключения к серверу

			foreach (User user in User.Where(x => x.Context != null))
			{
				user.Connect();
			}

			this.bot.StartReceiving();
		}

		private void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
		{
			// Проверим от кого поступила команда.

			long chatId = e.Message.Chat.Id;

			try
			{
				User user = User.Where(u => u.TelegramUserId == e.Message.From.Id).FirstOrDefault();

				if (user == null)
				{
					user = new User
					{
						ChatId = chatId,
						TelegramUserId = e.Message.From.Id
					};

					User.Add(user);
				}
				else if (user.Context != null)
				{
					// Подключаемся к серверу.
					user.Connect();
				}

				// Обрабатываем команду.
				ProcessCommand(user, e.Message.Text);
			}
			catch (Exception exc)
			{
				var message = $"Ошибка обработки команды. {exc.Message}";

				bot.SendTextMessageAsync(chatId, message);

				Logger.LogError(message);
			}
		}


		private void ProcessCommand(User user, string text)
		{
			string[] commandFields = Regex.Split(text, @"\s");

			if (commandFields.Length == 0)
			{
				throw new Exception("Неверный формат запроса");
			}

			// Получим аргументы запроса

			string command;
			string[] arguments;


			Action<User, string[]> handler;

			// Пользователь может уже обрабатывать команду. В этом случае передадим в качестве
			// текста команды выполняющуся команду, а в качестве аргументов весь текст сообщения.

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
				handler(user, arguments);
			}
			else
			{
				SendText(user.ChatId, "Неизвестная команда");
			}
		}

		public void SendText(long chatId, string message)
		{
			this.bot.SendTextMessageAsync(chatId, message).Wait();
		}

		public void AddCommandHandler(Action<User, string[]> handler, params string[] commandNames)
		{
			foreach (string cmd in commandNames)
				this.commandHandlers[cmd] = handler;
		}
	}
}
