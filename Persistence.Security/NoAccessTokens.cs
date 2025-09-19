namespace Fdb.Rx.Persistence.Security
{
    public class NoAccessTokens : IProvideAccessTokens
    {
        public string GetToken()
        {
            return string.Empty;
        }
    }
}