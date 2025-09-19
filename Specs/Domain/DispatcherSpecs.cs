using Fdb.Rx.Test;
using NUnit.Framework;
using Sensemaking.Bdd;

namespace Fdb.Rx.Testing.Domain;

[TestFixture]
public partial class DispatcherSpecs : Specification
{
    [Test]
    public void dispatches_to_event_handler()
    {
        Given(an_event);
        And(a_repository);
        And(a_handler_for_the_event);
        When(dispatching);
        Then(the_handler_is_called);
        And(the_respository_is_available);
    }

    [Test]
    public void does_not_dispatch_to_handler_of_wrong_event()
    {
        Given(an_event);
        And(a_handler_for_a_different_event);
        When(dispatching);
        Then(the_handler_is_not_called);
    }
}