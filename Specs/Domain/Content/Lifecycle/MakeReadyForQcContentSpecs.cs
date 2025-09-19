using System;
using Fdb.Rx.Domain;
using NUnit.Framework;
using Sensemaking.Bdd;

namespace Sensemaking.Specs.Domain.Lifecycle;

public class MakeReadyForQcContentSpecs : CommonLifecycleSteps
{
    [Test]
    public void making_content_ready_for_qc()
    {
        Given(new_content_is_created_by(charlotte_bronte));
        When(it_is_made_ready_for_qc_by(charlotte_bronte));
        Then(make_ready_for_qc_exits(charlotte_bronte));
    }

    [Test]
    public void content_must_be_edited_to_be_made_ready_for_qc()
    {
        Given(some_live_content);
        When(it_is_made_ready_for_qc_by(charlotte_bronte));
        Then(() => informs<ValidationException>("Content must have been edited to be made ready for qc."));
    }

    [Test]
    public void retired_content_cant_be_made_ready_for_qc()
    {
        Given(some_retired_content);
        When(it_is_made_ready_for_qc_by(human_user));
        Then(() => informs<ValidationException>("Retired content cannot be made ready for qc."));
    }

    [Test]
    public void system_user_cannot_make_content_ready_for_qc()
    {
        Given(new_content_is_created_by(charlotte_bronte));
        When(it_is_made_ready_for_qc_by(system_user));
        Then(() => informs<ValidationException>("System users cannot make content ready for qc."));
    }

    protected Action make_ready_for_qc_exits(User last_human_editor)
    {
        var not_last_human_editor = last_human_editor == charlotte_bronte ? jane_austin : charlotte_bronte;
        return () =>
        {
            content.HasTransition(Transitions.Qc).should_be_true();
            content.HasTransitionFor(not_last_human_editor, Transitions.Qc).should_be_true();
            content.HasTransitionFor(last_human_editor, Transitions.Qc).should_be_false();
            content.HasTransitionFor(system_user, Transitions.Qc).should_be_false();
            And(event_is_raised(Transitions.MakeReadyForQc));
        };
    }
}