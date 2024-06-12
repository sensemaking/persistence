using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sensemaking.Query;

public interface IQuery<in T, U>
{
    Task<IEnumerable<U>> GetResultsAsync(T parameters);
}

public interface IRunQueries
{
    Task<IEnumerable<T>> GetResults<T>(string container, string query);
    Task<IEnumerable<T>> GetResults<T>((string Container, string Query) definition);
}