using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lers.Utils;
using LersBot.Bot.Core;
/*using Lers;
using Lers.Core;
using Lers.Data;*/
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace LersBot
{
	/// <summary>
	/// Windows-служба бота LersBot.
	/// </summary>
	public class LersBotService : BackgroundService
	{
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly LersBot _bot;
		private readonly Notifier notifier;
		private readonly IHostApplicationLifetime _lifeTime;
		private readonly UsersService _users;
		private readonly Config _config;
		private const string StartCommand = "/start";
		private const string GetCurrentsCommand = "/getcurrents";
		private const string GetNodesCommand = "/nodes";
		private const string GetMeasurePointsCommand = "/mpts";
		public const string SetNotifyOnCommand = "/setnotify_on";
		public const string SetNotifyOffCommand = "/setnotify_off";
		public const string SystemStateReport = "/sysstate";
		public const string PortStatus = "/portstatus";
		public const string GetMyJobsCommand = "/getmyjobs";

		public LersBotService(IHostApplicationLifetime lifeTime,
			UsersService users,
			IOptionsSnapshot<Config> config,
			LersBot bot)
		{
			_bot = bot;
			_lifeTime = lifeTime;
			_users = users;
			_config = config.Value;
		}


		/// <summary>
		/// Обрабатывает команду начала сеанса работы пользователя с ботом.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="arguments"></param>
		[Authorize(false)]
		private async Task HandleStart(User user, string[] arguments)
		{
			if (user.CommandContext == null)
			{
				// Начато выполнение команды. Создаём контекст команды и запрашиваем логин на сервере.

				user.CommandContext = new StartCommandContext(StartCommand);

				await _bot.SendText(user.ChatId, "Введите логин на сервере ЛЭРС УЧЁТ.");
			}
			else
			{
				// Команда уже выполняется.

				var context = (StartCommandContext)user.CommandContext;

				if (string.IsNullOrEmpty(context.Login))
				{
					// Логин ещё пустой, значит его передал пользователь. Сохраняем и запрашиваем пароль.

					context.Login = arguments[0];

					await _bot.SendText(user.ChatId, "Введите пароль на сервере ЛЭРС УЧЁТ");
				}
				else if (string.IsNullOrEmpty(context.Password))
				{
					// Если пароля ещё нет, значит он был передан сейчас. Сохраняем пароль и очищаем контекст команды.

					context.Password = arguments[0];

					user.CommandContext = null;

					// Создаём контекст пользователя ЛЭРС УЧЁТ.

					var uriBuilder = new UriBuilder
					{
						Host = _config.LersServerAddress,
						Port = _config.LersServerPort,
						Scheme = "http"
					};

					user.Context = new LersContext(uriBuilder.Uri);

					try
					{
						// Проверяем подключение и выводим приветствие.

						await user.Connect(context.Login, context.Password);

						await _bot.SendText(user.ChatId, $"Добро пожаловать,  {user.Current.DisplayName}");
					}
					catch (Exception exc)
					{
						logger.Error(exc, "Ошибка подключения к серверу ЛЭРС УЧЁТ");
						user.Context = null;
						_users.Remove(user);
						throw;
					}
				}

				_users.Save();
			}
		}

		
		private async Task ShowCurrents(User user, string[] arguments)
		{
			if (user.Context == null)
				throw new UnauthorizedCommandException(GetCurrentsCommand);

			var measurePointsClient = new Lers.Rest.MeasurePointsClient(user.Context.BaseUri.ToString(),
				user.Context.RestClient);

			var manualPollClient = new Lers.Rest.ManualPollClient(user.Context.BaseUri.ToString(),
				user.Context.RestClient);			

			long chatId = user.ChatId;

			var measurePoint = (await measurePointsClient.GetMeasurePoints(arguments))
				.FirstOrDefault();

			if (measurePoint == null)
			{
				await _bot.SendText(chatId, "Точка учёта не найдена");
				return;
			}

			await _bot.SendText(chatId, $"Точка учёта {measurePoint.FullTitle}");

			await manualPollClient.PollArchiveAsync(DateTimeOffset.Now, DateTimeOffset.Now, new Lers.Rest.PollArchiveRequestParameters
			{
				AbsentDataOnly = false,
				RequestedDataMask = Lers.Rest.DeviceDataType.Current,
				MeasurePoints = new int[] { measurePoint.Id },
				StartMode = Lers.Rest.PollManualStartMode.Force
			});
			
			/*
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

			MeasurePointData.CurrentsSaved -= handler;*/
		}

		/*
		
		/// <summary>
		/// Отправляет список полученных текущих данных.
		/// </summary>
		/// <param name="chatId"></param>
		/// <param name="record"></param>
		private void SendCurrents(long chatId, MeasurePointConsumptionRecord record)
		{
			var sb = new StringBuilder();

			foreach (var parameter in record)
			{
				if (parameter.Value != null)
				{
					var desc = DataParameterDescriptor.Get(parameter.Key);

					string text = $"{desc.ShortTitle} = {parameter.Value.Value:f2}";

					sb.AppendLine(text);
				}
			}

			bot.SendText(chatId, System.Web.HttpUtility.HtmlEncode(sb.ToString()));
		}*/

		/// <summary>
		/// Отправляет список объектов учёта.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		private async Task ShowNodes(User user, string[] arguments)
		{
			if (user.Context == null)
				throw new UnauthorizedCommandException(GetNodesCommand);

			var client = new Lers.Rest.NodesClient(user.Context.BaseUri.ToString(), 
				user.Context.RestClient);

			var nodes = await client.GetNodes(arguments);

			long chatId = user.ChatId;

			if (!nodes.Any())
			{
				await _bot.SendText(chatId, "Ничего не найдено");
				return;
			}

			var comparer = new NaturalSortComparer();
			await SendListMessage(chatId, nodes.OrderBy(x => x.Title, comparer), x => x.Title);
		}

		private async Task ShowMeasurePoints(User user, string[] arguments)
		{
			if (user.Context == null)
				throw new UnauthorizedCommandException(GetMeasurePointsCommand);

			var client = new Lers.Rest.MeasurePointsClient(user.Context.BaseUri.ToString(), user.Context.RestClient);
			var measurePoints = await client.GetMeasurePoints(arguments);

			long chatId = user.ChatId;

			if (!measurePoints.Any())
			{
				await _bot.SendText(chatId, "Ничего не найдено");
				return;
			}

			await SendListMessage(chatId, measurePoints.OrderBy(x => x.FullTitle, new NaturalSortComparer()), x => x.FullTitle);
		}

		private async Task SendListMessage<T>(long chatId, IEnumerable<T> list, Func<T, string> textSelector)
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

			var sb = new StringBuilder(4096);

			foreach (var item in list)
			{
				string addition = (sb.Length != 0) ? "\r\n" + textSelector(item) : textSelector(item);

				if (sb.Length + addition.Length > 4096)
				{
					// Максимальная длина достигнута - отправляем сообщение.

					await _bot.SendText(chatId, sb.ToString());

					sb.Clear();
					sb.Append(textSelector(item));
				}
				else
				{
					sb.Append(addition);
				}
			}

			// Отправляем оставшийся текст или весь текст, если длина не была превышена.

			await _bot.SendText(chatId, sb.ToString());
		}

		/// <summary>
		/// Отправляет пользователю отчёт о состоянии системы.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="args"></param>
		private async Task SendSystemStateReport(User user, string[] args)
		{
			if (user.Context == null)
				throw new UnauthorizedCommandException(SystemStateReport);

			var reportsClient = new Lers.Rest.ReportsClient(user.Context.BaseUri.ToString(),
				user.Context.RestClient);

			var generateClient = new Lers.Rest.GenerateClient(user.Context.BaseUri.ToString(),
				user.Context.RestClient);

			// Получим системный отчёт о состоянии системы
			var report = (await reportsClient.GetReportsAsync(Lers.Rest.ReportType.SystemState, null))
				.FirstOrDefault();

			if (report == null)
			{
				throw new BotException("Отчёт о состоянии системы не найден на сервере");
			}

			// Формируем отчёт.
			var generated = await generateClient.GenerateExportedAsync(report.Id, false, new Lers.Rest.GenerateExportedReportRequestParameters
			{
				GenerateOptions = new Lers.Rest.BaseGenerateReportRequestParameters
				{
					StartDate = DateTime.Today,
					EndDate = DateTime.Today
				},
				ExportOptions = new Lers.Rest.ExportOptions
				{
					ExportFormat = Lers.Rest.ReportExportFormat.Pdf
				}
			});

			// Формируем ссылку для загрузки и качаем документ.
			
			var file = await user.Context.RestClient.GetAsync($"api/v0.1/downloads/{generated.DownloadKey}");

			if (!file.IsSuccessStatusCode)
			{
				throw new BotException("Не удалось загрузить сформированный отчёт. " + file.ReasonPhrase);
			}

			using var contentStream = await file.Content.ReadAsStreamAsync();

			await _bot.SendDocument(user.ChatId, contentStream, generated.FileName, $"Отчёт о состоянии системы от {DateTime.Now}");
		}

		[Authorize(true)]
		private async Task SendPortStatus(User user, string[] args)
		{
			var portsClient = new Lers.Rest.PollPortsClient(user.Context.BaseUri.ToString(),
				user.Context.RestClient);

			var status = await portsClient.GetStatusAsync();

			var sb = new StringBuilder();
			sb.AppendLine($"Служб опроса: {status.PollServices}");
			sb.AppendLine($"Портов опроса: {status.Total}");
			sb.AppendLine($"Активных портов: {status.Active}");
			sb.AppendLine($"Свободных портов: {status.Free}");
			sb.AppendLine($"Заблокированных портов: {status.Blocked}");

			await _bot.SendText(user.ChatId, sb.ToString());
		}

		/// <summary>
		/// Обрабатывает запрос работ на объектах учёта.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="args"></param>
		[Authorize(true)]
		private async Task GetMyJobs(User user, string[] args)
		{
			if (user.Context == null)
				throw new UnauthorizedCommandException(SystemStateReport);

			var jobsClient = new Lers.Rest.NodeJobsClient(user.Context.BaseUri.ToString(), user.Context.RestClient);

			// Запрашиваем список невыполненных работ на объектах
			var response = await jobsClient.GetListAsync(true);

			var nodeJobs = response.NodeJobList
				.Where(x => x.PerformerAccountId == user.Current.Id && x.State != Lers.Rest.NodeJobState.Completed);

			var sb = new StringBuilder();

			foreach (var job in nodeJobs)
			{
				var jobNode = response.Nodes[job.NodeId.ToString()];

				sb.AppendLine($"Задание: {job.Title}");				
				sb.AppendLine($"Объект учёта: {jobNode.Title} ({jobNode.Address})");

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

			await _bot.SendText(user.ChatId, sb.ToString());
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			logger.Info($"\r\n==========================================\r\n"
						+ "=== Загрузка бота Telegram для сервера ЛЭРС УЧЁТ...\r\n"
						+ "========================================== ");

			_lifeTime.ApplicationStopping.Register(() =>
			{
				/*notifier.Stop();*/

				logger.Info("Bot stopped.");
			});

			
			try
			{
				/*notifier = new Notifier(bot);
				*/

				logger.Info($"Starting bot.");

				_bot.AddCommandHandler(HandleStart, StartCommand);
				_bot.AddCommandHandler(ShowCurrents, GetCurrentsCommand);
				_bot.AddCommandHandler(ShowNodes, GetNodesCommand);
				_bot.AddCommandHandler(ShowMeasurePoints, GetMeasurePointsCommand);/*
				bot.AddCommandHandler(notifier.ProcessSetNotifyOn, SetNotifyOnCommand);
				bot.AddCommandHandler(notifier.ProcessSetNotifyOff, SetNotifyOffCommand);*/
				_bot.AddCommandHandler(SendSystemStateReport, SystemStateReport);
				_bot.AddCommandHandler(SendPortStatus, PortStatus);
				_bot.AddCommandHandler(GetMyJobs, GetMyJobsCommand);
				
				await _bot.Start();

				/*notifier.Start();*/
			}
			catch (Exception exc)
			{
				logger.Info(exc, "Ошибка запуска бота.");

				throw;
			}
		}
	}
}
