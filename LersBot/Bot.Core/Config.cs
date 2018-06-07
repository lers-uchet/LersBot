using System;
using System.IO;
using Newtonsoft.Json;

namespace LersBot
{
	/// <summary>
	/// Параметры конфигурации бота.
	/// </summary>
	internal class Config
	{
		public static readonly string UsersFilePath = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\LERS\LersBot\users.json");

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
