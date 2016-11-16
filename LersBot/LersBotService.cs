using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Lers;
using Lers.Core;
using Lers.Data;

namespace LersBot
{
	/// <summary>
	/// Windows-служба бота LersBot.
	/// </summary>
	partial class LersBotService : ServiceBase
	{
		private LersBot bot;

		private Notifier notifier;

		private const string StartCommand = "/start";
		private const string GetCurrentsCommand = "/getcurrents";
		private const string GetNodesCommand = "/nodes";
		private const string GetMeasurePointsCommand = "/mpts";
		public const string SetNotifyCommand = "/setnotify";


		public LersBotService()
		{
			InitializeComponent();
		}

		public void Start()
		{
			OnStart(null);
		}

		public void MyStop()
		{
			OnStop();
		}


		/// <summary>
		/// Вызывается при запуске службы.
		/// </summary>
		/// <param name="args"></param>
		protected override void OnStart(string[] args)
		{
			try
			{
				Config.Load();
				User.LoadList();

				Logger.Initialize(Config.LogFilePath);

				bot = new LersBot();

				notifier = new Notifier(bot);

				Logger.LogMessage($"Stariting {bot.UserName}");

				bot.AddCommandHandler(HandleStart, StartCommand);
				bot.AddCommandHandler(ShowCurrents, GetCurrentsCommand);
				bot.AddCommandHandler(ShowNodes, GetNodesCommand);
				bot.AddCommandHandler(ShowMeasurePoints, GetMeasurePointsCommand);
				bot.AddCommandHandler(notifier.ProcessSetNotify, SetNotifyCommand);

				bot.Start();

				notifier.Start();
			}
			catch (Exception exc)
			{
				Logger.LogMessage($"Ошибка запуска бота. {exc.ToString()}");
				throw;
			}
		}

		/// <summary>
		/// Вызывается при остановке службы.
		/// </summary>
		protected override void OnStop()
		{
			notifier.Stop();

			Logger.LogMessage($"Stopped {bot.UserName}");
		}


		/// <summary>
		/// Обрабатывает команду начала сеанса работы пользователя с ботом.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="arguments"></param>
		private void HandleStart(User user, string[] arguments)
		{
			if (user.CommandContext == null)
			{
				// Начато выполнение команды. Создаём контекст команды и запрашиваем логин на сервере.

				user.CommandContext = new StartCommandContext(StartCommand);

				bot.SendText(user.ChatId, "Введите логин на сервере ЛЭРС УЧЁТ.");
			}
			else
			{
				// Команда уже выполняется.

				var context = (StartCommandContext)user.CommandContext;

				if (string.IsNullOrEmpty(context.Login))
				{
					// Логин ещё пустой, значит его передал пользователь. Сохраняем и запрашиваем пароль.

					context.Login = arguments[0];

					bot.SendText(user.ChatId, "Введите пароль на сервере ЛЭРС УЧЁТ");
				}
				else if (string.IsNullOrEmpty(context.Password))
				{
					// Если пароля ещё нет, значит он был передан сейчас. Сохраняем пароль и очищаем контекст команды.

					context.Password = arguments[0];

					user.CommandContext = null;

					// Создаём контекст пользователя ЛЭРС УЧЁТ.

					user.Context = new LersContext
					{
						Login = context.Login,
						Password = context.Password
					};

					try
					{
						// Проверяем подключение и выводим приветствие.

						user.Connect();

						bot.SendText(user.ChatId, $"Добро пожаловать,  {user.Context.Server.Accounts.Current.DisplayName}");
					}
					catch
					{
						user.Context = null;
						throw;
					}
				}

				User.Save();
			}
		}

		private void ShowCurrents(User user, string[] arguments)
		{
			if (user.Context == null)
				throw new UnauthorizedCommandException(GetCurrentsCommand);

			LersServer server = user.Context.Server;
			long chatId = user.ChatId;

			var measurePoint = server.GetMeasurePoints(arguments).FirstOrDefault();

			if (measurePoint == null)
			{
				bot.SendText(chatId, "Точка учёта не найдена");
				return;
			}

			bot.SendText(chatId, $"Точка учёта {measurePoint.FullTitle}");

			var options = new MeasurePointPollCurrentOptions { StartMode = Lers.Common.PollManualStartMode.Force };

			int pollSessionId = measurePoint.PollCurrent(options);

			bot.SendText(chatId, "Запущен опрос");

			var autoResetEvent = new System.Threading.AutoResetEvent(false);

			MeasurePointData.CurrentsSaved += (sender, e) =>
			{
				try
				{
					SendCurrents(chatId, e.Consumption);

					autoResetEvent.Set();
				}
				catch (Exception exc)
				{
					bot.SendText(chatId, exc.Message);
				}
			};

			MeasurePointData.SubscribeSaveCurrents(server, pollSessionId);

			if (!autoResetEvent.WaitOne(120000))
			{
				bot.SendText(chatId, "Не удалось получить текущие данные за 2 минуты.");
			}

			MeasurePointData.UnsubscribeSaveCurrents(server);
		}

		private void SendCurrents(long chatId, MeasurePointConsumptionRecord record)
		{
			var valueNames = new List<string>();
			var valueDisplayNames = new List<string>();

			switch (record.ResourceKind)
			{
				case ResourceKind.Water:
					var waterValues = Enum.GetValues(typeof(WaterRecordValues)).Cast<WaterRecordValues>().Where(v => v != WaterRecordValues.All && v != WaterRecordValues.None).ToList();
					waterValues.ForEach(v => valueNames.Add(v.ToString()));
					break;

				case ResourceKind.Gas:
					var gasValues = Enum.GetValues(typeof(GasRecordValues)).Cast<GasRecordValues>().Where(v => v != GasRecordValues.All && v != GasRecordValues.None).ToList();

					gasValues.ForEach(v => valueNames.Add(v.ToString()));

					break;

				case ResourceKind.Electricity:
					var electricValues = Enum.GetValues(typeof(ElectricCurrentsRecordValues)).Cast<ElectricCurrentsRecordValues>().Where(v => v != ElectricCurrentsRecordValues.All && v != ElectricCurrentsRecordValues.None).ToList();

					electricValues.ForEach(v => valueNames.Add(v.ToString()));

					break;

				default: throw new InvalidOperationException("Неверный тип ресурса");
			}

			var sb = new StringBuilder();

			for (int i = 0; i < valueNames.Count; ++i)
			{
				double? val = record.GetValue(valueNames[i]);

				if (val.HasValue)
				{
					string text = $"{valueNames[i]} = {val.Value:f2}";

					sb.AppendLine(text);
				}
			}

			bot.SendText(chatId, System.Web.HttpUtility.HtmlEncode(sb.ToString()));
		}

		private void ShowNodes(User user, string[] arguments)
		{
			if (user.Context == null)
				throw new UnauthorizedCommandException(GetNodesCommand);

			var nodes = user.Context.Server.GetNodes(arguments);

			long chatId = user.ChatId;

			if (!nodes.Any())
			{
				bot.SendText(chatId, "Ничего не найдено");
				return;
			}

			try
			{
				foreach (var node in nodes)
				{
					bot.SendText(chatId, node.Title);
				}
			}
			catch (AggregateException ae)
			{
				foreach (var e in ae.InnerExceptions)
				{
					Logger.LogError(e.Message);
				}
			}
		}

		private void ShowMeasurePoints(User user, string[] arguments)
		{
			if (user.Context == null)
				throw new UnauthorizedCommandException(GetMeasurePointsCommand);

			var measurePoints = user.Context.Server.GetMeasurePoints(arguments);

			long chatId = user.ChatId;

			if (!measurePoints.Any())
			{
				bot.SendText(chatId, "Ничего не найдено");
				return;
			}

			try
			{
				foreach (var mp in measurePoints)
				{
					bot.SendText(chatId, mp.FullTitle);
				}
			}
			catch (AggregateException ae)
			{
				foreach (var e in ae.InnerExceptions)
				{
					Logger.LogError(e.Message);
				}
			}
		}
	}
}
