using System;

namespace LersBot
{
	/// <summary>
	/// Параметры конфигурации бота.
	/// </summary>
	public class Config
	{
		public static readonly string UsersFilePath = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\LERS\LersBot\users.json");

		/// <summary>
		/// Токен.
		/// </summary>
		public string Token { get; set; }

		/// <summary>
		/// Адрес сервера ЛЭРС УЧЁТ.
		/// </summary>
		public string LersServerAddress { get; set; }

		/// <summary>
		/// Порт сервера ЛЭРС УЧЁТ.
		/// </summary>
		public ushort LersServerPort { get; set; }
	}
}
