using Fdb.Rx.Persistence.Cosmos;
using NUnit.Framework;

namespace Sensemaking.Specs.Persistence.Cosmos;

[SetUpFixture]
public class TestTeardown
{
    [OneTimeTearDown]
    public void Start()
    {
        Settings.CosmosDb.GetCosmosClient().EnsureDeleteDatabase(Settings.CosmosDb.Database);
    }
}