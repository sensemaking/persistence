namespace Sensemaking.Domain
{
    public class Deleted<T> : DomainEvent where T : IAggregate
    {
        public Deleted(T wasDeleted)
        {
            WasDeleted = wasDeleted;
        }

        public T WasDeleted { get; }
    }

    public class Published<T> : DomainEvent where T : IAggregate
    {
        public Published(T wasPublished)
        {
            WasPublished = wasPublished;
        }

        public T WasPublished { get; }
    }

    public class Unpublished<T> : DomainEvent where T : IAggregate
    {
        public Unpublished(T wasUnpublished)
        {
            WasUnpublished = wasUnpublished;
        }

        public T WasUnpublished { get; }
    }
}
