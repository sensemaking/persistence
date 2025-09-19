using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Fdb.Rx.Domain;
using Sensemaking.Monitoring;
using System.Serialization;

namespace Fdb.Rx.Persistence.Dapper
{
    internal class DapperPersistence : IPersist
    {
        public IMonitor Monitor { get; }
        protected readonly AggregateRegistration registration;

        protected IDb db;

        private const string PublicationTableSuffix = "AsPublished";
        
        public DapperPersistence(IDb db)
        {
            this.db = db;
            Monitor = this.db.Monitor;
            registration = new AggregateRegistration();
        }

        public Task<T> Get<T>(string id) where T : IAggregate
        {
            return Get<T>(registration.Get<T>().Collection(), id);
        }

        public async Task<T[]> Get<T>(params string[] ids) where T : IAggregate
        {
            return (await db.QueryAsync<string>($"SELECT [Document] FROM {registration.Get<T>().Collection()} WHERE Id IN @ids", new { ids })).Select(r => r.Deserialize<T>()).ToArray();
        }

        public async Task<T[]> GetAll<T>() where T : IAggregate
        {
            return (await db.QueryAsync<string>($"SELECT [Document] FROM {registration.Get<T>().Collection()}")).Select(r => r.Deserialize<T>()).ToArray();
        }

        public Task Persist<T>(T aggregate) where T : IAggregate
        {
            return Save(registration.Get<T>().Collection(), aggregate);
        }

        public async Task Remove<T>(T aggregate) where T : IAggregate
        {
            await Delete(registration.Get<T>().Collection(), aggregate);
            if (typeof(IAmContent).IsAssignableFrom(typeof(T)))
                await Delete(registration.Get<T>().Collection(PublicationTableSuffix), aggregate);
        }

        public AggregateRegistration GetTypeRegistration()
        {
            return registration;
        }

        public Task<T> GetLive<T>(string id) where T : IAmContent
        {
            return Get<T>(registration.Get<T>().Collection(PublicationTableSuffix), id);
        }

        public async Task<IReadOnlyCollection<T>> GetAllLive<T>() where T : IAmContent
        {
            var documents = await db.QueryAsync<string>($"SELECT [Document] FROM {registration.Get<T>().Collection(PublicationTableSuffix)}").ConfigureAwait(false);
            return documents.Select(x => x.Deserialize<T>()).ToArray();
        }

        public Task PersistAsLive<T>(T aggregate) where T : IAmContent
        {
            return Save(registration.Get<T>().Collection(PublicationTableSuffix), aggregate);
        }

        public Task RemoveFromLive<T>(T aggregate) where T : IAmContent
        {
            return Delete(registration.Get<T>().Collection(PublicationTableSuffix), aggregate);
        }

        private async Task<T> Get<T>(string table, string id)
        {
            var result = await db.QueryAsync<string>($"SELECT [Document] FROM {table} WHERE Id = @id", new { id });
            return result.Any() ? result.Single().Deserialize<T>() : default!;
        }

        private Task Save<T>(string table, T aggregate) where T : IAggregate
        {
            var serialize = aggregate.Serialize();

            return db.ExecuteAsync($@"MERGE INTO {table} AS target  
                USING(SELECT @id, @document) AS source(Id, Document) ON(target.Id = source.Id)
                WHEN MATCHED THEN UPDATE SET Document = source.Document
                WHEN NOT MATCHED THEN INSERT(Id, Document) VALUES(source.Id, source.Document);",
                new { id = aggregate.Id, document = serialize }, CommandType.Text);
        }

        private Task Delete<T>(string table, T aggregate) where T : IAggregate
        {
            return db.ExecuteAsync($"DELETE FROM {table} WHERE Id = @id", new { id = aggregate.Id }, CommandType.Text);
        }
    }
}
