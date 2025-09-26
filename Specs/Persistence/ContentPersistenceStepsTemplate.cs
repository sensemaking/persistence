using System;
using System.Linq;
using Sensemaking.Domain;
using Sensemaking.Bdd;

namespace Sensemaking.Specs.Persistence;

public abstract partial class ContentPersistenceSpecsTemplate
{
    protected const string container_name = "MyContentContainer";
    protected const string aggregate_content = "I am some of the most interesting content ever scribed.";

    private readonly Func<IPersist> persistenceFactory;

    private IPersist persistence;
    protected StubContent the_content;
    protected StubContent the_second_content;
    protected StubContent the_third_content;
    protected StubContent the_result;
    protected StubContent[] the_results;

    internal ContentPersistenceSpecsTemplate(Func<IPersist> persistenceFactory)
    {
        this.persistenceFactory = persistenceFactory;
    }

    protected override void before_each()
    {
        base.before_each();

        persistence = persistenceFactory();
        persistence.GetTypeRegistration().Register<StubContent>(container_name, null);
        the_content = new StubContent(Guid.NewGuid().ToString(), aggregate_content);
        the_second_content = new StubContent(Guid.NewGuid().ToString(), aggregate_content);
        the_third_content = new StubContent(Guid.NewGuid().ToString(), aggregate_content);
        the_result = default;
        the_results = default;
    }

    protected void some_content()
    {
        persistence.Persist(the_content).Await();
    }

    protected void second_content()
    {
        persistence.Persist(the_second_content).Await();
    }

    protected void third_content()
    {
        persistence.Persist(the_third_content).Await();
    }

    protected void it_is_removed()
    {
        persistence.Remove(the_content).Await();
    }

    protected void getting_the_content()
    {
        the_result = persistence.Get<StubContent>(the_content.Id).Await();
    }

    private void getting_some_aggregates()
    {
        the_results = persistence.Get<StubContent>(the_content.Id, the_second_content.Id).Await();
    }

    private void getting_all_aggregates()
    {
        the_results = persistence.GetAll<StubContent>().Await();
    }

    protected void getting_live_content()
    {
        the_result = persistence.GetLive<StubContent>(the_content.Id).Await();
    }

    private void getting_all_live_content()
    {
        the_result = persistence.GetAllLive<StubContent>().Await().SingleOrDefault();
    }

    protected void the_content_is_retrieved()
    {
        the_result.Id.should_be(the_content.Id);
    }

    protected void the_content_is_not_retrieved()
    {
        the_result.should_be_null();
    }

    private void it_is_persisted_as_live()
    {
        persistence.PersistAsLive(the_content).Await();
    }

    private void it_is_removed_from_live()
    {
        persistence.RemoveFromLive(the_content).Await();
    }

    private void the_live_content_is_retrieved()
    {
        the_result.Text.should_be(aggregate_content);
    }

    private void the_live_content_is_not_retrieved()
    {
        the_result.should_be_null();
    }

    private void the_aggregates_are_returned()
    {
        the_results.Length.should_be(2);
        the_results.should_contain(the_content);
        the_results.should_contain(the_second_content);
    }

    private void all_aggregates_are_returned()
    {
        the_results.Length.should_be(3);
        the_results.should_contain(the_content);
        the_results.should_contain(the_second_content);
        the_results.should_contain(the_third_content);
    }
}