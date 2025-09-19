using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fdb.Rx.Domain;
using Sensemaking.Monitoring;

namespace Fdb.Rx.Persistence.Blob
{
    internal class BlobPersistence : IPersist
    {
        private readonly AggregateRegistration registration = new AggregateRegistration();
        public IMonitor Monitor { get; } = new Monitor(Account.Name);

        public Task<T> Get<T>(string id) where T : IAggregate
        {
            return Account.GetClient().Get<T>(registration.Get<T>().Collection(), id);
        }

        public Task<T[]> Get<T>(params string[] ids) where T : IAggregate
        {
           return Task.FromResult(ids.Aggregate(new List<T>(), (list, id) =>
           {
                list.Add(Account.GetClient().Get<T>(registration.Get<T>().Collection(), id).Result);
                return list;
           }).ToArray());
        }

        public Task Persist<T>(T aggregate) where T : IAggregate
        {
            return Account.GetClient().Save(registration.Get<T>().Collection(), aggregate);
        }

        public Task Remove<T>(T aggregate) where T : IAggregate
        {
            return Account.GetClient().Delete<T>(registration.Get<T>().Collection(), aggregate.Id);
        }

        public AggregateRegistration GetTypeRegistration()
        {
            return registration;
        }

        public Task<T[]> GetAll<T>() where T : IAggregate
        {
            throw new System.NotImplementedException("Repository does not support getting multiple blobs by id.");
        }

        public Task<T> GetLive<T>(string id) where T : IAmContent
        {
            throw new System.NotImplementedException("Repository does not support content");
        }

        public Task<IReadOnlyCollection<T>> GetAllLive<T>() where T : IAmContent
        {
            throw new System.NotImplementedException("Repository does not support content");
        }

        public Task PersistAsLive<T>(T aggregate) where T : IAmContent
        {
            throw new System.NotImplementedException("Repository does not support content");
        }

        public Task RemoveFromLive<T>(T aggregate) where T : IAmContent
        {
            throw new System.NotImplementedException("Repository does not support content");
        }
    }
}