using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LersBot.Bot.Core
{
	/// <summary>
	/// Сервис для работы со списком зарегистрированных пользователей.
	/// </summary>
	public class UsersService
	{
		/// <summary>
		/// Список зарегистрированных пользователей.
		/// </summary>
		private List<User> List { get; } =  new List<User>();

		public UsersService()
		{
			LoadList();
		}

		/// <summary>
		/// Загружает из файла список зарегистрированных пользователей.
		/// </summary>
		private void LoadList()
		{
			// Создаём папку с параметрами пользователей, если её ещё нет.

			Directory.CreateDirectory(Path.GetDirectoryName(Config.UsersFilePath));

			if (File.Exists(Config.UsersFilePath))
			{
				string content = File.ReadAllText(Config.UsersFilePath);

				var obj = JsonConvert.DeserializeObject<List<User>>(content);

				if (obj != null)
				{
					List.AddRange(obj);
				}
			}
		}



		/// <summary>
		/// Добавляет нового пользователя.
		/// </summary>
		/// <param name="user"></param>
		internal void Add(User user)
		{
			lock (List)
			{
				List.Add(user);

				Save();
			}
		}

		internal void Remove(User user)
		{
			lock (List)
			{
				List.Remove(user);

				Save();
			}
		}

		internal IEnumerable<User> Where(Func<User, bool> predicate)
		{
			lock (List)
			{
				return List.Where(predicate).ToList();
			}
		}

		internal User FirstOrDefault(Func<User, bool> predicate)
		{
			lock (List)
			{
				return List.FirstOrDefault(predicate);
			}
		}

		/// <summary>
		/// Сохраняет в файл список зарегистрированных пользователей.
		/// </summary>
		public void Save()
		{
			lock (List)
			{
				string contextText = JsonConvert.SerializeObject(List);

				File.WriteAllText(Config.UsersFilePath, contextText);
			}
		}
	}
}
