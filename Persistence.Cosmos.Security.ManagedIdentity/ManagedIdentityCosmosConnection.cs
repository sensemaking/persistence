using System;
using System.Serialization;
using Azure.Identity;
using Microsoft.Azure.Cosmos;

namespace Sensemaking.Persistence.Cosmos.Security.ManagedIdentity
{
    public class ManagedIdentityCosmosConnection(string endpoint) : IConnectToCosmos
    {
        public Func<CosmosClient> Client { get; private set; } = () => new CosmosClient(endpoint, new DefaultAzureCredential(), new CosmosClientOptions() { Serializer = new CosmosJsonNetSerializer(Serialization.GetSettings()) });
    }
}