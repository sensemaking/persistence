using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
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
}
