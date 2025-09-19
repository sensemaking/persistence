using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Sensemaking.Domain.Events;
using Newtonsoft.Json;

namespace Sensemaking.Domain
{

    public interface IAggregate
    {
        string Id { get; }
        Queue<DomainEvent> Events { get; }
    }

    public abstract class Aggregate<T> : IAggregate
    {
        protected Aggregate()
        {
            Id = default!;
            Events = new Queue<DomainEvent>();
        }

        protected Aggregate(T id)
        {
            Id = id;
            Events = new Queue<DomainEvent>();
        }

        string IAggregate.Id => Id!.ToString()!;

        [NotNull]
        public virtual T Id { get; private set; } = default!;

        [JsonIgnore]
        public Queue<DomainEvent> Events { get; }

        protected bool Equals(Aggregate<T> other)
        {
            return Id!.ToString() == other.Id!.ToString();
        }

        public override bool Equals(object? obj)
        {
            return obj is Aggregate<T> aggregate && Equals(aggregate);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }
    }
}