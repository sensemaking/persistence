using System;
using Sensemaking.Domain;
using NUnit.Framework;
using Sensemaking.Bdd;

namespace Sensemaking.Specs.Domain.Lifecycle;

public class ChangingContentSpecs : CommonLifecycleSteps
{
    [Test]
    public void content_changing()
    {
        scenario(new_content_changed_by_a_human);
        scenario(new_content_changed_by_the_system);
        scenario(live_content_changed_by_a_human);
        scenario(live_content_changed_by_the_system);
        scenario(suspended_content_changed_by_a_human);
        scenario(suspended_content_changed_by_the_system);
        scenario(retired_content_changed_by_a_human);
        scenario(retired_content_changed_by_the_system);
    }

    [Test]
    public void humans_changing_content_that_is_ready_for_qc_means_it_is_no_longer_ready_for_qc()
    {
        Given(some_ready_for_qc_content);
        When(it_is_changed_by(jane_austin));
        Then(changes_made(ContentLifecycles.New, jane_austin));
    }

    [Test]
    public void system_changes_after_human_changes_retains_whether_or_not_the_content_is_ready_for_qc()
    {
        scenario(any_content_made_ready_for_qc);
        scenario(any_content_not_yet_made_ready_for_qc);
    }

    [Test]
    public void is_ready_for_qc_if_only_system_changes_have_occured()
    {
        Given(some_live_content);
        When(it_is_changed_by(system_user));
        Then(is_ready_for_qc(true));
        Then(changes_made(ContentLifecycles.Live, system_user, null));
    }

    private void new_content_changed_by_a_human()
    {
        Given(new_content_is_created_by(charlotte_bronte));
        When(it_is_changed_by(jane_austin));
        Then(changes_made(ContentLifecycles.New, jane_austin));
    }

    private void new_content_changed_by_the_system()
    {
        Given(new_content_is_created_by(charlotte_bronte));
        When(it_is_changed_by(system_user));
        Then(changes_made(ContentLifecycles.New, system_user, charlotte_bronte));
    }

    private void live_content_changed_by_a_human()
    {
        Given(some_live_content);
        When(it_is_changed_by(jane_austin));
        Then(changes_made(ContentLifecycles.Live, jane_austin));
    }

    private void live_content_changed_by_the_system()
    {
        Given(some_live_content);
        When(it_is_changed_by(system_user));
        Then(is_ready_for_qc(true));
        Then(changes_made(ContentLifecycles.Live, system_user, null));
    }

    private void suspended_content_changed_by_a_human()
    {
        Given(some_suspended_content);
        When(it_is_changed_by(jane_austin));
        Then(changes_made(ContentLifecycles.Suspended, jane_austin));
    }

    private void suspended_content_changed_by_the_system()
    {
        Given(some_suspended_content);
        When(it_is_changed_by(system_user));
        Then(changes_made(ContentLifecycles.Suspended, system_user, null));
    }

    private void retired_content_changed_by_a_human()
    {
        Given(some_retired_content);
        When(it_is_changed_by(jane_austin));
        Then(() => informs<ValidationException>("Reactivate content before making changes."));
    }

    private void retired_content_changed_by_the_system()
    {
        Given(some_retired_content);
        When(it_is_changed_by(system_user));
        Then(changes_made(ContentLifecycles.Retired, system_user, null));
    }

    private void any_content_made_ready_for_qc()
    {
        Given(some_ready_for_qc_content);
        When(it_is_changed_by(system_user));
        Then(is_ready_for_qc(true));
    }

    private void any_content_not_yet_made_ready_for_qc()
    {
        Given(new_content_is_created_by(charlotte_bronte));
        When(it_is_changed_by(system_user));
        Then(is_ready_for_qc(false));
    }

    protected Action changes_made(ContentLifecycles initialLifecycle, User changed_by)
    {
        return changes_made(initialLifecycle, changed_by, changed_by);
    }

    protected Action changes_made(ContentLifecycles initialLifecycle, User changed_by, User? last_human_editor)
    {
        return () =>
        {
            Then(it_remains(initialLifecycle));
            And(it_was_edited_by(changed_by));
            And(the_last_human_editor_is(last_human_editor));
            And(event_is_raised(Transitions.Change));

            if (changed_by.IsHuman || has_previous_human_edits())
                And(it_must_be_made_ready_for_qc(changed_by));
            else
                And(it_is_automatically_ready_for_qc);
        };
    }

    private void it_is_automatically_ready_for_qc()
    {
        And(is_ready_for_qc(true));
        And(is_ready_for_qc_for(human_user, true));
    }

    private Action it_must_be_made_ready_for_qc(User changed_by)
    {
        return () =>
        {
            the_last_human_editor_is(changed_by);
            And(is_ready_for_qc(false));
        };
    }

    private Action is_ready_for_qc(bool readyForQc)
    {
        return () =>
        {
            if (content.Lifecycle == ContentLifecycles.Retired)
            {
                content.HasTransition(Transitions.MakeReadyForQc).should_be_false();
                content.HasTransition(Transitions.Qc).should_be_false();
                return;
            }

            if (readyForQc)
            {
                content.HasTransition(Transitions.MakeReadyForQc).should_be_false();
                content.HasTransition(Transitions.Qc).should_be_true();
            }
            else
            {
                content.HasTransition(Transitions.MakeReadyForQc).should_be_true();
                content.HasTransition(Transitions.Qc).should_be_false();
            }
        };
    }

    private Action is_ready_for_qc_for(User user, bool readyForQc)
    {
        return () =>
        {
            if (content.Lifecycle == ContentLifecycles.Retired)
            {
                content.HasTransitionFor(user, Transitions.MakeReadyForQc).should_be_false();
                content.HasTransitionFor(user, Transitions.Qc).should_be_false();
                return;
            }

            if (readyForQc)
            {
                content.HasTransitionFor(user, Transitions.MakeReadyForQc).should_be_false();
                content.HasTransitionFor(user, Transitions.Qc).should_be_true();
            }
            else
            {
                content.HasTransitionFor(user, Transitions.MakeReadyForQc).should_be_true();
                content.HasTransitionFor(user, Transitions.Qc).should_be_false();
            }
        };
    }
}