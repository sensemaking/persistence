using System;
using System.Threading;
using Microsoft.Azure.Cosmos;

namespace Sensemaking.Cosmos
{
    public static class Database
    {
        private static string? EndPoint;
        private static string? AuthorisationKey;
        internal static string? DatabaseName;

        private static readonly Lazy<CosmosClient> Client = new Lazy<CosmosClient>(() => 
            new CosmosClient(EndPoint, AuthorisationKey, new CosmosClientOptions { Serializer = new Serializer() }), 
            LazyThreadSafetyMode.PublicationOnly);

        public static void Configure(string endPoint, string authenticationKey, string databaseName)
        {
            EndPoint = endPoint;
            AuthorisationKey = authenticationKey;
            DatabaseName = databaseName;
        }

        internal static CosmosClient GetClient()
        {
            if (!IsConfigured)
                throw new Exception("Please provide end point, authorisation key and database name");

            return Client.Value;
        }

        private static bool IsConfigured => !string.IsNullOrEmpty(EndPoint) && !string.IsNullOrEmpty(AuthorisationKey) && !string.IsNullOrEmpty(DatabaseName);
    }
}