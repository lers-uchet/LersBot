using LersBot.Bot.Core;
using System.Threading.Tasks;

namespace LersBot
{
	/// <summary>
	/// Пользователь бота.
	/// </summary>
	public class User
	{
		/// <summary>
		/// Идентификатор пользователя Telegram.
		/// </summary>
		public long TelegramUserId { get; set; }

		/// <summary>
		/// Идентификатор чата для отправки сообщений пользователю.
		/// </summary>
		public long ChatId { get; set; }


		/// <summary>
		/// Контекст сервера ЛЭРС УЧЁТ.
		/// </summary>
		public LersContext Context { get; set; }


		/// <summary>
		/// Контекст выполняемой команды.
		/// </summary>
		internal CommandContext CommandContext { get; set; }

		public Lers.Rest.Account Current { get; private set; }

		public int CurrentId { get; private set; }

		public User()
		{

		}

		public async Task Authorize()
		{
			Context.RestClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Context.Token);

			var loginClient = new Lers.Rest.LoginClient(Context.BaseUri.ToString(), Context.RestClient);

			Current = (await loginClient.GetCurrentLoginAsync()).Account;
		}

		/// <summary>
		/// Устанавливает подключение к серверу ЛЭРС УЧЁТ.
		/// </summary>
		public async Task Connect(string login, string password)
		{			
			Context.RestClient.DefaultRequestHeaders.Authorization = null;

			var loginClient = new Lers.Rest.LoginClient(Context.BaseUri.ToString(), Context.RestClient);

			var response = await loginClient.LoginPlainAsync(new Lers.Rest.AuthenticatePlainRequestParameters
			{
				Login = login,
				Password = password,
				Application = "Telegram Bot"
			});

			// Сохраняем авторизацию.

			if (!string.IsNullOrEmpty(response.Token))
			{
				Context.Token = response.Token;
				Context.RestClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", response.Token);
			}

			// Запрашиваем текущего пользователя.

			Current = (await loginClient.GetCurrentLoginAsync()).Account;			
		}
	}	
}
