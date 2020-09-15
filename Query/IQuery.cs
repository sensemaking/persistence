using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Sensemaking.Query
{
    public interface IQuery<in T, U>
    {
        Task<ImmutableArray<U>> GetResultsAsync(T parameters);
    }
}