using LersBot.Bot.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LersBot
{
	class Program
	{
		static void Main(string[] args)
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			Directory.SetCurrentDirectory(GetCurrentAssemblyDirectory());

			var hostBuilder = CreateHostBuilder(args.Where(x => x.ToUpperInvariant() != "/CONSOLE").ToArray())
				.UseWindowsService();
					
			hostBuilder.Build().Run();
		}

		/// <summary>
		/// Создаёт хост для запуска службы.
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public static IHostBuilder CreateHostBuilder(string[] args) =>
		Host.CreateDefaultBuilder(args)
			.ConfigureAppConfiguration(configHost =>
			{
				configHost.SetBasePath(GetCurrentAssemblyDirectory());
				configHost.AddJsonFile("bot.config");
				configHost.AddEnvironmentVariables("LERS_TELEGRAM_BOT_");
				configHost.AddCommandLine(args);
			})
			.ConfigureServices((hostContext, services) =>
			{
				services.Configure<Config>(hostContext.Configuration);
				services.AddSingleton<UsersService>();
				services.AddSingleton<LersBot>();
				services.AddSingleton<Notifier>();
				services.AddHostedService<LersBotService>();
			});


		private static string GetCurrentAssemblyDirectory()
		{
			string currentExe = Assembly.GetExecutingAssembly().Location;

			return Path.GetDirectoryName(currentExe)
				?? throw new InvalidOperationException("Cannot get directory name");
		}
	}
}
