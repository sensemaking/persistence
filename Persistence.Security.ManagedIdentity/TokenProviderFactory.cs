namespace Sensemaking.Persistence.Security
{
    public class TokenAccessFactory
    {
        public IProvideAccessTokens Create(bool supportsManagedIdentity)
        {
            return supportsManagedIdentity ? new ManagedIdentityTokens() : new NoAccessTokens() as IProvideAccessTokens;
        }
    }
}
