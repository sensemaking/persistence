using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sensemaking.Domain;
using Sensemaking.Monitoring;

namespace Sensemaking.Persistence.Cosmos
{
    internal class CosmosPersistence : IPersist
    {
        protected readonly AggregateRegistration registration = new();
        private const string LiveCollectionSuffix = "-as-published";

        public IMonitor Monitor { get; } = new CosmosMonitor(Database.Connection, Database.DatabaseName);

        public Task<T> Get<T>(string id) where T : IAggregate
        {
            return Database.GetClient().Get<T>(Database.DatabaseName, registration.Get<T>().Collection(), id);
        }

        public async Task<T[]> Get<T>(params string[] ids) where T : IAggregate
        {
            return (await Database.GetClient().Query<T>(Database.DatabaseName, registration.Get<T>().Collection(), $"select * from c where array_contains([{string.Join(", ", ids.Select(id => $"'{id}'"))}], c.id)")).ToArray();
        }

        public async Task<T[]> GetAll<T>() where T : IAggregate
        {
            return (await Database.GetClient().Query<T>(Database.DatabaseName, registration.Get<T>().Collection(), $"select * from c ")).ToArray();
        }

        public Task Persist<T>(T aggregate) where T : IAggregate
        {
            return Database.GetClient().Save(Database.DatabaseName, registration.Get<T>().Collection(), aggregate);
        }

        public async Task Remove<T>(T aggregate) where T : IAggregate
        {
            var client = Database.GetClient();
            await client.Delete<T>(Database.DatabaseName, registration.Get<T>().Collection(), aggregate.Id);
            await client.Delete<T>(Database.DatabaseName, registration.Get<T>().Collection(LiveCollectionSuffix), aggregate.Id);
        }

        public AggregateRegistration GetTypeRegistration()
        {
            return registration;
        }

        public Task<T> GetLive<T>(string id) where T : IAmContent
        {
            return Database.GetClient().Get<T>(Database.DatabaseName, registration.Get<T>().Collection(LiveCollectionSuffix), id);
        }

        public Task<IReadOnlyCollection<T>> GetAllLive<T>() where T : IAmContent
        {
            return Database.GetClient().GetAll<T>(Database.DatabaseName, registration.Get<T>().Collection(LiveCollectionSuffix));
        }

        public Task PersistAsLive<T>(T aggregate) where T : IAmContent
        {
            return Database.GetClient().Save(Database.DatabaseName, registration.Get<T>().Collection(LiveCollectionSuffix), aggregate);
        }

        public Task RemoveFromLive<T>(T aggregate) where T : IAmContent
        {
            return Database.GetClient().Delete<T>(Database.DatabaseName, registration.Get<T>().Collection(LiveCollectionSuffix), aggregate.Id);
        }
    }
}