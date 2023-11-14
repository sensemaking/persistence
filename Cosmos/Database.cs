using System;
using System.Threading;
using Microsoft.Azure.Cosmos;

namespace Sensemaking.Cosmos
{
    public static class Database
    {
        internal static string DatabaseName = null!;

        private static Lazy<CosmosClient>? Client;

        public static void Configure(string endPoint, string authenticationKey, string databaseName)
        {
            DatabaseName = databaseName;
            Client = new Lazy<CosmosClient>(() =>
                    new CosmosClient(endPoint, authenticationKey, new CosmosClientOptions { Serializer = new Serializer() }),
                    LazyThreadSafetyMode.PublicationOnly);
        }

        internal static CosmosClient GetClient()
        {
            if (!IsConfigured())
                throw new Exception("Please provide end point, authorisation key and database name");

            return Client!.Value;
        }

        private static bool IsConfigured() => Client != null;
    }
}