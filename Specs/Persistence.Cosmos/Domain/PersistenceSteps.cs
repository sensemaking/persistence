using Sensemaking.Domain;
using Sensemaking.Persistence.Cosmos;
using Sensemaking.Persistence.Cosmos.Security.KeyAccess;
using NUnit.Framework;

namespace Sensemaking.Specs.Persistence.Cosmos.Domain;

[TestFixture]
public class PersistenceSteps() : PersistenceSpecsTemplate(() => RepositoryBuilder.For
    .Cosmos(new KeyAccessCosmosConnection(Settings.CosmosDb.Endpoint, Settings.CosmosDb.AccessKey), Settings.CosmosDb.Database)
    .Register<AnAggregate>(container_name, null)
    .Get().Repository)
{
    protected override void before_each()
    {
        base.before_each();
        var client = Settings.CosmosDb.GetCosmosClient();
        var db = client.EnsureDatabase(Settings.CosmosDb.Database);
        db.EnsureContainer(container_name);
    }
}