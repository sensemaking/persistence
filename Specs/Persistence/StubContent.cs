using System;
using Fdb.Rx.Domain;

namespace Fdb.Rx.Testing.Persistence;

public class StubContent : Content<string>
{
    public StubContent(string id, string text) : base(id, new User(Guid.NewGuid(), "Bob"))
    {
        Text = text;
    }

    public string Text { get; set; }
}