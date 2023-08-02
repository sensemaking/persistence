using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using static Sensemaking.Query.Cosmos.Querying;
using Database = Sensemaking.Cosmos.Database;

namespace Sensemaking.Query.Cosmos
{
    public abstract class Query<T, U> : IQuery<T, U>
    {
        public async Task<IEnumerable<U>> GetResultsAsync(T parameters)
        {
            var spec = GetQuerySpecification(parameters);
            var results = new List<U>();
            var iterator = Database.GetClient().GetDatabase(Database.DatabaseName!).GetContainer(spec.Container).GetItemQueryIterator<U>(new QueryDefinition(spec.Query));

            while(iterator.HasMoreResults)
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

            while(iterator.HasMoreResults)
                results.AddRange(await iterator.ReadNextAsync().ConfigureAwait(false));

            return results;
        }
    }

    public class Query<T>
    {
        private readonly (string Container, string Query) specification;

        public Query((string Container, string Query) specification)
        {
            this.specification = specification;
        }

        protected async Task<IEnumerable<T>> GetResults()
        {
            return await Querying.GetResults<T>(specification);
        }
    }
    
    public class ParameterisedQuery<T, U>
    {
        private readonly (string Container, Func<U, string> BuildQuery) specification;

        public ParameterisedQuery((string Container, Func<U, string> BuildQuery) specification)
        {
            this.specification = specification;
        }

        protected async Task<IEnumerable<T>> GetResults(U parameters)
        {
            return await Querying.GetResults<T>((specification.Container, specification.BuildQuery(parameters)));
        }
    }
}
