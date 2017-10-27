using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private LersBot bot;

		private Notifier notifier;

		private const string StartCommand = "/start";
		private const string GetCurrentsCommand = "/getcurrents";
		private const string GetNodesCommand = "/nodes";
		private const string GetMeasurePointsCommand = "/mpts";
		public const string SetNotifyOnCommand = "/setnotify_on";
		public const string SetNotifyOffCommand = "/setnotify_off";
		public const string SystemStateReport = "/sysstate";
		public const string PortStatus = "/portstatus";
		public const string GetMyJobsCommand = "/getmyjobs";

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
			logger.Info(
				$"\r\n==========================================\r\n"
				+ "=== Загрузка бота Telegram для сервера ЛЭРС УЧЁТ...\r\n"
				+ "========================================== ");

			try
			{
				Config.Load();
				User.LoadList();

				bot = new LersBot();

				notifier = new Notifier(bot);

				logger.Info($"Starting {bot.UserName}");

				bot.AddCommandHandler(HandleStart, StartCommand);
				bot.AddCommandHandler(ShowCurrents, GetCurrentsCommand);
				bot.AddCommandHandler(ShowNodes, GetNodesCommand);
				bot.AddCommandHandler(ShowMeasurePoints, GetMeasurePointsCommand);
				bot.AddCommandHandler(notifier.ProcessSetNotifyOn, SetNotifyOnCommand);
				bot.AddCommandHandler(notifier.ProcessSetNotifyOff, SetNotifyOffCommand);
				bot.AddCommandHandler(SendSystemStateReport, SystemStateReport);
				bot.AddCommandHandler(SendPortStatus, PortStatus);
				bot.AddCommandHandler(GetMyJobs, GetMyJobsCommand);

				bot.Start();

				notifier.Start();
			}
			catch (Exception exc)
			{
				logger.Info(exc, "Ошибка запуска бота.");

				throw;
			}
		}

		/// <summary>
		/// Вызывается при остановке службы.
		/// </summary>
		protected override void OnStop()
		{
			notifier.Stop();

			logger.Info($"Stopped {bot.UserName}");
		}

		/// <summary>
		/// Обрабатывает команду начала сеанса работы пользователя с ботом.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="arguments"></param>
		[Authorize(false)]
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

					user.Context = new LersContext { Login = context.Login, Password = context.Password };

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

			EventHandler<MeasurePointConsumptionEventArgs> handler = (sender, e) =>
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

			MeasurePointData.CurrentsSaved += handler;

			MeasurePointData.SubscribeSaveCurrents(server, pollSessionId);

			if (!autoResetEvent.WaitOne(120000))
			{
				bot.SendText(chatId, "Не удалось получить текущие данные за 2 минуты.");
			}

			MeasurePointData.UnsubscribeSaveCurrents(server);

			MeasurePointData.CurrentsSaved -= handler;
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
				SendListMessage(chatId, nodes.OrderBy(x => x.Title), x => x.Title);
			}
			catch (AggregateException ae)
			{
				foreach (var e in ae.InnerExceptions)
				{
					logger.Error(e.Message);
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
				SendListMessage(chatId, measurePoints.OrderBy(x => x.FullTitle), x => x.FullTitle);
			}
			catch (AggregateException ae)
			{
				foreach (var e in ae.InnerExceptions)
				{
					logger.Error(e.Message);
				}
			}
		}

		private void SendListMessage<T>(long chatId, IEnumerable<T> list, Func<T, string> textSelector)
		{
			if (list == null)
				throw new ArgumentNullException(nameof(list));

			if (!list.Any())
				throw new InvalidOperationException("Список не содержит элементов.");

			if (textSelector == null)
				throw new ArgumentNullException(nameof(textSelector));

			// Максимальная длина сообщения - 4096 UTF8 символов.
			// https://core.telegram.org/method/messages.sendMessage

			// Отправляем список блоками, не превышающими максимальную длину.

			StringBuilder sb = new StringBuilder(4096);

			foreach (var item in list)
			{
				string addition = (sb.Length != 0) ? "\r\n" + textSelector(item) : textSelector(item);

				if (sb.Length + addition.Length > 4096)
				{
					// Максимальная длина достигнута - отправляем сообщение.

					bot.SendText(chatId, sb.ToString());

					sb.Clear();
					sb.Append(textSelector(item));
				}
				else
				{
					sb.Append(addition);
				}
			}

			// Отправляем оставшийся текст или весь текст, если длина не была превышена.

			bot.SendText(chatId, sb.ToString());
		}

		/// <summary>
		/// Отправляет пользователю отчёт о состоянии системы.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="args"></param>
		private void SendSystemStateReport(User user, string[] args)
		{
			if (user.Context == null)
				throw new UnauthorizedCommandException(SystemStateReport);

			var reportManager = new Lers.Reports.ReportManager(user.Context.Server);

			// Получим системный отчёт о состоянии системы
			var report = reportManager.GetReportListAllowed().Where(r => r.Type == Lers.Reports.ReportType.SystemState && r.IsSystem).FirstOrDefault();

			if (report == null)
			{
				throw new BotException("Отчёт о состоянии системы не найден на сервере");
			}

			// Формируем отчёт
			var preparedReport = reportManager.GenerateSystemStateReport(report.Id);

			using (var stream = new MemoryStream(1024 * 1024))
			{
				// Экспортируем отчёт в PDF и отправляем пользователю.

				preparedReport.ExportToPdf(stream);

				bot.SendDocument(user.ChatId, stream, $"Отчёт о состоянии системы от {DateTime.Now}", "SystemStateReport.pdf");
			}
		}

		[Authorize(true)]
		private void SendPortStatus(User user, string[] args)
		{
			var status = user.Context.Server.PollPorts.GetPortStatus();

			var sb = new StringBuilder();
			sb.AppendLine($"Служб опроса: {status.PollServices}");
			sb.AppendLine($"Портов опроса: {status.Total}");
			sb.AppendLine($"Активных портов: {status.Active}");
			sb.AppendLine($"Свободных портов: {status.Free}");
			sb.AppendLine($"Заблокированных портов: {status.Blocked}");

			bot.SendText(user.ChatId, sb.ToString());
		}

		/// <summary>
		/// Обрабатывает запрос работ на объектах учёта.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="args"></param>
		[Authorize(true)]
		private void GetMyJobs(User user, string[] args)
		{
			if (user.Context == null)
				throw new UnauthorizedCommandException(SystemStateReport);

			var server = user.Context.Server;

			var currentAccount = server.Accounts.Current;

			// Запрашиваем список невыполненных работ на объектах

			var nodeJobs = server.NodeJobs
				.GetListAsync()
				.Result
				.Where(x => x.PerformerAccount?.Id == currentAccount.Id && x.State != NodeJobState.Completed);

			var sb = new StringBuilder();

			foreach (var job in nodeJobs)
			{
				sb.AppendLine($"Задание: {job.Title}");
				sb.AppendLine($"Объект учёта: {job.Node.Title}");

				var today = DateTime.Today;

				if (job.ScheduledEndDate < today)
				{
					int overdue = (int)(today - job.ScheduledEndDate.Date).TotalDays;

					sb.AppendLine($"{Emoji.Warning}Просрочено на {overdue} дн.");
				}
				else
				{
					int dueDays = (int)(job.ScheduledEndDate.Date - today).TotalDays;

					string due;

					if (dueDays == 0)
					{
						due = "сегодня";
					}
					else
					{
						due = $"через {dueDays}дн.";
					}

					sb.AppendLine($"Срок выполнения: {due}");
				}

				sb.AppendLine();
				sb.AppendLine();
			}

			if (sb.Length == 0)
			{
				sb.AppendLine("Нет невыполненных работ.");
			}

			bot.SendText(user.ChatId, sb.ToString());
		}
	}
}
