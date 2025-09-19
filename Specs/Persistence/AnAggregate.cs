using Sensemaking.Domain;

namespace Sensemaking.Specs.Persistence;

public class AnAggregate : Aggregate<string>
{
    public AnAggregate(string id, string content) : base(id)
    {
        Content = content;
    }

    public string Content { get; set; }
}