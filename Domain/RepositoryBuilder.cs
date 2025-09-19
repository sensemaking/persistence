using System;
using Sensemaking.Domain.Events;
using Sensemaking.Monitoring;

namespace Sensemaking.Domain
{
    public interface IRegisterAggregates
    {
        IProvideRepositories Register<T>(string collection, IValidateCollections<T>? collectionValidator) where T : IAggregate;
    }

    public interface IRegisterDomainEvents
    {
        IProvideRepositories Handling(Func<IHandleDomainEvents[]> handlerFactory);
    }

    public interface IProvideRepositories : IRegisterAggregates, IRegisterDomainEvents
    {
        public IRepositories Get();
    }

    public class RepositoryBuilder
    {
        public static RepositoriesConfiguration For { get; } = new RepositoriesConfiguration();
    }

    public class RepositoriesConfiguration : IProvideRepositories
    {
        private DomainEventDispatcher dispatcher = new DomainEventDispatcher(() => Array.Empty<IHandleDomainEvents>());
        internal IPersist Persistence { get; set; } = null!;
        internal Func<IPersist, DomainEventDispatcher, IRepositories> RepositoriesFactory { get; set; } = (persistence, dispatch) => new Repositories(new ContentRepository(persistence, dispatch), new Repository(persistence, dispatch), persistence.Monitor);

        public IProvideRepositories Register<T>(string collection, IValidateCollections<T>? validator) where T : IAggregate
        {
            Persistence.GetTypeRegistration().Register(collection, validator);
            return this;
        }

        public IProvideRepositories Handling(Func<IHandleDomainEvents[]> handlersFactory)
        {
            this.dispatcher = new DomainEventDispatcher(handlersFactory);
            return this;
        }

        public IRepositories Get()
        {
            var repositories = RepositoriesFactory(Persistence, dispatcher);
            dispatcher.Repositories = repositories;
            return repositories;
        }
    }

    public class Repositories : IRepositories
    {
        internal Repositories(IContentRepository content, IRepository aggregate, IMonitor monitor)
        {
            Content = content;
            Repository = aggregate;
            Monitor = monitor;
        }

        public IContentRepository Content { get; }
        public IRepository Repository { get; }
        public IMonitor Monitor { get; }

    }
}