using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LersBot
{
	class CommandContext
	{
		public readonly string Text;

		public CommandContext(string command)
		{
			this.Text = command;
		}
	}
}
