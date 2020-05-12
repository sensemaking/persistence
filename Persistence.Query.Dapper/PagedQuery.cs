using Sensemaking.Persistenc.Query;
using Sensemaking.Persistence.Dapper;

namespace Sensemaking.Persistence.Query.Dapper
{
    public abstract class PagedQuery<T, U> : IPagedQuery<T, U> where T : IPagedQueryParameters
    {
        protected IDb database;

        protected PagedQuery()
        {
            database = Query.Database;
        }

        public abstract PagedResult<U> GetResults(T parameters);
    }
}
