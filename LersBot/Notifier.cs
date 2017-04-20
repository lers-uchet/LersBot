﻿using System;
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

		private AutoResetEvent stopEvent = new AutoResetEvent(false);

		public Notifier(LersBot bot)
		{
			this.bot = bot;
			this.notifyThread.DoWork += NotifyThread_DoWork;
		}

		private async void NotifyThread_DoWork(object sender, DoWorkEventArgs e)
		{
			await CheckNotifications();

			while (!this.stopEvent.WaitOne(60000))
			{
				// Проверка запускается каждые 60 секунд
				await CheckNotifications();
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
					Logger.LogError("Ошибка проверки уведомлений пользователя. " + exc.ToString());
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
								text += Emoji.StopSign;
								break;

							case Lers.NotificationType.Incident:
							case Lers.NotificationType.EquipmentCalibrationRequired:
								text += Emoji.Warning;
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

			return nowTime >= current.NotifyStartTime && nowTime <= current.NotifyEndTime;
		}

		internal void Start()
		{
			this.notifyThread.RunWorkerAsync();
		}

		internal void Stop()
		{
			this.stopEvent.Set();
		}

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
