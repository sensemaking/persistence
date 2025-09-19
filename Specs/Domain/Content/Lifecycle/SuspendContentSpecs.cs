using System;
using Sensemaking.Domain;
using NUnit.Framework;

namespace Sensemaking.Specs.Domain.Lifecycle;

public class SuspendContentSpecs : CommonLifecycleSteps
{
    private const bool ready_for_qc = true;
    private const bool not_ready_for_qc = false;

    [Test]
    public void qcd_content_can_be_suspended()
    {
        Given(some_live_content);
        And(it_is_changed_by(jane_austin));
        When(it_is_suspended_by(jane_austin));
        Then(suspend_exits(not_ready_for_qc));
    }

    [Test]
    public void live_content_that_is_ready_for_qc_can_be_qcd_once_suspended()
    {
        Given(some_live_content);
        And(it_is_changed_by(jane_austin));
        And(it_is_made_ready_for_qc_by(charlotte_bronte));
        When(it_is_suspended_by(jane_austin));
        Then(suspend_exits(ready_for_qc));
    }

    [Test]
    public void live_content_without_edits_can_be_made_ready_for_qc_once_suspended()
    {
        Given(some_live_content);
        When(it_is_suspended_by(jane_austin));
        Then(suspend_exits(not_ready_for_qc));
    }

    [Test]
    public void new_content_can_not_be_suspended()
    {
        Given(new_content_is_created_by(charlotte_bronte));
        When(it_is_suspended_by(human_user));
        Then(() => informs<ValidationException>("New content cannot be suspended."));

    }

    [Test]
    public void retired_content_can_not_be_suspended()
    {
        Given(new_content_is_created_by(charlotte_bronte));
        And(it_is_retired_by(human_user));
        When(it_is_suspended_by(human_user));
        Then(() => informs<ValidationException>("Retired content cannot be suspended."));
    }

    [Test]
    public void system_users_can_suspend_content_they_created()
    {
        scenario(() =>
        {
            Given(new_content_is_created_by(system_user));
            When(it_is_suspended_by(system_user));
            Then(suspend_exits(not_ready_for_qc));
        });

        scenario(() =>
        {
            Given(some_live_content);
            When(it_is_suspended_by(system_user));
            Then(() => informs<ValidationException>("System users cannot suspend content unless they created it."));
        });

        scenario(() =>
        {
            Given(new_content_is_created_by(system_user));
            When(it_is_suspended_by(another_system_user));
            Then(() => informs<ValidationException>("System users cannot suspend content unless they created it."));
        });
    }

    private Action suspend_exits(bool isReadyForQc)
    {
        return () =>
        {
            Then(it_moves_to(ContentLifecycles.Suspended));
            And(it_is_no_longer_live);
            And(it_has_transitions(isReadyForQc ? Transitions.Change | Transitions.Qc | Transitions.Retire : Transitions.Change | Transitions.MakeReadyForQc | Transitions.Retire));
            And(event_is_raised(Transitions.Suspend));
        };
    }
}