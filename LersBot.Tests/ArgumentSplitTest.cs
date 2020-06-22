
namespace LersBot.Tests
{
	/*/// <summary>
	/// Содержит тесты блока разделения аргументов.
	/// </summary>
	public class ArgumentSplitTest
	{
		/// <summary>
		/// Проверяет что разделение сообщения на аргументы возвращает ожидаемые значения.
		/// </summary>
		[Test]
		public void SplitArgumentsWithoutQuotes_ReturnsExpected()
		{
			string input = "one two three four";

			string[] result = CommandArguments.Split(input);

			Assert.AreEqual(4, result.Length);

			Assert.AreEqual("one", result[0]);
			Assert.AreEqual("two", result[1]);
			Assert.AreEqual("three", result[2]);
			Assert.AreEqual("four", result[3]);
		}

		/// <summary>
		/// Проверяет что разделение сообщения на аргументы возвращает ожидаемые значения.
		/// </summary>
		[Test]
		public void SplitExtraSpacedArgumentsWithoutQuotes_ReturnsExpected()
		{
			string input = "one two  three four";

			string[] result = CommandArguments.Split(input);

			Assert.AreEqual(4, result.Length);

			Assert.AreEqual("one", result[0]);
			Assert.AreEqual("two", result[1]);
			Assert.AreEqual("three", result[2]);
			Assert.AreEqual("four", result[3]);
		}


		/// <summary>
		/// Проверяет что разделение сообщения на аргументы возвращает ожидаемые значения.
		/// </summary>
		[Test]
		public void SplitArgumentsWithQuotes_ReturnsExpected()
		{
			string input = "one \"two three\" four";

			string[] result = CommandArguments.Split(input);

			Assert.AreEqual(3, result.Length);

			Assert.AreEqual("one", result[0]);
			Assert.AreEqual("two three", result[1]);
			Assert.AreEqual("four", result[2]);
		}


		/// <summary>
		/// Проверяет что разделение сообщения на аргументы возвращает ожидаемые значения.
		/// </summary>
		[Test]
		public void SplitArgumentsWithNonClosingQuotes_ReturnsExpected()
		{
			string input = "one \"two three four";

			string[] result = CommandArguments.Split(input);

			Assert.AreEqual(4, result.Length);

			Assert.AreEqual("one", result[0]);
			Assert.AreEqual("two", result[1]);
			Assert.AreEqual("three", result[2]);
			Assert.AreEqual("four", result[3]);
		}


		/// <summary>
		/// Проверяет что разделение сообщения на аргументы возвращает ожидаемые значения.
		/// </summary>
		[Test]
		public void SplitArgumentsWithQuotesAtBeginning_ReturnsExpected()
		{
			string input = "\"one \"two three four";

			string[] result = CommandArguments.Split(input);

			Assert.AreEqual(4, result.Length);

			Assert.AreEqual("one ", result[0]);
			Assert.AreEqual("two", result[1]);
			Assert.AreEqual("three", result[2]);
			Assert.AreEqual("four", result[3]);
		}


		/// <summary>
		/// Проверяет что разделение сообщения на аргументы возвращает ожидаемые значения.
		/// </summary>
		[Test]
		public void SplitArgumentsWithQuotesAtEnd_ReturnsExpected()
		{
			string input = "one \"two three four\"";

			string[] result = CommandArguments.Split(input);

			Assert.AreEqual(2, result.Length);

			Assert.AreEqual("one", result[0]);
			Assert.AreEqual("two three four", result[1]);
		}
	}*/
}
