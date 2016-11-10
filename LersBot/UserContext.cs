using System;

namespace LersBot
{
	class UserContext
	{
		public string UserName;

		public long ChatId;

		/// <summary>
		/// Подключение к серверу ЛЭРС УЧЁТ, связанное с пользователем.
		/// </summary>
		internal Lers.LersServer Server;

		/// <summary>
		/// Дата последнего отправленного уведомления.
		/// </summary>
		public DateTime LastNotificationDate;

		/// <summary>
		/// Идентификатор последнего отправленного уведомления.
		/// </summary>
		public int LastNotificationId;

		/// <summary>
		/// Признак включения или отключения отправки уведомлений из центра.
		/// </summary>
		public bool SendNotifications = true;
	}
}
