using System;
using System.Collections.Generic;
using System.Serialization;
using System.Threading.Tasks;
using Sensemaking.Domain.Events;
using Newtonsoft.Json.Linq;

namespace Sensemaking.Domain
{
    public interface IRepository
    {
        Task<T> Get<T>(string id) where T : IAggregate;
        Task<T[]> Get<T>(params string[] ids) where T : IAggregate;
        Task<T[]> GetAll<T>() where T : IAggregate;
        Task Save<T>(T aggregate) where T : IAggregate;
        Task Delete<T>(T aggregate) where T : IAggregate;
        Task Delete<T>(string id) where T : IAggregate;
    }

    internal class Repository : IRepository
    {
        protected readonly IPersist persistence;
        protected readonly IDispatchDomainEvents dispatcher;
        private bool bypassEqualityCheck;

        internal Repository(IPersist persistence, DomainEventDispatcher dispatcher)
        {
            this.persistence = persistence;
            this.dispatcher = dispatcher;
            this.bypassEqualityCheck = false;
        }

        public Task<T> Get<T>(string id) where T : IAggregate
        {
            return persistence.Get<T>(id);
        }

        public Task<T[]> Get<T>(params string[] ids) where T : IAggregate
        {
            return persistence.Get<T>(ids);
        }

        public Task<T[]> GetAll<T>() where T : IAggregate
        {
            return persistence.GetAll<T>();
        }

        public async Task Save<T>(T aggregate) where T : IAggregate
        {
            if (!bypassEqualityCheck)
            {
                var existingAggregate = await persistence.Get<T>(aggregate.Id);
                if (existingAggregate != null!)
                    if (JToken.DeepEquals(JObject.Parse(existingAggregate.Serialize()), JObject.Parse(aggregate.Serialize())))
                        return;
            }

            var registeredAggregate = persistence.GetTypeRegistration().Get<T>();
            var validation = registeredAggregate.Validator?.Validate(aggregate, registeredAggregate.Collection());
            if (validation is { validationFailed: true }) throw validation.Value.exceptionToThrow;

            await persistence.Persist(aggregate).ConfigureAwait(false);
            aggregate.Events.Enqueue(new Changed<T>(aggregate));
            dispatcher.Dispatch(aggregate.Events);
        }

        public async Task Delete<T>(T aggregate) where T : IAggregate
        {
            await persistence.Remove(aggregate).ConfigureAwait(false);
            aggregate.Events.Enqueue(new Deleted<T>(aggregate));
            dispatcher.Dispatch(aggregate.Events);
        }

        public virtual async Task Delete<T>(string id) where T : IAggregate
        {
            var aggregate = await persistence.Get<T>(id).ConfigureAwait(false);
            if (aggregate != null)
                await Delete(aggregate).ConfigureAwait(false);
        }
    }
}
