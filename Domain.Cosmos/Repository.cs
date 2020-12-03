using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Sensemaking.Cosmos;

namespace Sensemaking.Domain.Cosmos
{
    public class Repository : BaseRepository
    {
        private const string PublicationCollectionSuffix = "-as-published";

        public Repository(IDispatchDomainEvents? dispatcher = null) : base(dispatcher) { }

        public override async Task<T> GetAsync<T>(string id)
        {
            return await Database.GetClient().GetAsync<T>(Database.DatabaseName!, CollectionName<T>(), id)!.ConfigureAwait(false);
        }

        protected override async Task<IEnumerable<T>> GetAllAsync<T>()
        {
            return await Database.GetClient().GetAllASync<T>(Database.DatabaseName!, CollectionName<T>()).ConfigureAwait(false);
        }

        protected override async Task SaveAggregateAsync<T>(T aggregate) 
        {
            await Database.GetClient().SaveAsync(Database.DatabaseName!, CollectionName<T>(), aggregate).ConfigureAwait(false);
        }

        protected override async Task DeleteAggregateAsync<T>(T aggregate)
        {
            await Database.GetClient().DeleteAsync<T>(Database.DatabaseName!, CollectionName<T>(), aggregate.Id).ConfigureAwait(false);
        }

        public override async Task<IEnumerable<T>> GetAllPublishedAsync<T>()
        {
            return await Database.GetClient().GetAllASync<T>(Database.DatabaseName!, CollectionName<T>(PublicationCollectionSuffix)).ConfigureAwait(false);
        }

        protected override async Task PublishAggregateAsync<T>(T aggregate)
        {
            await Database.GetClient().SaveAsync(Database.DatabaseName!, CollectionName<T>(PublicationCollectionSuffix), aggregate).ConfigureAwait(false);
        }

        protected override async Task UnpublishAggregateAsync<T>(T aggregate)
        {
            await Database.GetClient().DeleteAsync<T>(Database.DatabaseName!, CollectionName<T>(PublicationCollectionSuffix), aggregate.Id).ConfigureAwait(false);
        }
    }
}