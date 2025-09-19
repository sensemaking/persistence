using System;
using Microsoft.Azure.Cosmos;

namespace Sensemaking.Persistence.Cosmos
{
    public static class Database
    {
        internal static string DatabaseName = null!;
        internal static IConnectToCosmos Connection = null!;

        private static Lazy<CosmosClient>? Client;

        internal static void Configure(IConnectToCosmos cosmosAccountConnection, string databaseName)
        {
            Connection = cosmosAccountConnection;
            DatabaseName = databaseName;
            Client = new Lazy<CosmosClient>(() => Connection.Client());
        }

        internal static CosmosClient GetClient()
        {
            if (!IsConfigured)
                throw new Exception("Please configure the document db before attempting to use it.");

            return Client!.Value;
        }

        private static bool IsConfigured => Client != null;
    }
}