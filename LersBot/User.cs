using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace LersBot
{
	class User
	{
		/// <summary>
		/// Список зарегистрированных пользователей.
		/// </summary>
		internal static List<User> List;

		internal static void LoadList()
		{
			// Создаём папку с контекстами, если её ещё нет.
			Directory.CreateDirectory(Path.GetDirectoryName(Config.ContextFilePath));

			List = new List<User>();

			if (File.Exists(Config.ContextFilePath))
			{
				string content = File.ReadAllText(Config.ContextFilePath);

				var obj = JsonConvert.DeserializeObject<List<User>>(content);

				List.AddRange(obj);
			}
		}

		public static void Save()
		{
			lock (List)
			{
				string contextText = JsonConvert.SerializeObject(List);

				File.WriteAllText(Config.ContextFilePath, contextText);
			}
		}

		public string TelegramUser { get; set; }

		public long ChatId { get; set; }

		public LersContext Context;

		internal CommandContext CommandContext { get; set; }


		public void Connect()
		{
			if (this.Context.Server == null)
			{
				this.Context.Server = new Lers.LersServer();
			}

			if (!this.Context.Server.IsConnected)
			{
				var auth = new Lers.Networking.BasicAuthenticationInfo(this.Context.LersUser,
					Lers.Networking.SecureStringHelper.ConvertToSecureString(this.Context.LersPassword));

				this.Context.Server.VersionMismatch += (sender, e) => e.Ignore = true;

				this.Context.Server.Connect(Config.Instance.LersServerAddress, Config.Instance.LersServerPort, auth);
			}
		}
	}

	class LersContext
	{
		public string LersUser { get; set; }

		public string LersPassword { get; set; }

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
