namespace Fdb.Rx.Domain.Events
{
    public interface IHandleDomainEvents
    {
        bool RecordMetrics { get; }
        void Handle(DomainEvent domainEvent);
        bool CanHandle(DomainEvent domainEvent);
        IRepositories Repositories { get; set; }
    }

    public interface IHandleDomainEvents<in T> : IHandleDomainEvents where T : DomainEvent
    {
        void Handle(T domainEvent);
    }

    public abstract class DomainEventHandler<T> : IHandleDomainEvents<T> where T : DomainEvent
    {
        public virtual bool RecordMetrics => false;
        public abstract void Handle(T domainEvent);

        public void Handle(DomainEvent domainEvent)
        {
            Handle((T)domainEvent);
        }

        public bool CanHandle(DomainEvent domainEvent)
        {
            return domainEvent is T;
        }

        protected IRepositories Repositories => (this as IHandleDomainEvents).Repositories;
        IRepositories IHandleDomainEvents.Repositories { get; set; } = null!;
    }
}
