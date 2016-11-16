using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace LersBot
{
	/// <summary>
	/// Пользователь бота.
	/// </summary>
	class User
	{
		/// <summary>
		/// Список зарегистрированных пользователей.
		/// </summary>
		private static List<User> List;

		/// <summary>
		/// Загружает из файла список зарегистрированных пользователей.
		/// </summary>
		internal static void LoadList()
		{
			// Создаём папку с контекстами, если её ещё нет.

			Directory.CreateDirectory(Path.GetDirectoryName(Config.UsersFilePath));

			List = new List<User>();

			if (File.Exists(Config.UsersFilePath))
			{
				string content = File.ReadAllText(Config.UsersFilePath);

				var obj = JsonConvert.DeserializeObject<List<User>>(content);

				List.AddRange(obj);
			}
		}

		/// <summary>
		/// Добавляет нового пользователя.
		/// </summary>
		/// <param name="user"></param>
		internal static void Add(User user)
		{
			lock (List)
			{
				List.Add(user);

				Save();
			}
		}

		internal static IEnumerable<User> Where(Func<User, bool> predicate)
		{
			lock (List)
			{
				return List.Where(predicate).ToList();
			}
		}


		/// <summary>
		/// Сохраняет в файл список зарегисрированных пользователей.
		/// </summary>
		public static void Save()
		{
			lock (List)
			{
				string contextText = JsonConvert.SerializeObject(List);

				File.WriteAllText(Config.UsersFilePath, contextText);
			}
		}

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
			if (this.Context.Server == null)
			{
				this.Context.Server = new Lers.LersServer();
			}

			if (!this.Context.Server.IsConnected)
			{
				var auth = new Lers.Networking.BasicAuthenticationInfo(this.Context.Login,
					Lers.Networking.SecureStringHelper.ConvertToSecureString(this.Context.Password));

				this.Context.Server.VersionMismatch += (sender, e) => e.Ignore = true;

				this.Context.Server.Connect(Config.Instance.LersServerAddress, Config.Instance.LersServerPort, auth);
			}
		}
	}


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
