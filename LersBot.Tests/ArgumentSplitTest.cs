
using Xunit;

namespace LersBot.Tests
{
	/// <summary>
	/// Содержит тесты блока разделения аргументов.
	/// </summary>
	public class ArgumentSplitTest
	{
		/// <summary>
		/// Проверяет что разделение сообщения на аргументы возвращает ожидаемые значения.
		/// </summary>
		[Fact]
		public void SplitArgumentsWithoutQuotes_ReturnsExpected()
		{
			string input = "one two three four";

			string[] result = CommandArguments.Split(input);

			Assert.Equal(4, result.Length);

			Assert.Equal("one", result[0]);
			Assert.Equal("two", result[1]);
			Assert.Equal("three", result[2]);
			Assert.Equal("four", result[3]);
		}

		/// <summary>
		/// Проверяет что разделение сообщения на аргументы возвращает ожидаемые значения.
		/// </summary>
		[Fact]
		public void SplitExtraSpacedArgumentsWithoutQuotes_ReturnsExpected()
		{
			string input = "one two  three four";

			string[] result = CommandArguments.Split(input);

			Assert.Equal(4, result.Length);

			Assert.Equal("one", result[0]);
			Assert.Equal("two", result[1]);
			Assert.Equal("three", result[2]);
			Assert.Equal("four", result[3]);
		}


		/// <summary>
		/// Проверяет что разделение сообщения на аргументы возвращает ожидаемые значения.
		/// </summary>
		[Fact]
		public void SplitArgumentsWithQuotes_ReturnsExpected()
		{
			string input = "one \"two three\" four";

			string[] result = CommandArguments.Split(input);

			Assert.Equal(3, result.Length);

			Assert.Equal("one", result[0]);
			Assert.Equal("two three", result[1]);
			Assert.Equal("four", result[2]);
		}


		/// <summary>
		/// Проверяет что разделение сообщения на аргументы возвращает ожидаемые значения.
		/// </summary>
		[Fact]
		public void SplitArgumentsWithNonClosingQuotes_ReturnsExpected()
		{
			string input = "one \"two three four";

			string[] result = CommandArguments.Split(input);

			Assert.Equal(4, result.Length);

			Assert.Equal("one", result[0]);
			Assert.Equal("two", result[1]);
			Assert.Equal("three", result[2]);
			Assert.Equal("four", result[3]);
		}


		/// <summary>
		/// Проверяет что разделение сообщения на аргументы возвращает ожидаемые значения.
		/// </summary>
		[Fact]
		public void SplitArgumentsWithQuotesAtBeginning_ReturnsExpected()
		{
			string input = "\"one \"two three four";

			string[] result = CommandArguments.Split(input);

			Assert.Equal(4, result.Length);

			Assert.Equal("one ", result[0]);
			Assert.Equal("two", result[1]);
			Assert.Equal("three", result[2]);
			Assert.Equal("four", result[3]);
		}


		/// <summary>
		/// Проверяет что разделение сообщения на аргументы возвращает ожидаемые значения.
		/// </summary>
		[Fact]
		public void SplitArgumentsWithQuotesAtEnd_ReturnsExpected()
		{
			string input = "one \"two three four\"";

			string[] result = CommandArguments.Split(input);

			Assert.Equal(2, result.Length);

			Assert.Equal("one", result[0]);
			Assert.Equal("two three four", result[1]);
		}
	}
}
