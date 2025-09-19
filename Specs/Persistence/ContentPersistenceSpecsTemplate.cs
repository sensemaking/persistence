using NUnit.Framework;
using Sensemaking.Bdd;

namespace Fdb.Rx.Testing.Persistence;

public abstract partial class ContentPersistenceSpecsTemplate: Specification
{
    [Test]
    public void persists_and_gets_content()
    {
        Given(some_content);
        And(second_content);
        And(third_content);
        When(getting_the_content);
        Then(the_content_is_retrieved);

        When(getting_some_aggregates);
        Then(the_aggregates_are_returned);
        
        When(getting_all_aggregates);
        Then(all_aggregates_are_returned);
        
        When(getting_live_content);
        Then(the_content_is_not_retrieved);

        When(getting_all_live_content);
        Then(the_content_is_not_retrieved);
    }

    [Test]
    public void removes_content()
    {
        Given(some_content);
        And(it_is_persisted_as_live);
        And(it_is_removed);
        When(getting_the_content);
        Then(the_content_is_not_retrieved);

        When(getting_live_content);
        Then(the_live_content_is_not_retrieved);

        When(getting_all_live_content);
        Then(the_live_content_is_not_retrieved);
    }

    [Test]
    public void gets_all_live_content()
    {
        Given(some_content);
        And(it_is_persisted_as_live);
        When(getting_live_content);
        Then(the_live_content_is_retrieved);

        When(getting_all_live_content);
        Then(the_live_content_is_retrieved);
    }

    [Test]
    public void removes_live_content()
    {
        Given(some_content);
        And(it_is_persisted_as_live);
        And(it_is_removed_from_live);
        When(getting_live_content);
        Then(the_live_content_is_not_retrieved);

        When(getting_all_live_content);
        Then(the_live_content_is_not_retrieved);
    }
}