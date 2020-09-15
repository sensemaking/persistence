using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sensemaking.Query
{
    public interface IQuery<in T, U>
    {
        Task<IEnumerable<U>> GetResultsAsync(T parameters);
    }
}