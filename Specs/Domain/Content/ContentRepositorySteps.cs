using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Fdb.Rx.Domain;
using Fdb.Rx.Domain.Events;
using Fdb.Rx.Persistence.Dapper;
using NSubstitute;
using NUnit.Framework;
using Sensemaking.Bdd;

namespace Sensemaking.Specs.Domain;

[TestFixture]
public partial class ContentRepositorySpecs
{
    private const string collection = "test_collection";
    private const string published_collection = $"{collection}AsPublished";
    private const string non_content_collection = "non_content_collection";
    protected IContentRepository the_repository;
    protected StubContent content;
    protected StubContent more_content;
    protected StubContent even_more_content;
    protected StubContent[] content_returned;
    private Action domain_delete_handle;
    private Action<IRepositories> domain_change_handle;
    private Action domain_event_handle;
    private StubValidator<StubContent> validator;
    private readonly IDb db = DbFactory.Create(Startup.Database.connection_string);
    private IPersist the_persistence;
    private readonly User charlotte_bronte = new(Guid.NewGuid(), "charlotte bronte");


    protected override void before_all()
    {
        base.before_all();
        db.Execute($"CREATE TABLE {collection} (Id varchar(200), Document varchar(MAX))", commandType: CommandType.Text);
        db.Execute($"CREATE TABLE {published_collection} (Id varchar(200), Document varchar(MAX))", commandType: CommandType.Text);
        db.Execute($"CREATE TABLE {non_content_collection} (Id varchar(200), Document varchar(MAX))", commandType: CommandType.Text);
    }

    protected override void after_all()
    {
        db.Execute($"DROP TABLE {collection}", commandType: CommandType.Text);
        db.Execute($"DROP TABLE {published_collection}", commandType: CommandType.Text);
        db.Execute($"DROP TABLE {non_content_collection}", commandType: CommandType.Text);
        base.after_all();
    }

    protected override void before_each()
    {
        base.before_each();
        var changeHandler = new FakeChangedDomainEventHandlerWithRepository<StubContent>(domain_change_handle = Substitute.For<Action<IRepositories>>());
        var deleteHandler = new FakeDeletedDomainEventHandler<StubContent>(domain_delete_handle = Substitute.For<Action>());
        var eventHandler = new FakeDomainEventHandler<StubContent.StubEvent>(domain_event_handle = Substitute.For<Action>());
        validator = new StubValidator<StubContent>();
        the_repository = RepositoryBuilder.For.Dapper(db)
            .Register(collection, validator)
            .Register<StubAggregate>(non_content_collection, null)
            .Handling(() => new IHandleDomainEvents[] { deleteHandler, changeHandler, eventHandler })
            .Get().Content;

        db.Execute($"TRUNCATE TABLE {collection}", commandType: CommandType.Text);
        db.Execute($"TRUNCATE TABLE {published_collection}", commandType: CommandType.Text);
        db.Execute($"TRUNCATE TABLE {non_content_collection}", commandType: CommandType.Text);
    }

    private void persistence_is_mocked()
    {
        the_persistence = Substitute.For<IPersist>();
        the_repository.SetReflectedValue("persistence", the_persistence);
        the_persistence.Get<StubContent>(Arg.Any<string>()).Returns(Task.FromResult<StubContent>(null), Task.FromResult(content));
        var aggregateRegistration = new AggregateRegistration();
        aggregateRegistration.Register<StubContent>(collection, null);
        the_persistence.GetTypeRegistration().Returns(aggregateRegistration);
    }

    private void an_aggregate()
    {
        content = new StubContent("edited content", charlotte_bronte);
    }

    private void another_aggregate()
    {
        more_content = new StubContent("edited content", charlotte_bronte);
    }

    private void a_thired_aggregate()
    {
        even_more_content = new StubContent("edited content", charlotte_bronte);
    }

    private void a_collection_validator_for_the_aggregate()
    {
        validator.FailValidation = true;
    }

    private void it_raises_a_domain_event()
    {
        content.RaiseEvent();
    }

    private void it_is_registered() { }

    private void to_register_it_again()
    {
        trying(() => RepositoryBuilder.For.Dapper(db).Register<StubContent>(collection, null).Register<StubContent>(collection, null));
    }

    protected void saving_the_aggregate()
    {
        the_repository.Save(content, charlotte_bronte).Await();
    }

    private void saving_the_aggregates()
    {
        the_repository.Save(content, charlotte_bronte).Await();
        the_repository.Save(more_content, charlotte_bronte).Await();
        the_repository.Save(even_more_content, charlotte_bronte).Await();
    }

    private void saving_the_aggregate_again()
    {
        saving_the_aggregate();
    }

    private void getting_the_aggregates()
    {
        content_returned = the_repository.Get<StubContent>(content.Id.ToString(), more_content.Id.ToString()).Await();
    }

    private void getting_all_aggregates()
    {
        content_returned = the_repository.GetAll<StubContent>().Await();
    }

    private void it_is_saved()
    {
        the_repository.Get<StubContent>(content.Id.ToString()).Await().Text.should_be(content.Text);
    }

    private void the_event_is_dispatched_once()
    {
        domain_change_handle.Received(1).Invoke(Arg.Any<IRepositories>());
    }

    private void it_is_not_saved_again()
    {
        the_persistence.Received(1).Persist(Arg.Is<StubContent>((agg) => agg.Text == content.Text));
    }

    private void the_domain_event_handler_can_use_the_repository()
    {
        var repositories = (IRepositories)domain_change_handle.ReceivedCalls().ToArray().Single().GetArguments().Single();
        var saved_aggregate = new StubAggregate();
        repositories.Repository.Save(saved_aggregate).Await();
        var aggregate = repositories.Repository.Get<StubAggregate>(saved_aggregate.Id.ToString()).Await();
        aggregate.Content.should_be(saved_aggregate.Content);

    }

    private void the_event_is_dispatched()
    {
        domain_event_handle.Received(1).Invoke();
    }

    internal class FakeChangedDomainEventHandlerWithRepository<T> : DomainEventHandler<Changed<T>> where T : IAggregate
    {
        private readonly Action<IRepositories> HandleEvent;

        public FakeChangedDomainEventHandlerWithRepository(Action<IRepositories> handleEvent)
        {
            HandleEvent = handleEvent;
        }

        public override void Handle(Changed<T> domainEvent)
        {
            HandleEvent(Repositories);
        }
    }

    private void the_aggregates_are_returned()
    {
        content_returned.Length.should_be(2);
        content_returned.should_contain(content);
        content_returned.should_contain(more_content);
    }

    private void all_aggregates_are_returned()
    {
        content_returned.Length.should_be(3);
        content_returned.should_contain(content);
        content_returned.should_contain(more_content);
        content_returned.should_contain(even_more_content);
    }
}
