using Microsoft.Azure.Cosmos;
using System;
using System.Serialization;

namespace Sensemaking.Persistence.Cosmos.Security.KeyAccess
{
    public class KeyAccessCosmosConnection(string endpoint, string authorizationKey) : IConnectToCosmos
    {
        public Func<CosmosClient> Client { get; private set; } = () => new CosmosClient(endpoint, authorizationKey, new CosmosClientOptions() { Serializer = new CosmosJsonNetSerializer(Serialization.GetSettings()) });
    }
}