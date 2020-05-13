using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Serialization;
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

        [return: MaybeNull]
        public override T Get<T>(string id)
        {
            return Get<T>(CollectionName<T>(), id)!;
        }

        protected override T[] GetAll<T>()
        {
            return db.Query<string>($"SELECT [Document] FROM [{CollectionName<T>()}]").Select(x => x.Deserialize<T>()).ToArray();
        }

        public override T[] GetAllPublished<T>()
        {
            return db.Query<string>($"SELECT [Document] FROM [{CollectionName<T>()}{PublicationTableSuffix}]").Select(x => x.Deserialize<T>()).ToArray();
        }

        protected override void SaveAggregate<T>(T aggregate)
        {
            Save(CollectionName<T>(), aggregate);
        }

        protected override void DeleteAggregate<T>(T aggregate)
        {
            Delete(CollectionName<T>(), aggregate);
        }

        protected override void PublishAggregate<T>(T aggregate)
        {
            Save(CollectionName<T>(PublicationTableSuffix), aggregate);
        }

        protected override void UnpublishAggregate<T>(T aggregate)
        {
            Delete(CollectionName<T>(PublicationTableSuffix),aggregate);
        }

        [return: MaybeNull]
        private T Get<T>(string table, string id)
        {
            var result = db.Query<string>($"SELECT [Document] FROM [{table}] WHERE Id = @id", new { id });
            return result.Any() ? result.Single().Deserialize<T>() : default(T)!;
        }

        private void Save<T>(string table, T aggregate) where T : IAggregate
        {
            db.Execute($@"MERGE INTO [{table}] AS target  
                USING(SELECT @id, @document) AS source(Id, Document) ON(target.Id = source.Id)
                WHEN MATCHED THEN UPDATE SET Document = source.Document
                WHEN NOT MATCHED THEN INSERT(Id, Document) VALUES(source.Id, source.Document);",
            new { id = aggregate.Id, document = aggregate.Serialize() }, CommandType.Text);
        }

        private void Delete<T>(string table, T aggregate) where T : IAggregate
        {
            db.Execute($"DELETE FROM [{ table }] WHERE Id = @id", new { id = aggregate.Id }, CommandType.Text);
        }
    }
}
