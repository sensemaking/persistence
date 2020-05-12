namespace Sensemaking.Persistenc.Query
{
    public interface IPagedQuery<in T,U> where T : IPagedQueryParameters
    {
        PagedResult<U> GetResults(T parameters);
    }

    public interface IPagedQueryParameters
    {
        uint Page { get; }
        uint PageSize { get; }
    }

    public class PagedResult<T>
    {
        public PagedResult(T[] contents, uint pageCount)
        {
            Contents = contents;
            PageCount = pageCount;
        }

        public T[] Contents { get; }
        public uint PageCount { get; }
    }
}
