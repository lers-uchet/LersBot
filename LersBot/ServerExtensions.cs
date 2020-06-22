using System;
using System.Collections.Generic;
using System.Linq;
/*using Lers;
using Lers.Core;
*/
namespace LersBot
{
	static class ServerExtensions
	{
		/*public static IEnumerable<Node> GetNodes(this LersServer server, IEnumerable<string> criterias)
		{
			if (criterias == null)
			{
				throw new ArgumentException("Не заданы критерии для поиска объектов", nameof(criterias));
			}

			// Выбираем те объекты, у которых в имени и в адресе есть все переданные критерии

			return server.Nodes.GetList().Where(x => string.Concat(x.Title, x.Address).ContainsAll(criterias));
		}

		public static IEnumerable<MeasurePoint> GetMeasurePoints(this LersServer server, IEnumerable<string> criterias)
		{
			if (criterias == null || !criterias.Any())
			{
				throw new ArgumentException("Не заданы критерии для поиска объектов", nameof(criterias));
			}

			// Выбираем те точки учёта, у которых в имени и в адресе есть все переданные критерии

			return server.MeasurePoints.GetList().Where(x => x.Type == MeasurePointType.Regular && string.Concat(x.FullTitle, x.Address).ContainsAll(criterias));
		}*/
	}
}
