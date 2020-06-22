using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lers.Rest;

namespace LersBot
{
	static class ServerExtensions
	{
		public static async Task<IEnumerable<Node>> GetNodes(this NodesClient client, IEnumerable<string> criterias)
		{
			if (criterias == null)
			{
				throw new ArgumentException("Не заданы критерии для поиска объектов", nameof(criterias));
			}

			// Выбираем те объекты, у которых в имени и в адресе есть все переданные критерии

			var nodes = await client.GetNodesAsync(null, null, null, null, null, null, null, null);

			return nodes.Nodes.Where(x => string.Concat(x.Title, x.Address).ContainsAll(criterias));
		}
		
		public static async Task<IEnumerable<MeasurePoint>> GetMeasurePoints(this MeasurePointsClient server, IEnumerable<string> criterias)
		{
			if (criterias == null || !criterias.Any())
			{
				throw new ArgumentException("Не заданы критерии для поиска объектов", nameof(criterias));
			}

			// Выбираем те точки учёта, у которых в имени и в адресе есть все переданные критерии

			var measurePoints = await server.GetMeasurePointsAsync(null, null, MeasurePointType.Regular);

			return measurePoints.MeasurePoints.Where(x => x.Type == MeasurePointType.Regular 
				&& string.Concat(x.FullTitle, x.Address)
				.ContainsAll(criterias));
		}
	}
}
