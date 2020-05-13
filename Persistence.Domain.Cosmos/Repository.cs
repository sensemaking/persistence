using System.Diagnostics.CodeAnalysis;
using Sensemaking.Persistence.Cosmos;

namespace Sensemaking.Domain.Cosmos
{
    public class Repository : BaseRepository
    {
        private const string PublicationCollectionSuffix = "-as-published";

        public Repository(IDispatchDomainEvents? dispatcher = null) : base(dispatcher) { }

        [return: MaybeNull]
        public override T Get<T>(string id)
        {
            return Database.GetClient().Get<T>(Database.DatabaseName!, CollectionName<T>(), id);
        }

        protected override T[] GetAll<T>()
        {
            return Database.GetClient().GetAll<T>(Database.DatabaseName!, CollectionName<T>());
        }

        protected override void SaveAggregate<T>(T aggregate) 
        {
            Database.GetClient().Save(Database.DatabaseName!, CollectionName<T>(), aggregate);
        }

        protected override void DeleteAggregate<T>(T aggregate)
        {
            Database.GetClient().Delete<T>(Database.DatabaseName!, CollectionName<T>(), aggregate.Id);
        }

        public override T[] GetAllPublished<T>()
        {
            return Database.GetClient().GetAll<T>(Database.DatabaseName!, CollectionName<T>(PublicationCollectionSuffix));
        }

        protected override void PublishAggregate<T>(T aggregate)
        {
            Database.GetClient().Save(Database.DatabaseName!, CollectionName<T>(PublicationCollectionSuffix), aggregate);
        }

        protected override void UnpublishAggregate<T>(T aggregate)
        {
            Database.GetClient().Delete<T>(Database.DatabaseName!, CollectionName<T>(PublicationCollectionSuffix), aggregate.Id);
        }
    }
}