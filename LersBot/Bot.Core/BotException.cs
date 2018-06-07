using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LersBot
{
	class BotException : Exception
	{
		public BotException(string message) : base(message)
		{
		}
	}

	class UnauthorizedCommandException : BotException
	{
		public UnauthorizedCommandException(string command)
			: base($"Для выполнения команды {command} необходима регистрация. Начните процедуру регистрации с помощью команды /start.")
		{
		}
	}
}
