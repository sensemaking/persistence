using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fdb.Rx.Domain;
using Fdb.Rx.Domain.Events;
using NSubstitute;
using NSubstitute.ClearExtensions;

namespace Fdb.Rx.Test.Domain
{
    public class SubstituteRepository : IRepository
    {
        private readonly Repository repository;
        private readonly IPersist persistence;
        private readonly IRepository SubstituteForReceivedCalls;

        internal SubstituteRepository(IPersist persistence, DomainEventDispatcher dispatcher)
        {
            repository = new Repository(persistence, dispatcher);
            SubstituteForReceivedCalls = Substitute.For<IRepository>();
            this.persistence = persistence;
        }

        public async Task<T> Get<T>(string id) where T : IAggregate
        {
            await SubstituteForReceivedCalls.Get<T>(id);
            return await repository.Get<T>(id);
        }

        public async Task<T[]> Get<T>(params string[] ids) where T : IAggregate
        {
            await SubstituteForReceivedCalls.Get<T>(ids);
            return await repository.Get<T>(ids);
        }

        public async Task<T[]> GetAll<T>() where T : IAggregate
        {
            await SubstituteForReceivedCalls.GetAll<T>();
            return await repository.GetAll<T>();
        }

        public async Task Save<T>(T aggregate) where T : IAggregate
        {
            try { persistence.GetTypeRegistration().Get<T>(); }
            catch (Exception) { persistence.GetTypeRegistration().Register<T>("must be registered to lookup collection validation", null); }
            await SubstituteForReceivedCalls.Save(aggregate);
            await repository.Save(aggregate);
        }

        public async Task Delete<T>(T aggregate) where T : IAggregate
        {
            await SubstituteForReceivedCalls.Delete(aggregate);
            await repository.Delete(aggregate);
        }

        public async Task Delete<T>(string id) where T : IAggregate
        {
            await SubstituteForReceivedCalls.Delete<T>(id);
            var aggregate = await Get<T>(id);
            if (aggregate == null)
                throw new Exception("You need to stub GetAsync to return the aggregate you want to delete in order to delete it.");

            await repository.Delete<T>(id);
        }

        public void ClearReceivedCalls()
        {
            SubstituteForReceivedCalls.ClearReceivedCalls();
        }

        public void ClearSubstitute()
        {
            SubstituteForReceivedCalls.ClearSubstitute();
            ((SubstitutePersistence) persistence).ClearSubstitute();
        }

        public void OnSaving<T>(string id, Action<T> action) where T : IAggregate
        {
            ((SubstitutePersistence) persistence).OnSaving(id, action);
        }

        public void OnDeleting<T>(string id, Action<T> action) where T : IAggregate
        {
            ((SubstitutePersistence) persistence).OnDeleting(id, action);
        }
    }
}