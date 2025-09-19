namespace Sensemaking.Persistence.Security
{
    public class NoAccessTokens : IProvideAccessTokens
    {
        public string GetToken()
        {
            return string.Empty;
        }
    }
}