using Sensemaking.Dapper;

namespace Sensemaking.Query.Dapper
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
