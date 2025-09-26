using Sensemaking.Persistence.Cosmos.Security.KeyAccess;
using Microsoft.Azure.Cosmos;
using NUnit.Framework;
using Sensemaking.Cosmos.Bdd;

namespace Sensemaking.Specs.Persistence.Cosmos;

[SetUpFixture]
public class BeforeEverything
{
    [OneTimeSetUp]
    public void Start()
    {
        CosmosEmulator.Start(new CosmosClient(Settings.CosmosDb.Endpoint, Settings.CosmosDb.AccessKey));
        Sensemaking.Persistence.Cosmos.Database.Configure(new KeyAccessCosmosConnection(Settings.CosmosDb.Endpoint, Settings.CosmosDb.AccessKey), Settings.CosmosDb.Database);
    }
}