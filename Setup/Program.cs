using System;
using WixSharp;

namespace Setup
{
	class Program
	{
		static void Main()
		{
			var project = new Project("Lers Telegram Bot",
							  new Dir(@"%ProgramFiles%\LERS\LersBot",
								  new DirFiles(@"..\LersBot\bin\publish-x86\*.*")));

			project.LicenceFile = "license.rtf";

			project.UI = WUI.WixUI_InstallDir;

			project.GUID = new Guid("6fe30b47-2577-43ad-9095-1861ba25889b");

			project.OutFileName = "Lers.TelegramBot.Setup";

			project.BuildMsi();
		}
	}
}