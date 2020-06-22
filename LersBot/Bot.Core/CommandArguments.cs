using System;
using System.Collections.Generic;

namespace LersBot
{
	/// <summary>
	/// Методы для работы с аргументами, которые передаются в командах бота.
	/// </summary>
	public class CommandArguments
	{
		/// <summary>
		/// Состояние разбора аргументов.
		/// </summary>
		enum ParseState
		{
			/// <summary>
			/// Разбираем обычный аргумент.
			/// </summary>
			Argument,

			/// <summary>
			/// Разбираем аргумент в кавычках.
			/// </summary>
			Quote,

			/// <summary>
			/// Нужно сохранить текущий результат в виде строки.
			/// </summary>
			Split
		}


		public CommandArguments()
		{
		}

		/// <summary>
		/// Разделяет текст сообщения боту на аргументы.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		/// <remarks>
		/// Строка разделяется на аргументы пробелами.
		/// </remarks>
		public static string[] Split(string input)
		{
			if (input == null)
			{
				throw new ArgumentNullException(nameof(input));
			}

			var result = new List<string>();

			var argument = string.Empty;

			var state = ParseState.Argument;

			for (int i = 0; i < input.Length; ++i)
			{
				char c = input[i];

				switch (c)
				{
					case '"':
						state = HandleQuote(state);
						break;

					case ' ':
					case '\t':
						state = HandleSpace(state, c, ref argument);
						break;

					default:
						argument += c;
						break;
				}

				if (state == ParseState.Split)
				{
					state = HandleSplit(result, ref argument);
				}
			}

			if (!string.IsNullOrEmpty(argument))
			{
				if (state == ParseState.Quote)
				{
					// Если после парсинга мы остаётся в состоянии "Quote", значит, какая-то
					// кавычка не закрылась. Постараемся разделить оставшуюся строку рекурсивно.
					string[] restFields = Split(argument);

					result.AddRange(restFields);
				}
				else if (state == ParseState.Argument)
				{
					result.Add(argument);
				}
				else
				{
					throw new InvalidOperationException("Неподдерживаемое состояние: " + state);
				}
			}

			return result.ToArray();
		}

		private static ParseState HandleSpace(ParseState state, char spaceChar, ref string argument)
		{
			switch (state)
			{
				case ParseState.Argument:
					// Если мы в режиме разбора строки, продолжаем
					return ParseState.Split;

				case ParseState.Quote:
					argument += spaceChar;
					return state;

				default:
					throw new InvalidOperationException("Неподдерживаемое состояние: " + state);
			}
		}

		private static ParseState HandleQuote(ParseState state)
		{
			switch (state)
			{
				case ParseState.Quote:
					// Если мы встретили кавычку и мы находимся в режиме разбора строки, перейдём к разделению.
					return ParseState.Split;

				case ParseState.Argument:
					// Если мы в режиме разбора аргумента, перейдём к разбору кавычек.
					return ParseState.Quote;

				default:
					throw new InvalidOperationException("Неподдерживаемое состояние: " + state);
			}
		}

		private static ParseState HandleSplit(List<string> result, ref string argument)
		{
			if (!string.IsNullOrEmpty(argument))
			{
				result.Add(argument);
				argument = string.Empty;
			}

			return ParseState.Argument;
		}
	}
}
