using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lers.Administration;

namespace LersBot
{
	class Notifier
	{
		private LersBot bot;

		private BackgroundWorker notifyThread = new BackgroundWorker();

		private AutoResetEvent stopEvent = new AutoResetEvent(false);

		public Notifier(LersBot bot)
		{
			this.bot = bot;
			this.notifyThread.DoWork += NotifyThread_DoWork;
		}

		private void NotifyThread_DoWork(object sender, DoWorkEventArgs e)
		{
			CheckNotifications();

			while (!this.stopEvent.WaitOne(60000))
			{
				// Проверка запускается каждые 60 секунд
				CheckNotifications();
			}
		}

		private void CheckNotifications()
		{
			// Проходим по всем зарегистрированным пользователям.

			foreach (var user in Config.Instance.Users)
			{
				try
				{
					if (!user.Context.SendNotifications || user.Context.ChatId == 0)
					{
						// Пользователь ещё не начал чат с ботом или отключил уведомления.
						continue;
					}

					user.Connect();

					CheckUserNotifications(user);
				}
				catch (Exception exc)
				{
					Console.WriteLine(exc.Message);
				}
			}

			Config.SaveContexts();
		}

		private void CheckUserNotifications(UserName user)
		{
			if (!AccountReceivesNotificationsNow(user.Context.Server.Accounts.Current))
			{
				return;
			}

			var notifications = user.Context.Server.Notifications.GetList().OrderBy(x => x.Id);

			if (!notifications.Any())
			{
				return;
			}

			// При первом запуске уведомления не рассылаем.

			if (user.Context.LastNotificationId != 0)
			{
				foreach (var notification in notifications)
				{
					if (notification.Id > user.Context.LastNotificationId)
					{
						string text = "";

						switch (notification.Type)
						{
							case Lers.NotificationType.CriticalError:
								text += "🚫 ";
								break;

							case Lers.NotificationType.Incident:
							case Lers.NotificationType.EquipmentCalibrationRequired:
								text += "⚠️ ";
								break;

							default:
								text += "ℹ️ ";
								break;
						}

						text += notification.Message;

						if (!string.IsNullOrEmpty(notification.Url))
						{
							text += $"\r\n{notification.Url}";
						}

						bot.SendText(user.Context.ChatId, text);
					}
				}
			}

			// Сохраним дату самого нового сообщения

			Lers.Notification lastNotify = notifications.OrderBy(x => x.Id).Last();

			user.Context.LastNotificationId = lastNotify.Id;
			user.Context.LastNotificationDate = lastNotify.DateTime;
		}

		private static bool AccountReceivesNotificationsNow(Account current)
		{
			DateTime dtNow = DateTime.Now;

			int nowTime = dtNow.Hour * 60 + dtNow.Minute;

			return nowTime >- current.NotifyStartTime && nowTime <= current.NotifyEndTime;
		}

		internal void Start()
		{
			this.notifyThread.RunWorkerAsync();
		}

		internal void Stop()
		{
			this.stopEvent.Set();
		}

		internal void ProcessSetNotify(UserName user, string[] arguments)
		{
			if (arguments.Length == 0 || arguments[0].ToLower() == "on")
			{
				user.Context.SendNotifications = true;

				bot.SendText(user.Context.ChatId, "Отправка уведомлений включена.");
			}
			else if (arguments[0] == "off")
			{
				user.Context.SendNotifications = false;

				bot.SendText(user.Context.ChatId, "Отправка уведомлений отключена.");
			}
			else
			{
				bot.SendText(user.Context.ChatId, "Неверные параметры команды.");
			}

			Config.SaveContexts();
		}
	}
}
