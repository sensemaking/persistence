using System;
using System.Collections.Generic;
using Fdb.Rx.Domain;
using Fdb.Rx.Domain.Events;
using NSubstitute;
using Sensemaking.Bdd;

namespace Fdb.Rx.Testing.Domain;

public partial class DispatcherSpecs
{
    private TestEvent the_event;
    private ITestableHandler the_handler;
    private static readonly IRepositories the_repositories = Substitute.For<IRepositories>();

    private void an_event()
    {
        the_event = new TestEvent();
    }
        
    private void a_repository() { }

    private void a_handler_for_the_event()
    {
        the_handler = new TestEventHandler();
    }

    private void a_handler_for_a_different_event()
    {
        the_handler = new WrongEventHandler();
    }

    private void dispatching()
    {
        var events = new Queue<DomainEvent>();
        events.Enqueue(the_event);
        var the_dispatcher = new DomainEventDispatcher(() => new IHandleDomainEvents[] { the_handler});
        the_dispatcher.Repositories = the_repositories;
        the_dispatcher.Dispatch(events);
    }

    private void the_handler_is_not_called()
    {
        the_handler.WasExecuted.should_be_false();
    }

    private void the_handler_is_called()
    {
        the_handler.WasExecuted.should_be_true();
    }

    private void the_respository_is_available()
    {
        the_handler.Repositories.should_be(the_repositories);
    }

}

internal interface ITestableHandler : IHandleDomainEvents
{
    bool WasExecuted { get; }
}

internal class TestEvent : DomainEvent
{
}

internal class WrongEvent : DomainEvent
{
}

internal class TestEventHandler : DomainEventHandler<TestEvent>, ITestableHandler
{
    public override void Handle(TestEvent domainEvent)
    {
        WasExecuted = true;
    }

    public bool WasExecuted { get; set; }
}

internal class WrongEventHandler : DomainEventHandler<WrongEvent>, ITestableHandler
{
    public override void Handle(WrongEvent domainEvent)
    {
        WasExecuted = true;
    }

    public bool WasExecuted { get; set; }
}