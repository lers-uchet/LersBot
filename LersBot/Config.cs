using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace LersBot
{
	/// <summary>
	/// Параметры конфигурации бота.
	/// </summary>
	class Config
	{
		public static string ContextFilePath = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\LERS\LersBot\users.json");

		public static string LogFilePath = Environment.ExpandEnvironmentVariables(@"%ALLUSERSPROFILE%\LERS\LersBot\bot.log");

		public static string BotConfigFilePath = "bot.config";


		public static Config Instance { get; private set; }

		public static void Load()
		{
			string configText = File.ReadAllText(BotConfigFilePath);

			Instance = JsonConvert.DeserializeObject<Config>(configText);

			// Создаём папку с контекстами, если её ещё нет.
			Directory.CreateDirectory(Path.GetDirectoryName(ContextFilePath));

			// Загрузим контексты
			Instance.LoadContexts();
		}

		private void LoadContexts()
		{
			Contexts = new List<UserContext>();

			if (File.Exists(ContextFilePath))
			{
				string contextContent = File.ReadAllText(ContextFilePath);

				var obj = JsonConvert.DeserializeObject<List<UserContext>>(contextContent);

				Contexts.AddRange(obj);
			}

			foreach (User user in Instance.Users)
			{
				var userCtxt = Contexts.Where(x => x.UserName == user.TelegramUser).FirstOrDefault();

				if (userCtxt == null)
				{
					userCtxt = new UserContext();
					userCtxt.UserName = user.TelegramUser;
					Contexts.Add(userCtxt);
				}

				user.Context = userCtxt;
			}
		}

		public static void SaveContexts()
		{
			lock (Instance)
			{
				string contextText = JsonConvert.SerializeObject(Instance.Contexts);

				File.WriteAllText(ContextFilePath, contextText);
			}
		}

		public string Token { get; set; }

		public IList<User> Users { get; set; }

		private List<UserContext> Contexts;

		public string LersServerAddress { get; set; }

		public ushort LersServerPort { get; set; }
	}


	class User
	{
		public string TelegramUser { get; set; }

		public string LersUser { get; set; }

		public string LersPassword { get; set; }

		internal UserContext Context;

		public  void Connect()
		{
			if (this.Context.Server == null)
			{
				this.Context.Server = new Lers.LersServer();
			}

			if (!this.Context.Server.IsConnected)
			{
				var auth = new Lers.Networking.BasicAuthenticationInfo(this.LersUser, Lers.Networking.SecureStringHelper.ConvertToSecureString(this.LersPassword));

				this.Context.Server.VersionMismatch += (sender, e) => e.Ignore = true;

				this.Context.Server.Connect(Config.Instance.LersServerAddress, Config.Instance.LersServerPort, auth);
			}
		}
	}
}
