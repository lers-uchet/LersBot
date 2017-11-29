using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lers.Administration;

namespace LersBot
{
	/// <summary>
	/// Класс, отправляющий уведомления клиентам.
	/// </summary>
	class Notifier
	{
		private LersBot bot;

		private BackgroundWorker notifyThread = new BackgroundWorker();

		private CancellationTokenSource stopToken = new CancellationTokenSource();

		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


		public Notifier(LersBot bot)
		{
			this.bot = bot;
			this.notifyThread.DoWork += NotifyThread_DoWork;
		}

		private async void NotifyThread_DoWork(object sender, DoWorkEventArgs e)
		{
			await CheckNotifications();

			while (!this.stopToken.IsCancellationRequested)
			{
				try
				{
					// Проверка запускается каждые 60 секунд
					await Task.Delay(60000, this.stopToken.Token);

					await CheckNotifications();
				}
				catch (OperationCanceledException)
				{
					return;
				}
			}
		}

		private async Task CheckNotifications()
		{
			// Проходим по всем зарегистрированным пользователям.

			var userList = User.Where(x => x.Context != null);

			foreach (var user in userList)
			{
				try
				{
					if (!user.Context.SendNotifications)
					{
						// Пользователь ещё не начал чат с ботом или отключил уведомления.
						continue;
					}

					user.Connect();

					await CheckUserNotifications(user);
				}
				catch (Exception exc)
				{
					logger.Error(exc, "Ошибка проверки уведомлений пользователя. ");
				}
			}

			User.Save();
		}

		private async Task CheckUserNotifications(User user)
		{
			if (!AccountReceivesNotificationsNow(user.Context.Server.Accounts.Current))
			{
				return;
			}

			var notifications = (await user.Context.Server.Notifications.GetListAsync()).OrderBy(x => x.Id);

			if (!notifications.Any())
			{
				return;
			}

			// Сохраним дату самого нового сообщения
			Lers.Notification lastNotify = notifications.OrderBy(x => x.Id).Last();

			// При первом запуске уведомления не рассылаем.
			try
			{
				if (user.Context.LastNotificationId != 0)
				{
					foreach (var notification in notifications)
					{
						if (notification.Id > user.Context.LastNotificationId)
						{
							string text = "";

							switch (notification.Importance)
							{
								case Lers.Importance.Warn:
									text += Emoji.Warning;
									break;

								case Lers.Importance.Error:
									text += Emoji.StopSign;
									break;

								default:
									text += Emoji.InformationSource;
									break;
							}

							text += " " + notification.Message;

							if (!string.IsNullOrEmpty(notification.Url))
							{
								text += $"\r\n{notification.Url}";
							}

							await bot.SendTextAsync(user.ChatId, text);

							// Сохраним последнее отправленное сообщение.
							lastNotify = notification;
						}
					}
				}
			}
			finally
			{
				// Сохраним в контекст пользователя информацию о последнем отправленном сообщении.
				user.Context.LastNotificationId = lastNotify.Id;
				user.Context.LastNotificationDate = lastNotify.DateTime;
			}
		}

		private static bool AccountReceivesNotificationsNow(Account current)
		{
			DateTime dtNow = DateTime.Now;

			int nowTime = dtNow.Hour * 60 + dtNow.Minute;

			return nowTime >= current.NotifyStartTime && nowTime <= current.NotifyEndTime;
		}

		internal void Start()
		{
			this.notifyThread.RunWorkerAsync();
		}

		internal void Stop() => this.stopToken.Cancel();

		internal void ProcessSetNotifyOn(User user, string[] arguments)
		{
			if (user.Context == null)
				throw new UnauthorizedCommandException(LersBotService.SetNotifyOnCommand);

			user.Context.SendNotifications = true;

			bot.SendText(user.ChatId, "Отправка уведомлений включена.");

			User.Save();
		}

		internal void ProcessSetNotifyOff(User user, string[] arguments)
		{
			if (user.Context == null)
				throw new UnauthorizedCommandException(LersBotService.SetNotifyOffCommand);

			user.Context.SendNotifications = false;

			bot.SendText(user.ChatId, "Отправка уведомлений выключена.");

			User.Save();
		}
	}
}
