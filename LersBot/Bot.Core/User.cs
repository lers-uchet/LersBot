using System;

namespace LersBot
{
	/// <summary>
	/// Пользователь бота.
	/// </summary>
	class User
	{
		/// <summary>
		/// Идентификатор пользователя Telegram.
		/// </summary>
		public long TelegramUserId { get; set; }

		/// <summary>
		/// Идентификатор чата для отправки сообщений пользователю.
		/// </summary>
		public long ChatId { get; set; }


		/// <summary>
		/// Контекст сервера ЛЭРС УЧЁТ.
		/// </summary>
		public LersContext Context;


		/// <summary>
		/// Контекст выполняемой команды.
		/// </summary>
		internal CommandContext CommandContext { get; set; }


		/// <summary>
		/// Устанавливает подключение к серверу ЛЭРС УЧЁТ.
		/// </summary>
		public void Connect()
		{
			/*if (this.Context.Server == null)
			{
				this.Context.Server = new Lers.LersServer("Бот Telegram");
			}

			if (!this.Context.Server.IsConnected)
			{
				var auth = new Lers.Networking.BasicAuthenticationInfo(this.Context.Login,
					Lers.Networking.SecureStringHelper.ConvertToSecureString(this.Context.Password));

				this.Context.Server.VersionMismatch += (sender, e) => e.Ignore = true;

				try
				{
					this.Context.Server.Connect(Config.Instance.LersServerAddress, Config.Instance.LersServerPort, auth);
				}
				catch (Lers.Networking.AuthorizationFailedException exc)
				{
					logger.Error($"Ошибка подключения пользователя {this.Context.Login} к серверу {Config.Instance.LersServerAddress}. {exc.Message}");
					Remove(this);
				}
			}*/
		}
	}	
}
