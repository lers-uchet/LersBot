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
		public static string UsersFilePath = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\LERS\LersBot\users.json");

		public static string LogFilePath = Environment.ExpandEnvironmentVariables(@"%ALLUSERSPROFILE%\LERS\LersBot\bot.log");

		private static string BotConfigFileName = "bot.config";


		public static Config Instance { get; private set; }

		public static void Load()
		{
			var uri = new UriBuilder(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);

			string botConfigPath = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));

			botConfigPath = Path.Combine(botConfigPath, BotConfigFileName);

			string configText = File.ReadAllText(botConfigPath);

			Instance = JsonConvert.DeserializeObject<Config>(configText);
		}


		public string Token { get; set; }

		public string LersServerAddress { get; set; }

		public ushort LersServerPort { get; set; }
	}
}
