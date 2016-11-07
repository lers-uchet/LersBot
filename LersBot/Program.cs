using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Lers;
using Lers.Core;
using Lers.Data;
using Lers.Utils;

namespace LersBot
{
	class Program
	{
		private static LersBot bot;

		static void Main(string[] args)
		{
			Config.Load();

			bot = new LersBot();

			Console.WriteLine($"Stariting {bot.UserName}");

			bot.AddCommandHandler(ShowStart, "/start");
			bot.AddCommandHandler(ShowCurrents, "/getcurrents");
			bot.AddCommandHandler(ShowNodes, "/nodes");
			bot.AddCommandHandler(ShowMeasurePoints, "/mpts");
			bot.Start();

			Console.WriteLine("Press any key to exit");

			Console.ReadKey();
		}

		private static void ShowStart(LersServer server, long chatId, string[] arguments)
		{
			bot.SendText(chatId, $"Добро пожаловать, {server.Accounts.Current.DisplayName}");
		}


		private static void ShowCurrents(LersServer server, long chatId, string[] arguments)
		{
			var measurePoint = server.GetMeasurePoints(arguments).FirstOrDefault();

			if (measurePoint == null)
			{
				bot.SendText(chatId, "Точка учёта не найдена");
				return;
			}

			bot.SendText(chatId, $"Точка учёта {measurePoint.FullTitle}");

			var options = new MeasurePointPollCurrentOptions { StartMode = Lers.Common.PollManualStartMode.Force };

			int pollSessionId = measurePoint.PollCurrent(options);

			bot.SendText(chatId, "Запущен опрос");

			var autoResetEvent = new System.Threading.AutoResetEvent(false);

			var notifyToken = server.AddNotification((int)Lers.Interop.Operation.SAVE_CURRENT_DATA,
				(int)Lers.Interop.EntityType.PollSession, pollSessionId, true,
				(notifyData, userState) =>
				{
					try
					{
						var param = (Lers.Interop.CurrentConsumptionDataReadNotifyParams)Lers.Serialization.SerializationHelper.FromPropertyBag(notifyData);

						if (measurePoint != null)
						{
							MeasurePointConsumptionRecord record = MeasurePointData.__ConvertToMeasurePointCurrentConsumptionRecord(param.Consumption, measurePoint.ResourceKind);

							SendCurrents(chatId, record);
						}

						autoResetEvent.Set();
					}
					catch (Exception exc)
					{
						bot.SendText(chatId, exc.Message);
					}
				}, null);

			if (!autoResetEvent.WaitOne(120000))
			{
				bot.SendText(chatId, "Не удалось получить текущие данные за 2 минуты.");
			}

			server.RemoveNotification(notifyToken);

			server.Disconnect(10000);
		}

		private static void SendCurrents(long chatId, MeasurePointConsumptionRecord record)
		{
			var valueNames = new List<string>();
			var valueDisplayNames = new List<string>();

			switch (record.ResourceKind)
			{
				case ResourceKind.Water:
					var waterValues = Enum.GetValues(typeof(WaterRecordValues)).Cast<WaterRecordValues>().Where(v => v != WaterRecordValues.All && v != WaterRecordValues.None).ToList();
					waterValues.ForEach(v => valueNames.Add(v.ToString()));
					break;

				case ResourceKind.Gas:
					var gasValues = Enum.GetValues(typeof(GasRecordValues)).Cast<GasRecordValues>().Where(v => v != GasRecordValues.All && v != GasRecordValues.None).ToList();

					gasValues.ForEach(v => valueNames.Add(v.ToString()));

					break;

				case ResourceKind.Electricity:
					var electricValues = Enum.GetValues(typeof(ElectricCurrentsRecordValues)).Cast<ElectricCurrentsRecordValues>().Where(v => v != ElectricCurrentsRecordValues.All && v != ElectricCurrentsRecordValues.None).ToList();

					electricValues.ForEach(v => valueNames.Add(v.ToString()));

					break;

				default: throw new InvalidOperationException("Неверный тип ресурса");
			}

			for (int i = 0; i < valueNames.Count; ++i)
			{
				double? val = record.GetValue(valueNames[i]);

				if (val.HasValue)
				{
					string text = $"{valueNames[i]} = {val.Value:f2}";

					bot.SendText(chatId, text);
				}
			}
		}

		private static void ShowNodes(LersServer server, long chatId, string[] arguments)
		{
			var nodes = server.GetNodes(arguments);

			if (!nodes.Any())
			{
				bot.SendText(chatId, "Ничего не найдено");
				return;
			}

			try
			{
				foreach (var node in nodes)
				{
					bot.SendText(chatId, node.Title);
				}
			}
			catch (AggregateException ae)
			{
				foreach (var e in ae.InnerExceptions)
				{
					Console.WriteLine(e.Message);
				}
			}
		}


		private static void ShowMeasurePoints(LersServer server, long chatId, string[] arguments)
		{
			var measurePoints = server.GetMeasurePoints(arguments);

			if (!measurePoints.Any())
			{
				bot.SendText(chatId, "Ничего не найдено");
				return;
			}

			try
			{
				foreach (var mp in measurePoints)
				{
					bot.SendText(chatId, mp.FullTitle);
				}
			}
			catch (AggregateException ae)
			{
				foreach (var e in ae.InnerExceptions)
				{
					Console.WriteLine(e.Message);
				}
			}
		}
	}
}

