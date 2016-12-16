namespace LersBot
{
	/// <summary>
	/// Параметры команды, которую отправил пользователь.
	/// </summary>
	class UserCommandParameters
	{
		public readonly string Commmand;

		public readonly string[] Arguments;

		public UserCommandParameters(string command, string[] arguments)
		{
			this.Commmand = command;
			this.Arguments = arguments;
		}
	}
}
