using Fdb.Rx.Persistence.Cosmos;
using NUnit.Framework;

namespace Fdb.Rx.Testing.Persistence.Cosmos;

[SetUpFixture]
public class TestTeardown
{
    [OneTimeTearDown]
    public void Start()
    {
        Settings.CosmosDb.GetCosmosClient().EnsureDeleteDatabase(Settings.CosmosDb.Database);
    }
}