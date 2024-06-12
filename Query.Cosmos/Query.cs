using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using static Sensemaking.Query.Cosmos.Querying;
using Database = Sensemaking.Cosmos.Database;

namespace Sensemaking.Query.Cosmos;

public abstract class Query<T, U> : IQuery<T, U>
{
    public async Task<IEnumerable<U>> GetResultsAsync(T parameters)
    {
        var spec = GetQuerySpecification(parameters);
        var results = new List<U>();
        var iterator = Database.GetClient().GetDatabase(Database.DatabaseName!).GetContainer(spec.Container).GetItemQueryIterator<U>(new QueryDefinition(spec.Query));

        while (iterator.HasMoreResults)
            results.AddRange(await iterator.ReadNextAsync().ConfigureAwait(false));

        return results;
    }

    protected abstract QuerySpecification GetQuerySpecification(T parameters);

    protected class QuerySpecification
    {
        public QuerySpecification(string container, string query)
        {
            Container = container;
            Query = query;
        }

        public string Container { get; }
        public string Query { get; }
    }
}

internal static class Querying
{
    internal static async Task<IEnumerable<T>> GetResults<T>((string Container, string Query) specification)
    {
        var results = new List<T>();
        var iterator = Database.GetClient().GetDatabase(Database.DatabaseName!)
            .GetContainer(specification.Container)
            .GetItemQueryIterator<T>(new QueryDefinition(specification.Query));

        while (iterator.HasMoreResults)
            results.AddRange(await iterator.ReadNextAsync().ConfigureAwait(false));

        return results;
    }
}

public class Query<T>
{
    private readonly (string Container, string Query) specification;

    public Query(string container, string query) : this(new(container, query))
    {
    }

    public Query((string Container, string Query) specification)
    {
        this.specification = specification;
    }

    public async Task<IEnumerable<T>> GetResults()
    {
        return await GetResults<T>(specification);
    }
}

public class ParameterisedQuery<T, U>
{
    private readonly (string Container, Func<T, string> BuildQuery) specification;

    public ParameterisedQuery(string container, Func<T, string> buildQuery) : this(new(container, buildQuery))
    {
    }

    public ParameterisedQuery((string Container, Func<T, string> BuildQuery) specification)
    {
        this.specification = specification;
    }

    public async Task<IEnumerable<U>> GetResults(T parameters)
    {
        return await GetResults<U>((specification.Container, specification.BuildQuery(parameters)));
    }
}

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