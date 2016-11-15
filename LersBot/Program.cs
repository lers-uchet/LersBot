using System;
using System.Diagnostics;

namespace LersBot
{
	class Program
	{
		static void Main(string[] args)
		{
			bool runAsService = true;

			if (args.Length > 0)
			{
				string arg = args[0];

				if (arg.ToLower() == "/console")
				{
					runAsService = false;
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
