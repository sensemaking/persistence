using System;
using Microsoft.Azure.Cosmos;

namespace Fdb.Rx.Persistence.Cosmos
{
    public interface IConnectToCosmos
    {
        public Func<CosmosClient> Client { get; }
    }
}