using System;

namespace LersBot.Bot.Core
{
	/// <summary>
	/// Контекст пользователя ЛЭРС УЧЁТ, связываемый с пользователем Telegram.
	/// </summary>
	class LersContext
	{
		/// <summary>
		/// Имя пользователя ЛЭРС УЧЁТ.
		/// </summary>
		public string Login { get; set; }

		/// <summary>
		/// Пароль на сервере ЛЭРС УЧЁТ.
		/// </summary>
		public string Password { get; set; }

		/*/// <summary>
		/// Подключение к серверу ЛЭРС УЧЁТ, связанное с пользователем.
		/// </summary>
		internal Lers.LersServer Server;*/

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
