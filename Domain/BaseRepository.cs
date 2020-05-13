using System;
using System.Collections.Generic;
using System.Linq;

namespace Sensemaking.Domain
{
    public interface IRepository
    {
        void Register<T>(string collection, IValidateCollections<T> collectionValidator) where T : IAggregate;
        T Get<T>(string id) where T : IAggregate;
        void Save<T>(T aggregate) where T : IAggregate;
        void Delete<T>(T aggregate) where T : IAggregate;
        void Delete<T>(string id) where T : IAggregate;
    }

    public interface IPublishableRepository
    {
        void Publish<T>(T aggregate) where T : IPublishableAggregate;
        void Unpublish<T>(T aggregate) where T : IPublishableAggregate;
        T[] GetAllPublished<T>() where T : IPublishableAggregate;
    }

    public abstract class BaseRepository : IRepository, IPublishableRepository
    {
        private readonly IDictionary<Type, object> TypeRegistration = new Dictionary<Type, object>();
        protected readonly IDispatchDomainEvents? dispatcher;

        protected BaseRepository(IDispatchDomainEvents? dispatcher = null)
        {
            this.dispatcher = dispatcher;
        }

        public void Register<T>(string collection, IValidateCollections<T> collectionValidator) where T : IAggregate
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

        public void Save<T>(T aggregate) where T : IAggregate
        {
            RegistrationFor<T>().CollectionValidator?.Validate(GetAll<T>().Where(x => x.Id != aggregate.Id).Union(new[] { aggregate }));
            SaveAggregate(aggregate);
            aggregate.Saved();
            dispatcher?.Dispatch(aggregate.Events);
            dispatcher?.Dispatch(new Queue<DomainEvent>(new[] { new Published<T>(aggregate) }));
        }

        public void Publish<T>(T aggregate) where T : IPublishableAggregate
        {
            Save(aggregate);
            PublishAggregate(aggregate);
            aggregate.Published();
            dispatcher?.Dispatch(new Queue<DomainEvent>(new[] { new Published<T>(aggregate) }));
        }

        public void Unpublish<T>(T aggregate) where T : IPublishableAggregate
        {
            UnpublishAggregate(aggregate);
            aggregate.Unpublished();
            dispatcher?.Dispatch(new Queue<DomainEvent>(new[] { new Unpublished<T>(aggregate) }));
        }

        public void Delete<T>(T aggregate) where T : IAggregate
        {
            DeleteAggregate(aggregate);
            dispatcher?.Dispatch(new Queue<DomainEvent>(new[] { new Deleted<T>(aggregate) }));
        }

        public void Delete<T>(string id) where T : IAggregate
        {
            var aggregate = Get<T>(id);
            if (aggregate != null)
                Delete(aggregate);
        }

        public abstract T Get<T>(string id) where T : IAggregate;
        protected abstract T[] GetAll<T>() where T : IAggregate;
        protected abstract void SaveAggregate<T>(T aggregate) where T : IAggregate;
        protected abstract void DeleteAggregate<T>(T aggregate) where T : IAggregate;

        public abstract T[] GetAllPublished<T>() where T : IPublishableAggregate;
        protected abstract void PublishAggregate<T>(T aggregate) where T : IPublishableAggregate;
        protected abstract void UnpublishAggregate<T>(T aggregate) where T : IPublishableAggregate;
    }
}