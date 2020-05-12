namespace Sensemaking.Domain
{
    public interface IHandleDomainEvents
    {
        void Handle(DomainEvent domainEvent);
        bool CanHandle(DomainEvent domainEvent);
    }

    public interface IHandleDomainEvents<in T> : IHandleDomainEvents where T : DomainEvent
    {
        void Handle(T domainEvent);
    }

    public abstract class DomainEventHandler<T> : IHandleDomainEvents<T> where T : DomainEvent
    {
        public abstract void Handle(T domainEvent);

        public void Handle(DomainEvent domainEvent)
        {
            Handle((T)domainEvent);
        }

        public bool CanHandle(DomainEvent domainEvent)
        {
            return domainEvent is T;
        }
    }
}
