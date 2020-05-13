namespace Sensemaking.Query
{
    public interface IGetQueries
    {
        IQuery<T, U> Get<T, U>() where T : class;
        IPagedQuery<T, U> GetPaged<T, U>() where T : IPagedQueryParameters;
    }

    public class QueryFactory : IGetQueries
    {
        public IQuery<T, U> Get<T, U>() where T : class
        {
            return TypeScanner.Get<IQuery<T, U>>();
        }

        public IPagedQuery<T, U> GetPaged<T, U>() where T : IPagedQueryParameters
        {
            return TypeScanner.Get<IPagedQuery<T, U>>();
        }
    }
}
