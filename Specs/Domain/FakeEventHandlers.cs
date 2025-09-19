using System;
using Fdb.Rx.Domain;
using Fdb.Rx.Domain.Events;

namespace Fdb.Rx.Testing.Domain;

internal class FakeDomainEventHandler<T> : DomainEventHandler<T> where T : DomainEvent
{
    private readonly Action HandleEvent;

    public FakeDomainEventHandler(Action handleEvent)
    {
        HandleEvent = handleEvent;
    }

    public override void Handle(T domainEvent)
    {
        HandleEvent();
    }
}

internal class FakeChangedDomainEventHandler<T> : DomainEventHandler<Changed<T>> where T : IAggregate
{
    private readonly Action<T> HandleEvent;

    public FakeChangedDomainEventHandler(Action<T> handleEvent)
    {
        HandleEvent = handleEvent;
    }

    public override void Handle(Changed<T> domainEvent)
    {
        HandleEvent(domainEvent.WasChanged);
    }
}

internal class FakeMadeReadyForQcDomainEventHandler<T> : DomainEventHandler<MadeReadyForQc<T>> where T : IAggregate
{
    private readonly Action<T> HandleEvent;

    public FakeMadeReadyForQcDomainEventHandler(Action<T> handleEvent)
    {
        HandleEvent = handleEvent;
    }

    public override void Handle(MadeReadyForQc<T> domainEvent)
    {
        HandleEvent(domainEvent.WasChanged);
    }
}

internal class FakeQcdDomainEventHandler<T> : DomainEventHandler<Qcd<T>> where T : IAggregate
{
    private readonly Action<T, T> HandleEvent;

    public FakeQcdDomainEventHandler(Action<T, T> handleEvent)
    {
        HandleEvent = handleEvent;
    }

    public override void Handle(Qcd<T> domainEvent)
    {
        HandleEvent(domainEvent.WasQcd, domainEvent.PreviouslyLive);
    }
}

internal class FakeSuspendedDomainEventHandler<T> : DomainEventHandler<Suspended<T>> where T : IAggregate
{
    private readonly Action<T, T> HandleEvent;

    public FakeSuspendedDomainEventHandler(Action<T, T> handleEvent)
    {
        HandleEvent = handleEvent;
    }

    public override void Handle(Suspended<T> domainEvent)
    {
        HandleEvent(domainEvent.WasSuspended, domainEvent.PreviouslyLive);
    }
}

internal class FakeRetiredDomainEventHandler<T> : DomainEventHandler<Retired<T>> where T : IAggregate
{
    private readonly Action<T, T> HandleEvent;

    public FakeRetiredDomainEventHandler(Action<T, T> handleEvent)
    {
        HandleEvent = handleEvent;
    }

    public override void Handle(Retired<T> domainEvent)
    {
        HandleEvent(domainEvent.WasRetired, domainEvent.PreviouslyLive);
    }
}

internal class FakeReactivatedDomainEventHandler<T> : DomainEventHandler<Reactivated<T>> where T : IAggregate
{
    private readonly Action<T> HandleEvent;

    public FakeReactivatedDomainEventHandler(Action<T> handleEvent)
    {
        HandleEvent = handleEvent;
    }

    public override void Handle(Reactivated<T> domainEvent)
    {
        HandleEvent(domainEvent.WasReactivated);
    }
}

internal class FakeDeletedDomainEventHandler<T> : DomainEventHandler<Deleted<T>> where T : IAggregate
{
    private readonly Action HandleEvent;

    public FakeDeletedDomainEventHandler(Action handleEvent)
    {
        HandleEvent = handleEvent;
    }

    public override void Handle(Deleted<T> domainEvent)
    {
        HandleEvent();
    }
}

internal class MetricRecordingChangedDomainEventHandler<T> : DomainEventHandler<Changed<T>> where T : IAggregate
{
    public override bool RecordMetrics => true;
    private readonly Action<T> HandleEvent;

    public MetricRecordingChangedDomainEventHandler(Action<T> handleEvent)
    {
        HandleEvent = handleEvent;
    }

    public override void Handle(Changed<T> domainEvent)
    {
        HandleEvent(domainEvent.WasChanged);
    }
}
