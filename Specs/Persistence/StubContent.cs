using System;
using Fdb.Rx.Domain;

namespace Sensemaking.Specs.Persistence;

public class StubContent : Content<string>
{
    public StubContent(string id, string text) : base(id, new User(Guid.NewGuid(), "Bob"))
    {
        Text = text;
    }

    public string Text { get; set; }
}