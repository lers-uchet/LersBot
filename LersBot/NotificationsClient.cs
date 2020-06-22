using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace LersBot
{
	public class NotificationsClient : IAsyncDisposable
	{
		private readonly HubConnection _hub = null;

		public NotificationsClient(string uri, string token)
		{
			HttpTransportType transports = GetSupportedSignalRTransports();

			var connectionBuilder = new HubConnectionBuilder()
				.WithUrl(uri, options =>
				{
					options.AccessTokenProvider = () => Task.FromResult(token);
					options.Transports = transports;
				})
				.WithAutomaticReconnect()
				.AddNewtonsoftJsonProtocol(c => c.PayloadSerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter()));

			_hub = connectionBuilder.Build();
		}

		public Task Connect() => _hub.StartAsync();

		public async Task<IDisposable> Subscribe<T>(Lers.Rest.Operation operation, 
			Lers.Rest.EntityType entityType, 
			int entityId, Action<T> handler)
		{
			var notification = _hub.On("Notify", new Action<Lers.Rest.Operation, Lers.Rest.NotificationParameters>(
				(op, param) =>
				{
					if (op == operation
						&& (entityType == Lers.Rest.EntityType.Empty || entityType == param.EntityType)
						&& (entityId <= 0 || entityId == param.EntityId))
					{
						var data = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(param.JsonData);
						handler(data);
					}
				}));

			await _hub.InvokeAsync("Subscribe", operation, entityType, entityId);

			return notification;
		}

		/// <summary>
		/// Возвращает поддерживаемые для SignalR транспорты на данной версии ОС.
		/// </summary>
		/// <remarks>
		/// Для Windows7 нужно использовать только LongPolling, так как это единственный поддерживаемый способ.
		/// </remarks>
		/// <returns></returns>
		private static HttpTransportType GetSupportedSignalRTransports()
		{
			HttpTransportType transports;

			var verWindows7 = new Version(6, 1, 7601);

			var verCurrent = Environment.OSVersion.Version;
			var currentVersionToCompare = new Version(verCurrent.Major, verCurrent.Minor, verCurrent.Build);

			if (currentVersionToCompare.CompareTo(verWindows7) > 0)
			{
				transports = HttpTransportType.WebSockets | HttpTransportType.ServerSentEvents | HttpTransportType.LongPolling;
			}
			else
			{
				// Для ОС Windows7 и ниже используем только метод Long Polling.
				transports = HttpTransportType.LongPolling;
			}

			return transports;
		}

		public async ValueTask DisposeAsync()
		{
			if (_hub != null)
			{
				await _hub.DisposeAsync();
			}
		}
	}
}
