using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sensemaking.Domain;
using Sensemaking.Domain.Events;
using NSubstitute;
using NSubstitute.ClearExtensions;

namespace Sensemaking.Test.Domain
{
    public class SubstituteContentRepository : IContentRepository
    {
        private readonly ContentRepository repository;
        private readonly IPersist persistence;
        public readonly IContentRepository SubstituteForReceivedCalls = Substitute.For<IContentRepository>();

        internal SubstituteContentRepository(IPersist persistence, DomainEventDispatcher dispatcher)
        {
            repository = new ContentRepository(persistence, dispatcher);
            this.persistence = persistence;
        }

        public async Task<T> Get<T>(string id) where T : IAmContent
        {
            await SubstituteForReceivedCalls.Get<T>(id);
            return (await repository.Get<T>(id));
        }

        public async Task<T[]> Get<T>(params string[] ids) where T : IAmContent
        {
            await SubstituteForReceivedCalls.Get<T>(ids);
            return (await repository.Get<T>(ids));
        }

        public async Task<T[]> GetAll<T>() where T : IAmContent
        {
            await SubstituteForReceivedCalls.GetAll<T>();
            return (await repository.GetAll<T>());
        }

        public async Task Save<T>(T aggregate, User user) where T : IAmContent
        {
            try { persistence.GetTypeRegistration().Get<T>(); }
            catch (Exception) { persistence.GetTypeRegistration().Register<T>("must be registered to lookup collection validation", null); }
            await SubstituteForReceivedCalls.Save(aggregate, user);
            await repository.Save(aggregate, user);
        }

        public async Task Delete<T>(T aggregate, User user) where T : IAmContent
        {
            await SubstituteForReceivedCalls.Delete(aggregate, user);
            await repository.Delete(aggregate, user);
        }

        public async Task Delete<T>(string id, User user) where T : IAmContent
        {
            await SubstituteForReceivedCalls.Delete<T>(id, user);
            var aggregate = await Get<T>(id);
            if (aggregate == null)
                throw new Exception("You need to stub GetAsync to return the aggregate you want to delete in order to delete it.");

            await repository.Delete<T>(id, user);
        }

        public async Task Suspend<T>(T aggregate, User user) where T : IAmContent
        {
            await SubstituteForReceivedCalls.Suspend(aggregate, user);
            await repository.Suspend(aggregate, user);
        }

        public async Task Retire<T>(T aggregate, User user) where T : IAmContent
        {
            await SubstituteForReceivedCalls.Retire(aggregate, user);
            await repository.Retire(aggregate, user);
        }

        public async Task<T> GetLive<T>(string id) where T : IAmContent
        {
            await SubstituteForReceivedCalls.GetLive<T>(id);
            return await repository.GetLive<T>(id);
        }

        public async Task<IReadOnlyCollection<T>> GetAllLive<T>() where T : IAmContent
        {
            await SubstituteForReceivedCalls.GetAllLive<T>();
            return await repository.GetAllLive<T>();
        }

        public async Task Qc<T>(T aggregate, User user) where T : IAmContent
        {
            SubstituteForReceivedCalls.Qc(aggregate, user).Await();
            await repository.Qc(aggregate, user);
        }

        public async Task MakeReadyForQc<T>(T aggregate, User user) where T : IAmContent
        {
            SubstituteForReceivedCalls.MakeReadyForQc(aggregate, user).Await();
            await repository.MakeReadyForQc(aggregate, user);
        }

        public void ClearReceivedCalls()
        {
            SubstituteForReceivedCalls.ClearReceivedCalls();
        }

        public void ClearSubstitute()
        {
            SubstituteForReceivedCalls.ClearSubstitute();
            ((SubstitutePersistence)persistence).ClearSubstitute();
        }

        public void OnSaving<T>(string id, Action<T> action) where T : IAggregate
        {
            ((SubstitutePersistence)persistence).OnSaving(id, action);
        }

        public void OnDeleting<T>(string id, Action<T> action) where T : IAggregate
        {
            ((SubstitutePersistence)persistence).OnDeleting(id, action);
        }

        public Task Reactivate<T>(T aggregate, User user) where T : IAmContent
        {
            SubstituteForReceivedCalls.Reactivate(aggregate, user).Await();
            return repository.Reactivate(aggregate, user);
        }
    }
}