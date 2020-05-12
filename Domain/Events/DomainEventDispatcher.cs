using System;
using System.Collections.Generic;
using System.Linq;

namespace Sensemaking.Domain
{
    public interface IDispatchDomainEvents
    {
        void Dispatch(Queue<DomainEvent> events);
    }
    
    public class DomainEventDispatcher : IDispatchDomainEvents
    {
        private readonly IEnumerable<IHandleDomainEvents> _handlers;

        public DomainEventDispatcher(IEnumerable<IHandleDomainEvents> handlers)
        {
            _handlers = handlers;
        }

        public void Dispatch(Queue<DomainEvent> events)
        {
            while((events ?? new Queue<DomainEvent>()).Count > 0)
                Dispatch(events.Dequeue());
            
        }

        private void Dispatch(DomainEvent evnt)
        {
            _handlers.Where(handler => handler.CanHandle(evnt)).ForEach(handler => handler.Handle(evnt));
        }
    }
}
