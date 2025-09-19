using System;
using Fdb.Rx.Domain;
using NUnit.Framework;

namespace Sensemaking.Specs.Domain.Lifecycle;

public class ReactivatingContentSpecs : CommonLifecycleSteps
{
    [Test]
    public void content_reactivation()
    {
        scenario(retired_new_content_can_be_reactivated);
        scenario(retired_live_content_can_be_reactivated);
        scenario(retired_suspended_content_can_be_ractivated);

    }

    private void retired_new_content_can_be_reactivated()
    {
        Given(some_new_content);
        And(it_is_retired_by(human_user));
        When(it_is_reactivated_by(human_user));
        Then(reactivate_exits(ContentLifecycles.New));
    }

    private void retired_live_content_can_be_reactivated()
    {
        Given(some_live_content);
        And(it_is_retired_by(human_user));
        When(it_is_reactivated_by(human_user));
        Then(reactivate_exits(ContentLifecycles.Suspended));
    }

    private void retired_suspended_content_can_be_ractivated()
    {
        Given(some_suspended_content);
        And(it_is_retired_by(human_user));
        When(it_is_reactivated_by(human_user));
        Then(reactivate_exits(ContentLifecycles.Suspended));
    }

    private Action reactivate_exits(ContentLifecycles lifecycle)
    {
        return () =>
        {
            Then(it_moves_to(lifecycle));
            And(event_is_raised(Transitions.Reactivate));
        };
    }

    [Test]
    public void new_content_cannot_be_reactivated()
    {
        Given(some_new_content);
        When(it_is_reactivated_by(human_user));
        Then(() => informs<ValidationException>("Only retired content can be reactivated."));
    }

    [Test]
    public void live_content_cannot_be_reactivated()
    {
        Given(some_live_content);
        When(it_is_reactivated_by(human_user));
        Then(() => informs<ValidationException>("Only retired content can be reactivated."));
    }

    [Test]
    public void suspended_content_cannot_be_reactivated()
    {
        Given(some_suspended_content);
        When(it_is_reactivated_by(human_user));
        Then(() => informs<ValidationException>("Only retired content can be reactivated."));
    }

    [Test]
    public void system_user_cannot_reactivate_content()
    {
        Given(some_live_content);
        When(it_is_reactivated_by(system_user));
        Then(() => informs<ValidationException>("System users cannot reactivate content."));
    }
}
