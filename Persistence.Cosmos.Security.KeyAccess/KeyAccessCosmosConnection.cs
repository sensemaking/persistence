using Microsoft.Azure.Cosmos;
using System;
using System.Serialization;

namespace Fdb.Rx.Persistence.Cosmos.Security.KeyAccess
{
    public class KeyAccessCosmosConnection(string endpoint, string authorizationKey) : IConnectToCosmos
    {
        public Func<CosmosClient> Client { get; private set; } = () => new CosmosClient(endpoint, authorizationKey, new CosmosClientOptions() { Serializer = new CosmosJsonNetSerializer(Serialization.GetSettings()) });
    }
}