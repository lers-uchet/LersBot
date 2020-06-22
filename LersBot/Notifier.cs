using LersBot.Bot.Core;
using NLog.LayoutRenderers;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Requests;
/*using Lers.Administration;*/

namespace LersBot
{
	/// <summary>
	/// Класс, отправляющий уведомления клиентам.
	/// </summary>
	public class Notifier
	{
		private LersBot _bot;
		private readonly UsersService _users;
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public Notifier(UsersService users, LersBot bot)
		{
			_bot = bot;
			_users = users;
		}


		private async Task CheckNotifications()
		{
			// Проходим по всем зарегистрированным пользователям.

			var userList = _users.Where(x => x.Context != null);

			foreach (var user in userList)
			{
				try
				{
					if (!user.Context.SendNotifications)
					{
						// Пользователь ещё не начал чат с ботом или отключил уведомления.
						continue;
					}
					
					await CheckUserNotifications(user);
				}
				catch (Exception exc)
				{
					logger.Error(exc, "Ошибка проверки уведомлений пользователя. ");
				}
			}

			_users.Save();
		}

		private async Task CheckUserNotifications(User user)
		{
			if (!AccountReceivesNotificationsNow(user.Current))
			{
				return;
			}

			var notificationsClient = new Lers.Rest.NotificationsClient(user.Context.BaseUri.ToString(),
				user.Context.RestClient);

			var endDate = DateTimeOffset.Now;
			var startDate = endDate.AddDays(-30);

			var response = await notificationsClient.GetNotificationsForPeriodAsync(startDate, endDate);

			var notifications = response.Notifications.OrderBy(x => x.NotificationId);

			if (!notifications.Any())
			{
				return;
			}

			// Сохраним дату самого нового сообщения
			Lers.Rest.Notification lastNotify = notifications.OrderBy(x => x.NotificationId).Last().Notification;

			// При первом запуске уведомления не рассылаем.

			try
			{
				if (user.Context.LastNotificationId != 0)
				{
					foreach (var notification in notifications)
					{
						if (notification.NotificationId > user.Context.LastNotificationId)
						{
							string text = "";

							text += notification.Notification.Importance switch
							{
								Lers.Rest.Importance.Warn => Emoji.Warning,
								Lers.Rest.Importance.Error => Emoji.StopSign,
								_ => Emoji.InformationSource,
							};
							text += " " + notification.Notification.Message;

							if (!string.IsNullOrEmpty(notification.Notification.Url))
							{
								text += $"\r\n{notification.Notification.Url}";
							}

							await _bot.SendTextAsync(user.ChatId, text);

							// Сохраним последнее отправленное сообщение.
							lastNotify = notification.Notification;
						}
					}
				}
			}
			finally
			{
				// Сохраним в контекст пользователя информацию о последнем отправленном сообщении.
				user.Context.LastNotificationId = lastNotify.Id;
				user.Context.LastNotificationDate = lastNotify.DateTime.DateTime;
			}
		}

		private static bool AccountReceivesNotificationsNow(Lers.Rest.Account current)
		{
			DateTime dtNow = DateTime.Now;

			int nowTime = dtNow.Hour * 60 + dtNow.Minute;

			return nowTime >= current.NotifyStartTime && nowTime <= current.NotifyEndTime;
		}

		internal async Task Start(CancellationToken stoppingToken)
		{
			await CheckNotifications();

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					// Проверка запускается каждые 60 секунд

					await Task.Delay(60000, stoppingToken);

					await CheckNotifications();
				}
				catch (OperationCanceledException)
				{
					return;
				}
			}
		}
		
		internal Task ProcessSetNotifyOn(User user, string[] arguments)
		{
			if (user.Context == null)
				throw new UnauthorizedCommandException(LersBotService.SetNotifyOnCommand);

			user.Context.SendNotifications = true;

			_bot.SendText(user.ChatId, "Отправка уведомлений включена.");

			_users.Save();

			return Task.CompletedTask;
		}

		internal Task ProcessSetNotifyOff(User user, string[] arguments)
		{
			if (user.Context == null)
				throw new UnauthorizedCommandException(LersBotService.SetNotifyOffCommand);

			user.Context.SendNotifications = false;

			_bot.SendText(user.ChatId, "Отправка уведомлений выключена.");

			_users.Save();

			return Task.CompletedTask;
		}
	}
}
