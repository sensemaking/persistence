using System;
using Sensemaking.Persistence.Cosmos.Security.KeyAccess;
using Sensemaking.Bdd;
using Microsoft.Azure.Cosmos;
using Sensemaking.Bdd;
using Database = Sensemaking.Persistence.Cosmos.Database;

namespace Sensemaking.Specs.Persistence.Cosmos;

public partial class DatabaseSpecs
{
    private CosmosClient first_cosmos_client;
    private CosmosClient second_cosmos_client;

    protected override void before_each()
    {
        base.before_each();
        first_cosmos_client = null;
        second_cosmos_client = null;
    }

    private void we_configure_cosmos_again()
    {
        Database.Configure(new KeyAccessCosmosConnection(Settings.CosmosDb.Endpoint, Settings.CosmosDb.AccessKey), Settings.CosmosDb.Database);
    }

    private void we_get_the_cosmos_client()
    {
        first_cosmos_client = Database.GetClient();
    }

    private void we_get_the_cosmos_client_again()
    {
        second_cosmos_client = Database.GetClient();
    }

    private void the_cosmos_client_is_the_same()
    {
        ReferenceEquals(first_cosmos_client, second_cosmos_client).should_be_true();
    }

    private void the_cosmos_client_is_not_the_same()
    {
        ReferenceEquals(first_cosmos_client, second_cosmos_client).should_be_false();
    }
}