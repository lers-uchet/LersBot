using System;

namespace LersBot
{
	/// <summary>
	/// Атрибут указывает требуется ли авторизация для выполнения команды бота.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	class AuthorizeAttribute : Attribute
	{
		/// <summary>
		/// Требуется ли авторизация для выполнения команды.
		/// </summary>
		public readonly bool IsAuthorizeRequired;

		/// <summary>
		/// Конструктор.
		/// </summary>
		/// <param name="isAuthorizeRequired"></param>
		public AuthorizeAttribute(bool isAuthorizeRequired)
		{
			this.IsAuthorizeRequired = isAuthorizeRequired;
		}
	}
}
