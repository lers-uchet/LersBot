using System;
using System.Diagnostics;

namespace LersBot
{
	class Program
	{
		static void Main(string[] args)
		{
			bool runAsService = false;

			if (args.Length > 0)
			{
				string arg = args[0];

				if (arg.ToLower() == "/service")
				{
					runAsService = true;
				}
			}

			if (runAsService)
			{
				System.ServiceProcess.ServiceBase.Run(new LersBotService());
			}
			else
			{
				var service = new LersBotService();

				service.Start();

				Console.WriteLine("Press any key to exit");

				Console.ReadKey();

				service.MyStop();
			}
		}
	}
}
