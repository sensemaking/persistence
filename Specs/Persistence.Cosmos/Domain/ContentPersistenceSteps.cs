using System.Collections.Generic;
using Fdb.Rx.Persistence.Cosmos;
using Fdb.Rx.Persistence.Cosmos.Security.KeyAccess;
using NUnit.Framework;
using Database = Fdb.Rx.Persistence.Cosmos.Database;

namespace Sensemaking.Specs.Persistence.Cosmos.Domain;

[TestFixture]
public class ContentPersistenceSteps() : ContentPersistenceSpecsTemplate(() => new CosmosPersistence())
{
    private const string publication_table_suffix = "-as-published";

    protected override void before_each()
    {
        base.before_each();
        var client = Settings.CosmosDb.GetCosmosClient();
        var db = client.EnsureDatabase(Settings.CosmosDb.Database);
        db.EnsureContainer(container_name);
        db.EnsureContainer(container_name + publication_table_suffix);
    }
}