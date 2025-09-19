using NUnit.Framework;
using Sensemaking.Bdd;

namespace Fdb.Rx.Testing.Persistence;

public abstract partial class PersistenceSpecsTemplate : Specification
{
    [Test]
    public virtual void save_and_gets_aggregates()
    {
        Given(an_aggregate);
        And(another_aggregate);
        And(a_third_aggregate);
                
        When(getting_the_aggregate);
        Then(the_aggregate_is_retrieved);
                
        When(getting_the_aggregates);
        Then(the_aggregates_are_retrieved);
                
        When(getting_all_aggregates);
        Then(all_aggregates_are_retrieved);
    }

    [Test]
    public void deletes_aggregates()
    {
        Given(an_aggregate);
        And(it_is_deleted);
        When(getting_the_aggregate);
        Then(the_aggregate_is_not_retrieved);
    }
}