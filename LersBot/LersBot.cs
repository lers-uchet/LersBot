using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Lers;

namespace LersBot
{
	/// <summary>
	/// Класс для реализации механизма бота.
	/// </summary>
	class LersBot
	{
		private Telegram.Bot.TelegramBotClient bot;

		private Dictionary<string, Action<LersServer, long, string[]>> commandHandlers = new Dictionary<string, Action<LersServer, long, string[]>>();

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
			this.bot.StartReceiving();
		}

		private void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
		{
			// Проверим от кого поступила команда.

			long chatId = e.Message.Chat.Id;

			try
			{
				UserName user = Config.Instance.Users.Where(u => u.TelegramUser == e.Message.From.Username || u.TelegramUser == "<anonymous>").FirstOrDefault();

				if (user == null)
				{
					bot.SendTextMessageAsync(chatId, "Извините, я вас не знаю.");
				}
				else
				{
					if (user.Context.ChatId != chatId)
					{
						user.Context.ChatId = chatId;

						Config.SaveContexts();
					}

					// Подключаемся к серверу.
					ConnectToServer(user);

					// Обрабатываем команду.
					ProcessCommand(user.Context.Server, chatId, e.Message.Text);
				}
			}
			catch (Exception exc)
			{
				var message = $"Ошибка обработки команды. {exc.Message}";

				bot.SendTextMessageAsync(chatId, message);

				Console.WriteLine(message);
			}
		}

		private void ConnectToServer(UserName user)
		{
			if (user.Context.Server == null || !user.Context.Server.IsConnected)
			{
				user.Context.Server = Connect(user);
			}
		}

		private static LersServer Connect(UserName user)
		{
			var server = new LersServer();

			var auth = new Lers.Networking.BasicAuthenticationInfo(user.LersUser, Lers.Networking.SecureStringHelper.ConvertToSecureString(user.LersPassword));

			server.VersionMismatch += (sender, e) => e.Ignore = true;

			server.Connect(Config.Instance.LersServerAddress, Config.Instance.LersServerPort, auth);

			return server;
		}

		private void ProcessCommand(LersServer server, long chatId, string text)
		{
			string[] commandFields = Regex.Split(text, @"\s");

			if (commandFields.Length == 0)
			{
				throw new Exception("Неверный формат запроса");
			}

			// Получим аргументы запроса

			string[] arguments = new string[commandFields.Length - 1];

			Array.Copy(commandFields, 1, arguments, 0, arguments.Length);

			Action<LersServer, long, string[]> handler;

			if (this.commandHandlers.TryGetValue(commandFields[0], out handler))
			{
				handler(server, chatId, arguments);
			}
			else
			{
				bot.SendTextMessageAsync(chatId, "Неизвестная команда");
			}
		}

		public void SendText(long chatId, string message)
		{
			this.bot.SendTextMessageAsync(chatId, message).Wait();
		}

		public void AddCommandHandler(Action<LersServer, long, string[]> handler, params string[] commandNames)
		{
			foreach (string cmd in commandNames)
				this.commandHandlers[cmd] = handler;
		}
	}
}
