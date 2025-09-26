using System;
using Sensemaking.Bdd;
using NUnit.Framework;
using Sensemaking.Bdd;

namespace Sensemaking.Specs.Domain;

[TestFixture]
public partial class ContentRepositorySpecs : Specification
{
    [Test]
    public void saving_saves_the_content()
    {
        Given(an_aggregate);
        And(another_aggregate);
        And(a_thired_aggregate);
        When(saving_the_aggregates);
        Then(it_is_saved);

        When(getting_the_aggregates);
        Then(the_aggregates_are_returned);

        When(getting_all_aggregates);
        Then(all_aggregates_are_returned);
    }

    [Test]
    public void saving_unchanged_content_does_not_continue()
    {
        Given(an_aggregate);
        And(persistence_is_mocked);
        When(saving_the_aggregate);
        And(saving_the_aggregate_again);
        Then(the_event_is_dispatched_once);
        And(it_is_not_saved_again);
    }

    [Test]
    public void saving_applies_any_collection_validation()
    {
        Given(an_aggregate);
        And(a_collection_validator_for_the_aggregate);
        When(() => trying(saving_the_aggregate));
        Then(() => informs<ValidationException>($"{StubValidator<StubAggregate>.ErrorMessage} in collection {collection}"));
    }

    [Test]
    public void saving_dispatches_any_domain_events_raised_by_the_content()
    {
        Given(an_aggregate);
        And(it_raises_a_domain_event);
        When(saving_the_aggregate);
        Then(the_event_is_dispatched);
    }

    [Test]
    public void provides_a_repository_to_the_domain_event_dispatcher()
    {
        Given(an_aggregate);
        When(saving_the_aggregate);
        Then(the_domain_event_handler_can_use_the_repository);
    }

    [Test]
    public void content_type_can_only_be_registered_once()
    {
        Given(an_aggregate);
        And(it_is_registered);
        When(() => trying(to_register_it_again));
        Then(() => informs<ValidationException>("Aggregate already registered."));
    }
}