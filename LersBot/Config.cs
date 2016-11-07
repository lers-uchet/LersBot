using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LersBot
{
	class Config
	{
		private const string ContextPath = "users.json";

		public static Config Instance { get; private set; }

		public static void Load()
		{
			string configText = File.ReadAllText("bot.config");

			Instance = JsonConvert.DeserializeObject<Config>(configText);

			// Загрузим контексты
			Instance.LoadContexts();
		}

		private void LoadContexts()
		{
			Contexts = new List<UserContext>();

			if (File.Exists(ContextPath))
			{
				string contextContent = File.ReadAllText(ContextPath);

				var obj = JsonConvert.DeserializeObject<List<UserContext>>(contextContent);

				Contexts.AddRange(obj);
			}

			foreach (UserName user in Instance.Users)
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
			string contextText = JsonConvert.SerializeObject(Instance.Contexts);

			File.WriteAllText(ContextPath, contextText);
		}

		public string Token { get; set; }

		public IList<UserName> Users { get; set; }

		private List<UserContext> Contexts;

		public string LersServerAddress { get; set; }

		public ushort LersServerPort { get; set; }
	}

	class UserName
	{
		public string TelegramUser { get; set; }

		public string LersUser { get; set; }

		public string LersPassword { get; set; }

		internal UserContext Context;
	}

	class UserContext
	{
		public string UserName;

		public long ChatId;

		public UserContext() {  }
	}
}
