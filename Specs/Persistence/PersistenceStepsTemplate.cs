using Fdb.Rx.Domain;
using Fdb.Rx.Test;
using System;
using System.Linq;
using Sensemaking.Bdd;

namespace Fdb.Rx.Testing.Persistence;

public abstract partial class PersistenceSpecsTemplate(Func<IRepository> repositoryFactory)
{
    protected const string container_name = "MyAggregateContainer";
    protected const string aggregate_content = "I am some of the most interesting content ever scribed.";
    
    private IRepository the_repository;
    protected AnAggregate the_aggregate;
    protected AnAggregate other_aggregate;
    protected AnAggregate third_aggregate;
    protected AnAggregate the_result;
    protected AnAggregate[] the_results;
    
    protected override void before_each()
    {
        base.before_each();
        the_repository = repositoryFactory();
        the_aggregate = new AnAggregate(Guid.NewGuid().ToString(), aggregate_content);
        other_aggregate = new AnAggregate(Guid.NewGuid().ToString(), $"other {aggregate_content}");
        third_aggregate = new AnAggregate(Guid.NewGuid().ToString(), $"third {aggregate_content}");
        the_result = default;
        the_results = default;
    }

    protected void an_aggregate()
    {
        the_repository.Save(the_aggregate).Await();
    }

    protected void another_aggregate()
    {
        the_repository.Save(other_aggregate).Await();
    }

    protected void a_third_aggregate()
    {
        the_repository.Save(third_aggregate).Await();
    }
    
    protected void it_is_deleted()
    {
        the_repository.Delete(the_aggregate).Await();
    }

    protected void getting_the_aggregate()
    {
        the_result = the_repository.Get<AnAggregate>(the_aggregate.Id).Await();
    }

    protected void getting_the_aggregates()
    {
        the_results = the_repository.Get<AnAggregate>(the_aggregate.Id, other_aggregate.Id).Await();
    }

    protected void getting_all_aggregates()
    {
        the_results = the_repository.GetAll<AnAggregate>().Await();
    }
    
    protected void the_aggregate_is_retrieved()
    {
        the_result.Id.should_be(the_aggregate.Id);
    }

    protected void the_aggregate_is_not_retrieved()
    {
        the_result.should_be_null();
    }

    protected void the_aggregates_are_retrieved()
    {
        the_results.Length.should_be(2);
        the_results.should_contain(the_aggregate);
        the_results.should_contain(other_aggregate);
    }

    protected void all_aggregates_are_retrieved()
    {
        the_results.Length.should_be(3);
        the_results.should_contain(the_aggregate);
        the_results.should_contain(other_aggregate);
        the_results.should_contain(third_aggregate);
    }
}