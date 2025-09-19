using System;
using Fdb.Rx.Domain;
using Fdb.Rx.Domain.Events;

namespace Sensemaking.Specs.Domain;

public static class Authors
{
    public static readonly User charlotte_bronte = new(Guid.NewGuid(), "Charlotte Bronte");
    public static readonly User jane_austin = new(Guid.NewGuid(), "Jane Austin");
    public static readonly User system_user = new(Guid.NewGuid(), "Big Brother", true);
    public static readonly User another_system_user = new(Guid.NewGuid(), "Big Brother's little brother", true);
    public static readonly User human_user = charlotte_bronte;
}

public class StubContent : Content<Guid>
{
    public StubContent(string text, User createdBy) : base(Guid.NewGuid(), createdBy)
    {
        Text = text;
    }

    public string Text { get; set; }

    public void RaiseEvent()
    {
        Events.Enqueue(new StubEvent());
    }

    public class StubEvent : DomainEvent { }
}
