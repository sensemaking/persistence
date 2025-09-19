using System;
using Microsoft.Azure.Cosmos;

namespace Sensemaking.Persistence.Cosmos
{
    public interface IConnectToCosmos
    {
        public Func<CosmosClient> Client { get; }
    }
}