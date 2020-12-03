using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Serialization;
using System.Threading.Tasks;
using Sensemaking.Dapper;

namespace Sensemaking.Domain.Dapper
{
    public class DbRepository : BaseRepository
    {
        protected IDb db;

        private const string PublicationTableSuffix = "AsPublished";

        public DbRepository(IDb db, IDispatchDomainEvents? dispatcher = null) : base(dispatcher)
        {
            this.db = db;
        }

        public override async Task<T> GetAsync<T>(string id)
        {
            return await GetAsync<T>(CollectionName<T>(), id)!.ConfigureAwait(false);
        }

        protected override async Task<IEnumerable<T>> GetAllAsync<T>()
        {
            return (await db.QueryAsync<string>($"SELECT [Document] FROM [{CollectionName<T>()}]").ConfigureAwait(false)).Select(x => x.Deserialize<T>());
        }

        public override async Task<IEnumerable<T>> GetAllPublishedAsync<T>()
        {
            return (await db.QueryAsync<string>($"SELECT [Document] FROM [{CollectionName<T>()}{PublicationTableSuffix}]").ConfigureAwait(false)).Select(x => x.Deserialize<T>());
        }

        protected override async Task SaveAggregateAsync<T>(T aggregate)
        {
            await SaveAsync(CollectionName<T>(), aggregate).ConfigureAwait(false);
        }

        protected override async Task DeleteAggregateAsync<T>(T aggregate)
        {
            await DeleteAsync(CollectionName<T>(), aggregate).ConfigureAwait(false);
        }

        protected override async Task PublishAggregateAsync<T>(T aggregate)
        {
            await SaveAsync(CollectionName<T>(PublicationTableSuffix), aggregate).ConfigureAwait(false);
        }

        protected override async Task UnpublishAggregateAsync<T>(T aggregate)
        {
            await DeleteAsync(CollectionName<T>(PublicationTableSuffix), aggregate).ConfigureAwait(false);
        }

        [return: MaybeNull]
        private async Task<T> GetAsync<T>(string table, string id)
        {
            var result = await db.QueryAsync<string>($"SELECT [Document] FROM [{table}] WHERE Id = @id", new { id }).ConfigureAwait(false);
            return result.Any() ? result.Single().Deserialize<T>() : default!;
        }

        private async Task SaveAsync<T>(string table, T aggregate) where T : IAggregate
        {
            await db.ExecuteAsync($@"MERGE INTO [{table}] with (holdlock) AS target  
                USING(SELECT @id, @document) AS source(Id, Document) ON(target.Id = source.Id)
                WHEN MATCHED THEN UPDATE SET Document = source.Document
                WHEN NOT MATCHED THEN INSERT(Id, Document) VALUES(source.Id, source.Document);",
            new { id = aggregate.Id, document = aggregate.Serialize() }, CommandType.Text).ConfigureAwait(false);
        }

        private async Task DeleteAsync<T>(string table, T aggregate) where T : IAggregate
        {
            await db.ExecuteAsync($"DELETE FROM [{ table }] WHERE Id = @id", new { id = aggregate.Id }, CommandType.Text).ConfigureAwait(false);
        }
    }
}
