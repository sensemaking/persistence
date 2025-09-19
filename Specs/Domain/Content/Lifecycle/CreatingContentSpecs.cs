using System;
using Fdb.Rx.Domain;
using NUnit.Framework;

namespace Fdb.Rx.Testing.Domain.Lifecycle;

public class CreatingContentSpecs : CommonLifecycleSteps
{
    [Test]
    public void content_creation()
    {
        scenario(created_by_a_human);
        scenario(created_by_the_system);
    }

    private void created_by_a_human()
    {
        When(new_content_is_created_by(charlotte_bronte));
        Then(create_exits(charlotte_bronte));
    }

    public void created_by_the_system()
    {
        When(new_content_is_created_by(system_user));
        Then(qc_exits);
        And(event_is_raised(Transitions.Change));
    }

    public Action create_exits(User user)
    {
        return () =>
        {
            Then(it_moves_to(ContentLifecycles.New));
            And(it_was_edited_by(user));
            And(the_last_human_editor_is(user));
            And(it_has_transitions(Transitions.Change | Transitions.MakeReadyForQc | Transitions.Retire));
            And(it_has_transitions_for(system_user, Transitions.Change));
            And(event_is_raised(Transitions.Change));
        };
    }
}