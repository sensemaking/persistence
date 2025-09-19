namespace Fdb.Rx.Persistence.Cosmos
{
    public class QuerySpecification
    {
        public QuerySpecification(string container, string query)
        {
            Container = container;
            Query = query;
        }

        public string Container { get; }
        public string Query { get; }
    }
}