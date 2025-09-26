using System.Serialization;
using Sensemaking.Bdd;
using NUnit.Framework;

namespace Sensemaking.Specs.Domain;

[TestFixture]
public partial class SubstituteRepositoriesSpecs : Specification
{
    [Test]
    public void get_provides_saved_aggregates()
    {
        scenario(gets_saved_aggregate);

        scenario(changes_to_saved_aggregate_after_save_are_not_reflected_in_the_get);
    }

    private void gets_saved_aggregate()
    {
        Given(an_aggregate_is_saved(stub_aggregate));
        And(an_aggregate_is_saved(stub_aggregate));
        And(an_aggregate_is_saved(another_stub_aggregate));
        And(an_aggregate_is_saved(a_third_stub_aggregate));

        When(when_getting(stub_aggregate));
        Then(the_aggregate_is_returned(stub_aggregate));

        When(when_getting(another_stub_aggregate));
        Then(the_aggregate_is_returned(another_stub_aggregate));

        When(when_getting_multiple(stub_aggregate, another_stub_aggregate));
        Then(the_aggregates_are_returned(stub_aggregate, another_stub_aggregate));

        When(when_getting_all);
        Then(all_aggregates_are_returned);
    }

    private void changes_to_saved_aggregate_after_save_are_not_reflected_in_the_get()
    {
        var original_aggregate_as_saved = stub_aggregate.Serialize().Deserialize<StubContent>();
        Given(an_aggregate_is_saved(stub_aggregate));
        And(the_aggregate_is_modified_in_memory_later(stub_aggregate));
        When(when_getting(stub_aggregate));
        Then(the_aggregate_is_returned(stub_aggregate));
        And(the_aggregate_has_the_same_contents(original_aggregate_as_saved));
    }

    [Test]
    public void get_returns_a_new_instance_of_any_saved_aggregate()
    {
        Given(an_aggregate_is_saved(stub_aggregate));
        And(when_getting(stub_aggregate));
        When(getting_it_again);
        Then(it_is_not_the_same_instance);
    }

    [Test]
    public void can_mock_on_saving()
    {
        Given(a_save_is_mocked_for(stub_aggregate));
        When(saving(stub_aggregate));
        Then(it_was_saved(stub_aggregate));
    }

    [Test]
    public void can_mock_on_deleting()
    {
        scenario(() =>
        {
            Given(a_delete_is_mocked_for(stub_aggregate));
            When(deleting(stub_aggregate));
            Then(it_was_deleted(stub_aggregate));
        });

        scenario(() =>
        {
            Given(a_delete_is_mocked_for(stub_aggregate));
            When(deleting_by_id(stub_aggregate));
            Then(it_was_deleted(stub_aggregate));
        });
    }
}