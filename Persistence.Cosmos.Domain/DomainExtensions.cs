using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Fdb.Rx.Domain;
using Microsoft.Azure.Cosmos;

namespace Fdb.Rx.Persistence.Cosmos
{
    internal static class DomainExtensions
    {
        internal static async Task<T> Get<T>(this CosmosClient client, string databaseName, string collectionName, string documentId) where T : IAggregate
        {
            try
            {
                return await client.GetDatabase(databaseName).GetContainer(collectionName).ReadItemAsync<T>(documentId, new PartitionKey(documentId)).ConfigureAwait(false);
            }
            catch (CosmosException)
            {
                return default!;
            }
        }

        internal static async Task<IReadOnlyCollection<T>> GetAll<T>(this CosmosClient client, string databaseName, string collectionName) where T : IAggregate
        {
            var results = new List<T>();
            using (var iterator = client.GetDatabase(databaseName).GetContainer(collectionName).GetItemQueryIterator<T>())
            {
                while (iterator.HasMoreResults)
                {
                    var items = await iterator.ReadNextAsync().ConfigureAwait(false);
                    items.ForEach(item => results.Add(item));
                }
            }

            return results;
        }

        internal static async Task Save<T>(this CosmosClient client, string databaseName, string collectionName, T aggregate) where T : IAggregate
        {
            await client.GetDatabase(databaseName).GetContainer(collectionName).UpsertItemAsync(aggregate, requestOptions: new ItemRequestOptions { EnableContentResponseOnWrite = false });
        }

        internal static async Task Delete<T>(this CosmosClient client, string databaseName, string collectionName, string documentId) where T : IAggregate
        {
            try
            {
                await client.GetDatabase(databaseName).GetContainer(collectionName).DeleteItemAsync<T>(documentId, new PartitionKey(documentId)).ConfigureAwait(false);
            }
            catch (CosmosException exception)
            {
                if (exception.StatusCode != HttpStatusCode.NotFound)
                    throw;
            }
        }

        internal static async Task<IEnumerable<T>> Query<T>(this CosmosClient client, string databaseName, string collectionName, string query) where T : IAggregate
        {
            var results = new List<T>();
            var iterator = Database.GetClient().GetDatabase(databaseName)
                .GetContainer(collectionName)
                .GetItemQueryIterator<T>(new QueryDefinition(query));

            while (iterator.HasMoreResults)
                results.AddRange(await iterator.ReadNextAsync().ConfigureAwait(false));

            return results;
        }
    }
}