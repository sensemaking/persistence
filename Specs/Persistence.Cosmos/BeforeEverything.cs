using Fdb.Rx.Persistence.Cosmos.Security.KeyAccess;
using Fdb.Rx.Test.Cosmos;
using Microsoft.Azure.Cosmos;
using NUnit.Framework;

namespace Sensemaking.Specs.Persistence.Cosmos;

[SetUpFixture]
public class BeforeEverything
{
    [OneTimeSetUp]
    public void Start()
    {
        CosmosEmulator.Start(new CosmosClient(Settings.CosmosDb.Endpoint, Settings.CosmosDb.AccessKey));
        Fdb.Rx.Persistence.Cosmos.Database.Configure(new KeyAccessCosmosConnection(Settings.CosmosDb.Endpoint, Settings.CosmosDb.AccessKey), Settings.CosmosDb.Database);
    }
}