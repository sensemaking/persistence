using System;
using Fdb.Rx.Domain;
using Fdb.Rx.Domain.Events;

namespace Fdb.Rx.Testing.Domain;

public class StubAggregate : Aggregate<Guid>
{
    public StubAggregate(string content = "Lots of interesting content.") : base(Guid.NewGuid())
    {
        Content = content;
    }

    public string Content { get; private set; }
        
    public void RaiseEvent()
    {
        Events.Enqueue(new StubEvent());
    }

    public class StubEvent : DomainEvent
    {
                
    }
}

public class StubValidator<T> : IValidateCollections<T> where T : IAggregate
{
    public bool FailValidation;
    public const string ErrorMessage = "some error";

    public (bool validationFailed, ValidationException exceptionToThrow) Validate(T aggregate, string collection)
    {
        return (FailValidation, new ValidationException($"{ErrorMessage} in collection {collection}"));
    }
}