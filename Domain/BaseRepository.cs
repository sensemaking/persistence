using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sensemaking.Domain
{
    public interface IRepository
    {
        void Register<T>(string collection, IValidateCollections<T>? collectionValidator) where T : IAggregate;
        Task<T> GetAsync<T>(string id) where T : IAggregate;
        Task SaveAsync<T>(T aggregate) where T : IAggregate;
        Task DeleteAsync<T>(T aggregate) where T : IAggregate;
        Task DeleteAsync<T>(string id) where T : IAggregate;
    }

    public interface IPublishableRepository : IRepository
    {
        Task PublishAsync<T>(T aggregate) where T : IPublishableAggregate;
        Task UnpublishAsync<T>(T aggregate) where T : IPublishableAggregate;
        Task<IEnumerable<T>> GetAllPublishedAsync<T>() where T : IPublishableAggregate;
    }

    public abstract class BaseRepository : IPublishableRepository
    {
        private readonly IDictionary<Type, object> TypeRegistration = new Dictionary<Type, object>();
        protected readonly IDispatchDomainEvents? dispatcher;

        protected BaseRepository(IDispatchDomainEvents? dispatcher = null)
        {
            this.dispatcher = dispatcher;
        }

        public void Register<T>(string collection, IValidateCollections<T>? collectionValidator) where T : IAggregate
        {
            if (TypeRegistration.ContainsKey(typeof(T)))
                TypeRegistration.Remove(typeof(T));

            TypeRegistration.Add(typeof(T), new TypeRegistration<T>(collection, collectionValidator));
        }

        private TypeRegistration<T> RegistrationFor<T>() where T : IAggregate
        {
            if (!TypeRegistration.ContainsKey(typeof(T)))
                throw new Exception($"{typeof(T).FullName} has not been registered with document db.");

            return (TypeRegistration[typeof(T)] as TypeRegistration<T>)!;
        }

        protected string CollectionName<T>(string suffix = "") where T : IAggregate
        {
            return $"{RegistrationFor<T>().Collection}{suffix}";
        }

        public async Task SaveAsync<T>(T aggregate) where T : IAggregate
        {
            RegistrationFor<T>().CollectionValidator?.Validate((await GetAllAsync<T>().ConfigureAwait(false)).Where(x => x.Id != aggregate.Id).Union(new[] { aggregate }));
            aggregate.Saved();
            await SaveAggregateAsync(aggregate).ConfigureAwait(false);
            dispatcher?.Dispatch(aggregate.Events);
            dispatcher?.Dispatch(new Queue<DomainEvent>(new[] { new Saved<T>(aggregate) }));
        }

        public async Task PublishAsync<T>(T aggregate) where T : IPublishableAggregate
        {
            aggregate.Published();
            await SaveAsync(aggregate).ConfigureAwait(false);
            await PublishAggregateAsync(aggregate).ConfigureAwait(false);
            dispatcher?.Dispatch(new Queue<DomainEvent>(new[] { new Published<T>(aggregate) }));
        }

        public async Task UnpublishAsync<T>(T aggregate) where T : IPublishableAggregate
        {
            aggregate.Unpublished();
            await UnpublishAggregateAsync(aggregate).ConfigureAwait(false);
            dispatcher?.Dispatch(new Queue<DomainEvent>(new[] { new Unpublished<T>(aggregate) }));
        }

        public async Task DeleteAsync<T>(T aggregate) where T : IAggregate
        {
            await DeleteAggregateAsync(aggregate).ConfigureAwait(false);
            dispatcher?.Dispatch(new Queue<DomainEvent>(new[] { new Deleted<T>(aggregate) }));
        }

        public async Task DeleteAsync<T>(string id) where T : IAggregate
        {
            var aggregate = await GetAsync<T>(id).ConfigureAwait(false);
            if (aggregate != null)
                await DeleteAsync(aggregate).ConfigureAwait(false);
        }

        public abstract Task<T> GetAsync<T>(string id) where T : IAggregate;
        protected abstract Task<IEnumerable<T>> GetAllAsync<T>() where T : IAggregate;
        protected abstract Task SaveAggregateAsync<T>(T aggregate) where T : IAggregate;
        protected abstract Task DeleteAggregateAsync<T>(T aggregate) where T : IAggregate;

        public abstract Task<IEnumerable<T>> GetAllPublishedAsync<T>() where T : IPublishableAggregate;
        protected abstract Task PublishAggregateAsync<T>(T aggregate) where T : IPublishableAggregate;
        protected abstract Task UnpublishAggregateAsync<T>(T aggregate) where T : IPublishableAggregate;
    }
}