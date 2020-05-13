using System.Collections.Generic;
using Microsoft.Azure.Cosmos;

namespace Sensemaking.Query.Cosmos
{
    internal static class QueryExtensions
    {
        internal static T[] Query<T>(this CosmosClient client, string databaseName, string container, string query)
        {
            var results = new List<T>();
            var iterator = client.GetDatabase(databaseName).GetContainer(container).GetItemQueryIterator<T>(new QueryDefinition(query));
            while (iterator.HasMoreResults)
            {
                foreach (var item in iterator.ReadNextAsync().Result)
                {
                    results.Add(item);
                }
            }
            return results.ToArray();
        }
    }
}