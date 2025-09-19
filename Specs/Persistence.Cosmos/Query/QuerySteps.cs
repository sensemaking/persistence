using System;
using System.Collections.Generic;
using System.Linq;
using Sensemaking.Domain;
using Sensemaking.Persistence.Cosmos;
using Sensemaking.Persistence.Cosmos.Query.Sql;
using Sensemaking.Persistence.Cosmos.Security.KeyAccess;
using Sensemaking.Test;
using Microsoft.Azure.Cosmos;
using Sensemaking.Bdd;
using Database = Sensemaking.Persistence.Cosmos.Database;
using User = Sensemaking.Domain.User;

namespace Sensemaking.Specs.Persistence.Cosmos.Query;

public abstract partial class QuerySpecs
{
    private IReadOnlyCollection<StubContent> all_aggregates;
    private IReadOnlyCollection<StubContent> expected_aggregates;
    private IEnumerable<StubContent> the_result;
    private QuerySpecification spec;

    private const string container = "QueryTest";

    private IContentRepository the_repository;

    protected override void before_each()
    {
        base.before_each();
        var client = new CosmosClient(Settings.CosmosDb.Endpoint, Settings.CosmosDb.AccessKey);
        var db = client.EnsureDatabase(Settings.CosmosDb.Database);
        db.EnsureContainer(container);
        the_repository = RepositoryBuilder
            .For.Cosmos(new KeyAccessCosmosConnection(Settings.CosmosDb.Endpoint, Settings.CosmosDb.AccessKey), Settings.CosmosDb.Database)
            .Register<StubContent>(container, null)
            .Get().Content;

        spec = new QuerySpecification(container, "select * from QueryTest WHERE QueryTest.id in (\"1\",\"3\")");
    }

    private void existing_documents()
    {
        expected_aggregates = new[]
        {
            new StubContent("1", "content1"),
            new StubContent("3", "content3"),
        };
        all_aggregates = new List<StubContent>(expected_aggregates)
        {
            new StubContent("2", "content1"),
        };
        foreach (var aggregate in all_aggregates)
        {
            the_repository.Save(aggregate, new User(Guid.NewGuid(), "")).Await();
        }
    }

    private void getting_results_via_query()
    {
        the_result = new QueryRunner().GetResults<StubContent>(spec.Container, spec.Query).Await();
    }

    private void documents_are_retrieved()
    {
        the_result.Count().should_be(expected_aggregates.Count);
        foreach (var aggregate in the_result)
        {
            expected_aggregates.Single(ex => ex.Id == aggregate.Id && ex.Text == aggregate.Text).should_not_be_null();
        }
    }
}