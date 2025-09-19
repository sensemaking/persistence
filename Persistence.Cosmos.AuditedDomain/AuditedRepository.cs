using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sensemaking.Domain;
using Sensemaking.Domain.Events;
using Sensemaking.Messaging;
using AuditedDomainRepository = Sensemaking.AuditedDomain.Repository;

namespace Sensemaking.Persistence.Cosmos.AuditedDomain;

public class AuditedRepository : IContentRepository
{
    private readonly AuditedDomainRepository repository;

    internal AuditedRepository(IContentRepository repository, IPublishEvents auditPublisher)
    {
        this.repository = new AuditedDomainRepository(repository, auditPublisher);
    }

    public async Task<T> Get<T>(string id) where T : IAmContent => await repository.Get<T>(id);
    public async Task Save<T>(T aggregate, User user) where T : IAmContent => await repository.Save(aggregate, user);
    public async Task Delete<T>(T aggregate, User user) where T : IAmContent => await repository.Delete(aggregate, user);
    public async Task Delete<T>(string id, User user) where T : IAmContent => await repository.Delete<T>(id, user);
    public async Task Suspend<T>(T aggregate, User user) where T : IAmContent => await repository.Suspend(aggregate, user);
    public async Task Retire<T>(T aggregate, User user) where T : IAmContent => await repository.Retire(aggregate, user);
    public async Task<T> GetLive<T>(string id) where T : IAmContent => await repository.GetLive<T>(id);
    public async Task<IReadOnlyCollection<T>> GetAllLive<T>() where T : IAmContent => await repository.GetAllLive<T>();
    public void Register<T>(string collection, IValidateCollections<T>? collectionValidator) where T : IAmContent => repository.Register(collection, collectionValidator);
    public void RegisterForDomainEvents<T>(string collection, IValidateCollections<T>? collectionValidator) where T : IAggregate => repository.RegisterForDomainEvents(collection, collectionValidator);
    public async Task Qc<T>(T aggregate, User user) where T : IAmContent => await repository.Qc(aggregate, user);
    public async Task MakeReadyForQc<T>(T aggregate, User user) where T : IAmContent => await repository.MakeReadyForQc(aggregate, user);
    public async Task Reactivate<T>(T aggregate, User user) where T : IAmContent => await repository.Reactivate(aggregate, user);
}