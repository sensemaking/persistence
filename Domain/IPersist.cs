using System.Collections.Generic;
using System.Threading.Tasks;
using Sensemaking.Monitoring;

namespace Fdb.Rx.Domain
{
    internal interface IPersist : IPersistContent
    {
        IMonitor Monitor { get; }
        Task<T> Get<T>(string id) where T : IAggregate;
        Task<T[]> Get<T>(params string[] ids) where T : IAggregate;
        Task<T[]> GetAll<T>() where T : IAggregate;
        Task Persist<T>(T aggregate) where T : IAggregate;
        Task Remove<T>(T aggregate) where T : IAggregate;
        AggregateRegistration GetTypeRegistration();
    }
    
    internal interface IPersistContent
    {
        Task<T> GetLive<T>(string id) where T : IAmContent;
        Task<IReadOnlyCollection<T>> GetAllLive<T>() where T : IAmContent;
        Task PersistAsLive<T>(T aggregate) where T : IAmContent;
        Task RemoveFromLive<T>(T aggregate) where T : IAmContent;
    }
}