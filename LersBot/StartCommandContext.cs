namespace LersBot
{
	/// <summary>
	/// Контекст выполняемой команды регистрации.
	/// </summary>
	class StartCommandContext : CommandContext
	{
		public StartCommandContext(string command) : base(command)
		{
		}

		public string Login { get; set; }

		public string Password { get; set; }
	}
}
