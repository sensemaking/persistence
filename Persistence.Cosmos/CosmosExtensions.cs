using Microsoft.Azure.Cosmos;

namespace Fdb.Rx.Persistence.Cosmos
{
    public static class CosmosExtensions
    {
        public static Database EnsureDatabase(this CosmosClient client, string databaseName)
        {
            var _ = client.CreateDatabaseIfNotExistsAsync(databaseName).Result;
            return client.GetDatabase(databaseName);
        }        
        
        public static DatabaseResponse EnsureDeleteDatabase(this CosmosClient client, string databaseName)
        {
            return client.GetDatabase(databaseName).DeleteAsync().Result;
        }

        public static Database EnsureContainer(this Database database, string containerName)
        {
            try { var __ = database.GetContainer(containerName).DeleteContainerAsync().Result; } catch { }

            var properties = new ContainerProperties(containerName, "/id");
            var _ = database.CreateContainerIfNotExistsAsync(properties).Result;
            return database;
        }
    }
}