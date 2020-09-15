using System.Collections.Generic;
using System.Threading.Tasks;
using Sensemaking.Dapper;

namespace Sensemaking.Query.Dapper
{
    public abstract class Query<T, U> : IQuery<T, U>
    {
        protected IDb database;

        protected Query()
        {
            database = Query.Database;
        }

        public abstract Task<IEnumerable<U>> GetResultsAsync(T parameters);
    }
}
