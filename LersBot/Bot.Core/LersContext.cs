using System;
using System.Net.Http;

namespace LersBot.Bot.Core
{
	/// <summary>
	/// Контекст пользователя ЛЭРС УЧЁТ, связываемый с пользователем Telegram.
	/// </summary>
	public class LersContext : IDisposable
	{
		/// <summary>
		/// Имя пользователя ЛЭРС УЧЁТ.
		/// </summary>
		public string Login { get; set; }

		/// <summary>
		/// Пароль на сервере ЛЭРС УЧЁТ.
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// Подключение к серверу ЛЭРС УЧЁТ, связанное с пользователем.
		/// </summary>
		public HttpClient RestClient { get; } = new HttpClient();

		/// <summary>
		/// Базовый URI сервера.
		/// </summary>
		public Uri BaseUri
		{
			get => RestClient.BaseAddress;
			set => RestClient.BaseAddress = value;
		}
		
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

		/// <summary>
		/// Токен авторизации на сервере.
		/// </summary>
		public string Token { get; set; }

		public LersContext(Uri baseUri)
		{
			BaseUri = baseUri;
		}

		public void Dispose() => RestClient.Dispose();
	}
}
