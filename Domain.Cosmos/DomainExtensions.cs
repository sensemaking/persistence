using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.Azure.Cosmos;

namespace Sensemaking.Domain.Cosmos
{
    internal static class DomainExtensions
    {
        [return: MaybeNull]
        internal static T Get<T>(this CosmosClient client, string databaseName, string collectionName, string documentId) where T : IAggregate
        {
            try
            {
                return client.GetDatabase(databaseName).GetContainer(collectionName).ReadItemAsync<T>(documentId, new PartitionKey(documentId))
                    .GetAwaiter().GetResult();
            }
            catch (CosmosException)
            {
                return default(T);
            }
        }

        internal static T[] GetAll<T>(this CosmosClient client, string databaseName, string collectionName) where T : IAggregate
        {
            var  results = new List<T>();
            var iterator = client.GetDatabase(databaseName).GetContainer(collectionName).GetItemQueryIterator<T>();
            while (iterator.HasMoreResults)
                iterator.ReadNextAsync().Result.ForEach(item => results.Add(item));

            return results.ToArray();
        }

        internal static void Save<T>(this CosmosClient client, string databaseName, string collectionName, T aggregate) where T : IAggregate
        {
            client.GetDatabase(databaseName).GetContainer(collectionName).UpsertItemAsync(aggregate).GetAwaiter().GetResult();
        }

        internal static void Delete<T>(this CosmosClient client, string databaseName, string collectionName, string itemId) where T : IAggregate
        {
            try
            {
                client.GetDatabase(databaseName).GetContainer(collectionName).DeleteItemAsync<T>(itemId, new PartitionKey(itemId))
                    .GetAwaiter().GetResult();
            }
            catch (CosmosException exception)
            {
                if (exception.StatusCode != HttpStatusCode.NotFound)
                    throw;
            }
        }
    }
}