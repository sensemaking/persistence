using Azure.Core;
using Azure.Identity;

namespace Fdb.Rx.Persistence.Security
{
    public class ManagedIdentityTokens : IProvideAccessTokens
    {
        internal ManagedIdentityTokens() { }

        public string GetToken() => new DefaultAzureCredential().GetTokenAsync(new TokenRequestContext(scopes: new[] { "https://database.windows.net/.default" })).Result.Token;
    }
}