using System;
using System.Collections.Generic;
using System.Linq;

namespace Fdb.Rx.Domain.Events
{
    public interface IDispatchDomainEvents
    {
        void Dispatch(Queue<DomainEvent> events);
        IRepositories Repositories { get; set; }
    }
    
    public class DomainEventDispatcher(Func<IEnumerable<IHandleDomainEvents>> handlerFactory) : IDispatchDomainEvents
    {
        private IEnumerable<IHandleDomainEvents>? handlers;

        public IRepositories Repositories { get; set; } = null!;

        public void Dispatch(Queue<DomainEvent> events)
        {
            while(events.Count > 0)
                Dispatch(events.Dequeue());
        }

        private void Dispatch(DomainEvent @event)
        {
            handlers ??= handlerFactory();
            handlers.Where(handler => handler.CanHandle(@event)).ForEach(handler =>
            {
                handler.Repositories = Repositories;
                if (handler.RecordMetrics)
                    Sensemaking.Logging.TimeThis(() => handler.Handle(@event), handler.GetType().Name);
                else
                    handler.Handle(@event);
            });
        }
    }
}
