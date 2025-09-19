using System;
using Fdb.Rx.Test;
using NUnit.Framework;
using Sensemaking.Bdd;

namespace Sensemaking.Specs.Domain;

[TestFixture]
public partial class RepositorySpecs : Specification
{
    [Test]
    public void saving_saves_the_aggregate()
    {
        Given(an_aggregate);
        When(saving_the_aggregate);
        Then(it_is_saved);
    }

    [Test]
    public void saving_an_unchanged_saved_aggregate_does_not_fire_its_changed_domain_event()
    {
        Given(an_aggregate);
        And(persistence_is_mocked);
        When(saving_the_aggregate);
        And(saving_the_aggregate_again);
        Then(the_event_is_dispatched_once);
        And(it_is_not_saved_again);
    }

    [Test]
    public void deleting_an_aggregate()
    {
        scenario(() =>
        {
            Given(an_aggregate);
            When(saving_the_aggregate);
            And(deleting_the_aggregate);
            Then(it_is_deleted);
            And(a_delete_event_is_dispatched);
        });

        scenario(() =>
        {
            Given(an_aggregate);
            When(saving_the_aggregate);
            And(deleting_the_aggregate_by_id);
            Then(it_is_deleted);
            And(a_delete_event_is_dispatched);
        });
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
    public void saving_dispatches_any_domain_events_raised_by_the_aggregate()
    {
        Given(an_aggregate);
        And(it_raises_a_domain_event);
        When(saving_the_aggregate);
        Then(the_event_is_dispatched);
    }

    [Test]
    public void domain_event_handler_metric_logging()
    {
        scenario(() =>
        {
            Given(an_aggregate);
            And(a_domain_event_that_records_metrics);
            When(saving_the_aggregate);
            Then(the_domain_event_handler_metric_is_logged);
        });

        scenario(() =>
        {
            Given(an_aggregate);
            And(a_domain_event_that_does_not_record_metrics);
            When(saving_the_aggregate);
            Then(the_domain_event_handler_metric_is_not_logged);
        });
    }

    [Test]
    public void provides_itself_to_the_domain_event_dispatcher()
    {
        Then(the_repository_is_available);
    }

    [Test]
    public void aggregate_can_only_be_registered_once()
    {
        Given(an_aggregate);
        And(it_is_registered);
        When(() => trying(to_register_it_again));
        Then(() => informs<ValidationException>("Aggregate already registered."));
    }
}