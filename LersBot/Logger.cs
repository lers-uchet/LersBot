using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LersBot
{
	enum Severity
	{
		Debug,

		Info,

		Error
	}

	/// <summary>
	/// Класс обеспечивает протоколирование в журнал.
	/// </summary>
	static class Logger
	{
		private static string LogFileName;

		public static void Initialize(string logFileName)
		{
			LogFileName = logFileName;

			Directory.CreateDirectory(Path.GetDirectoryName(LogFileName));
		}

		public static void Log(Severity severity, string message)
		{
			Console.WriteLine(message);

			var sb = new StringBuilder();

			sb.Append(DateTime.Now.ToString());

			sb.Append("\t");

			sb.Append(severity);

			sb.Append("\t");

			sb.Append(message);

			sb.Append("\r\n");

			File.AppendAllText(LogFileName, sb.ToString());
		}

		public static void LogMessage(string message)
		{
			Log(Severity.Info, message);
		}

		public static void LogDebug(string message)
		{
			Log(Severity.Debug, message);
		}

		public static void LogError(string message)
		{
			Log(Severity.Error, message);
		}
	}
}
