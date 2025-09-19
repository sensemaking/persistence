using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using ConfigurationBuilder = Microsoft.Extensions.Configuration.ConfigurationBuilder;

namespace Fdb.Rx.Testing.Persistence.Cosmos;

public static class Settings
{
    internal static readonly IConfiguration Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true).Build(); 
        
    public static class CosmosDb
    {
        public static readonly string Endpoint = Configuration["Cosmos.Endpoint"];
        public static readonly string AccessKey = Configuration["Cosmos.AccessKey"];
        public static readonly string Database = Configuration["Cosmos.Db.Name"];
        public static CosmosClient GetCosmosClient() => new(Endpoint, AccessKey);
    }
}