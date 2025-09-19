using Sensemaking.Persistence.Cosmos;

namespace Sensemaking.Domain
{
    public static class CosmosPersistenceProvider
    {
        public static IProvideRepositories Cosmos(this RepositoriesConfiguration repositories, IConnectToCosmos cosmosAccountConnection, string databaseName)
        {
            Database.Configure(cosmosAccountConnection, databaseName);
            repositories.Persistence = new CosmosPersistence();
            return repositories;
        }
    }
}