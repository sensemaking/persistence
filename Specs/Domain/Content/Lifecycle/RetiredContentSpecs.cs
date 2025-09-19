using System;
using Sensemaking.Domain;
using NUnit.Framework;

namespace Sensemaking.Specs.Domain.Lifecycle;

public class RetiredContentSpecs : CommonLifecycleSteps
{

    [Test]
    public void content_can_be_retired()
    {
        scenario(new_content_can_be_retired);
        scenario(live_content_can_be_retired);
        scenario(suspended_content_can_be_retired);
    }

    [Test]
    public void ready_for_qc_content_is_made_not_ready_for_qc()
    {
        Given(some_new_content);
        And(it_is_made_ready_for_qc_by(jane_austin));
        When(it_is_retired_by(human_user));
        Then(retire_exits);
    }

    private void new_content_can_be_retired()
    {
        Given(some_new_content);
        When(it_is_retired_by(human_user));
        Then(retire_exits);
    }

    private void live_content_can_be_retired()
    {
        Given(some_live_content);
        When(it_is_retired_by(human_user));
        Then(it_is_no_longer_live);
        And(retire_exits);
    }

    private void suspended_content_can_be_retired()
    {
        Given(some_suspended_content);
        When(it_is_retired_by(human_user));
        Then(retire_exits);
    }

    private void retire_exits()
    {
        Then(it_moves_to(ContentLifecycles.Retired));
        And(it_has_transitions(Transitions.Change | Transitions.Reactivate));
        And(it_has_transitions_for(system_user, Transitions.Change));
        And(event_is_raised(Transitions.Retire));
    }

    [Test]
    public void system_users_can_retire_content_they_created()
    {
        scenario(() =>
        {
            Given(new_content_is_created_by(system_user));
            When(it_is_retired_by(system_user));
            Then(retire_exits);
        });

        scenario(() =>
        {
            Given(some_new_content);
            When(it_is_retired_by(system_user));
            Then(() => informs<ValidationException>("System users cannot retire content unless they created it."));
        });

        scenario(() =>
        {
            Given(new_content_is_created_by(system_user));
            When(it_is_retired_by(another_system_user));
            Then(() => informs<ValidationException>("System users cannot retire content unless they created it."));
        });

        scenario(() =>
        {
            Given(some_live_content);
            When(it_is_retired_by(system_user));
            Then(() => informs<ValidationException>("System users cannot retire content unless they created it."));
        });
    }
}