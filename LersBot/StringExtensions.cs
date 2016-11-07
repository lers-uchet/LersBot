using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LersBot
{
	static class StringExtensions
	{
		public static bool ContainsAll(this string s, IEnumerable<string> values)
		{
			bool result = true;

			foreach (string value in values)
			{
				if (s.IndexOf(value, 0, StringComparison.OrdinalIgnoreCase) < 0)
				{
					result = false;
					break;
				}
			}

			return result;
		}
	}
}
