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
		public static Config Instance { get; private set; }

		public static void Load()
		{
			string configText = File.ReadAllText("bot.config");

			Instance = JsonConvert.DeserializeObject<Config>(configText);
		}

		public string Token { get; set; }

		public IList<UserName> Users { get; set; }

		public string LersServerAddress { get; set; }

		public ushort LersServerPort { get; set; }
	}

	class UserName
	{
		public string TelegramUser { get; set; }

		public string LersUser { get; set; }

		public string LersPassword { get; set; }
	}
}
