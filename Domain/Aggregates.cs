using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Sensemaking.Domain
{
    public interface IAggregate
    {
        string Id { get; }
        void Saved();
        Queue<DomainEvent> Events { get; }

    }

    public abstract class Aggregate<T> : IAggregate
    {
        protected Aggregate(T id)
        {
            if(id == null)
                throw new ArgumentException("Aggregate id's cannot be null");

            Id = id;
            Events = new Queue<DomainEvent>();
        }

        string IAggregate.Id => Id.ToString();
        [NotNull]
        public T Id { get; }

        void IAggregate.Saved() { }

        [JsonIgnore]
        public Queue<DomainEvent> Events { get; }

        protected bool Equals(Aggregate<T> other)
        {
            return Id.ToString() == other.Id.ToString();
        }

        public override bool Equals(object obj)
        {
            return obj is Aggregate<T> aggregate && Equals(aggregate);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }
    }

    public interface IPublishableAggregate : IAggregate
    {
        void Published();
        void Unpublished();
    }

    public enum PublicationStatus
    {
        NotYetPublished = 0,
        Published = 1,
        Suspended = 2
    }

    public abstract class PublishableAggregate<T> : Aggregate<T>, IPublishableAggregate
    {
        public PublicationStatus PublicationStatus { get; private set; }
        public bool HasUnpublishedChanges { get; private set; }

        protected PublishableAggregate(T id) : base(id)
        {
            PublicationStatus = PublicationStatus.NotYetPublished;
        }

        void IAggregate.Saved()
        {
            HasUnpublishedChanges = true;
        }

        void IPublishableAggregate.Published()
        {
            PublicationStatus = PublicationStatus.Published;
            HasUnpublishedChanges = false;
        }

        void IPublishableAggregate.Unpublished()
        {
            PublicationStatus = PublicationStatus.Suspended;
            HasUnpublishedChanges = false;
        }
    }
}