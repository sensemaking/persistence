using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Sensemaking.Domain.Cosmos
{
    internal static class DomainExtensions
    {
        internal static async Task<T?> GetAsync<T>(this CosmosClient client, string databaseName, string collectionName, string documentId) where T : class, IAggregate
        {
            try
            {
                return await client.GetDatabase(databaseName).GetContainer(collectionName).ReadItemAsync<T>(documentId, new PartitionKey(documentId));
            }
            catch (CosmosException)
            {
                return default;
            }
        }

        internal static async Task<IEnumerable<T>> GetAllASync<T>(this CosmosClient client, string databaseName, string collectionName) where T : class, IAggregate
        {
            var  results = new List<T>();
            var iterator = client.GetDatabase(databaseName).GetContainer(collectionName).GetItemQueryIterator<T>();
            while (iterator.HasMoreResults)
                (await iterator.ReadNextAsync()).ForEach(item => results.Add(item));

            return results;
        }

        internal static async Task SaveAsync<T>(this CosmosClient client, string databaseName, string collectionName, T aggregate) where T : class, IAggregate
        {
            await client.GetDatabase(databaseName).GetContainer(collectionName).UpsertItemAsync(aggregate);
        }

        internal static async Task DeleteAsync<T>(this CosmosClient client, string databaseName, string collectionName, string itemId) where T : class, IAggregate
        {
            try
            {
                await client.GetDatabase(databaseName).GetContainer(collectionName).DeleteItemAsync<T>(itemId, new PartitionKey(itemId));
            }
            catch (CosmosException exception)
            {
                if (exception.StatusCode != HttpStatusCode.NotFound)
                    throw;
            }
        }
    }
}