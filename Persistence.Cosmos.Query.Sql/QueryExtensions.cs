using System;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos;

namespace Fdb.Rx.Persistence.Cosmos.Query.Sql;

internal static class QueryExtensions
{
    internal static T[] Query<T>(this CosmosClient client, string databaseName, string container, string query)
    {
        var results = new List<T>();
        using (var iterator = client.GetDatabase(databaseName).GetContainer(container).GetItemQueryIterator<T>(new QueryDefinition(query)))
        {
            while (iterator.HasMoreResults)
            {
                results.AddRange(iterator.ReadNextAsync().Await());
            }
        }

        return results.ToArray();
    }
}