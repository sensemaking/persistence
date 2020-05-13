namespace Sensemaking.Query
{
    public interface IQuery<in T, out U>
    {
        U[] GetResults(T parameters);
    }
}