using System;
using System.Data;
using System.Threading.Tasks;
using Fdb.Rx.Domain;
using Fdb.Rx.Domain.Events;
using Fdb.Rx.Persistence.Dapper;
using NSubstitute;
using NUnit.Framework;
using Sensemaking;
using Sensemaking.Bdd;
using Sensemaking.Monitoring;
using Serilog;

namespace Sensemaking.Specs.Domain;

[TestFixture]
public partial class RepositorySpecs
{
    private const string collection = "test_collection";
    private ILogger logger;
    protected IRepositories the_repositories;
    protected StubAggregate the_aggregate;
    private Action domain_delete_handle;
    private Action<StubAggregate> domain_change_handle;
    private Action domain_event_handle;
    private StubValidator<StubAggregate> validator;
    private readonly IDb db = DbFactory.Create(Startup.Database.connection_string);
    private IPersist the_persistence;

    protected override void before_all()
    {
        base.before_all();
        db.Execute($"CREATE TABLE {collection} (Id varchar(200), Document varchar(MAX))", commandType: CommandType.Text);
    }


    protected override void after_all()
    {
        db.Execute($"DROP TABLE {collection}", commandType: CommandType.Text);
        base.after_all();
    }

    protected override void before_each()
    {
        base.before_each();
        logger = Substitute.For<ILogger>();
        Logging.Configure(new MonitorInfo("Fake", "Monitor"), logger);
        var deleteHandler = new FakeDeletedDomainEventHandler<StubAggregate>(domain_delete_handle = Substitute.For<Action>());
        var changeHandler = new FakeChangedDomainEventHandler<StubAggregate>(domain_change_handle = Substitute.For<Action<StubAggregate>>());
        var eventHandler = new FakeDomainEventHandler<StubAggregate.StubEvent>(domain_event_handle = Substitute.For<Action>());

        validator = new StubValidator<StubAggregate>();
        the_repositories = RepositoryBuilder.For.Dapper(db).Register(collection, validator)
            .Handling(() => new IHandleDomainEvents[] { deleteHandler, changeHandler, eventHandler })
            .Get();
    }

    private void persistence_is_mocked()
    {
        the_persistence = Substitute.For<IPersist>();
        the_repositories.Repository.SetReflectedValue("persistence", the_persistence);

        the_persistence.Get<StubAggregate>(Arg.Any<string>()).Returns(Task.FromResult<StubAggregate>(null), Task.FromResult(the_aggregate));
        var aggregateRegistration = new AggregateRegistration();
        aggregateRegistration.Register<StubAggregate>(collection, null);
        the_persistence.GetTypeRegistration().Returns(aggregateRegistration);
    }

    private void an_aggregate()
    {
        the_aggregate = new StubAggregate("edited content");
    }

    private void a_collection_validator_for_the_aggregate()
    {
        validator.FailValidation = true;
    }

    private void a_domain_event_that_records_metrics()
    {
        var eventHandler = new MetricRecordingChangedDomainEventHandler<StubAggregate>(domain_change_handle = Substitute.For<Action<StubAggregate>>());
        the_repositories = RepositoryBuilder.For.Dapper(db)
            .Register<StubAggregate>(collection, null)
            .Handling(() => new IHandleDomainEvents[] { eventHandler })
            .Get();
    }

    private void a_domain_event_that_does_not_record_metrics() { }

    private void it_raises_a_domain_event()
    {
        the_aggregate.RaiseEvent();
    }

    private void it_is_registered() { }

    private void to_register_it_again()
    {
        trying(() => RepositoryBuilder.For.Dapper(db).Register<StubAggregate>(collection, null).Register<StubAggregate>(collection, null));
    }

    protected void saving_the_aggregate()
    {
        the_repositories.Repository.Save(the_aggregate).Await();
    }

    private void saving_the_aggregate_again()
    {
        saving_the_aggregate();
    }

    protected void deleting_the_aggregate()
    {
        the_repositories.Repository.Delete(the_aggregate).Await();
    }

    protected void deleting_the_aggregate_by_id()
    {
        the_repositories.Repository.Delete<StubAggregate>(the_aggregate.Id.ToString()).Await();
    }

    private void it_is_saved()
    {
        the_repositories.Repository.Get<StubAggregate>(the_aggregate.Id.ToString()).Await().Content.should_be(the_aggregate.Content);
    }

    private void it_is_deleted()
    {
        the_repositories.Repository.Get<StubAggregate>(the_aggregate.Id.ToString()).Await().should_be_null();
    }

    private void a_delete_event_is_dispatched()
    {
        domain_delete_handle.Received().Invoke();
    }

    private void the_event_is_dispatched_once()
    {
        domain_change_handle.Received(1).Invoke(Arg.Is<StubAggregate>(a => a.Equals(the_aggregate)));
    }

    private void it_is_not_saved_again()
    {
        the_persistence.Received(1).Persist(Arg.Is<StubAggregate>((agg) => agg.Content == the_aggregate.Content));
    }

    private void the_repository_is_available()
    {
        the_repositories.Repository.GetReflectedValue<IDispatchDomainEvents>("dispatcher").Repositories.should_be(the_repositories);
    }

    private void the_event_is_dispatched()
    {
        domain_event_handle.Received(1).Invoke();
    }

    private void the_domain_event_handler_metric_is_logged()
    {
        logger.should_have_logged_information();
    }

    private void the_domain_event_handler_metric_is_not_logged()
    {
        logger.should_not_have_logged_information();
    }
}