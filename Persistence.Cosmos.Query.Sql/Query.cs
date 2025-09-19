using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Sensemaking.Persistence.Cosmos.Query.Sql;

public interface IRunQueries
{
    Task<IEnumerable<T>> GetResults<T>(string container, string query);
    Task<IEnumerable<T>> GetResults<T>((string Container, string Query) definition);
}

public class QueryRunner : IRunQueries
{
    public Task<IEnumerable<T>> GetResults<T>(string container, string query) => GetResults<T>((container, query));
    public async Task<IEnumerable<T>> GetResults<T>((string Container, string Query) definition)
    {
        var results = new List<T>();
        var iterator = Database.GetClient().GetDatabase(Database.DatabaseName!)
            .GetContainer(definition.Container)
            .GetItemQueryIterator<T>(new QueryDefinition(definition.Query));

        while (iterator.HasMoreResults)
            results.AddRange(await iterator.ReadNextAsync().ConfigureAwait(false));

        return results;
    }
}