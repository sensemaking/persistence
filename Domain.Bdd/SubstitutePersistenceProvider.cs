using Fdb.Rx.Test.Domain;

namespace Fdb.Rx.Domain
{
    public static class CosmosPersistenceProvider
    {
        public static IProvideRepositories Substitutes(this RepositoriesConfiguration repositories)
        {
            repositories.Persistence = new SubstitutePersistence();
            repositories.RepositoriesFactory = (persistence, dispatcher) => new Repositories(
                new SubstituteContentRepository(persistence, dispatcher), 
                new SubstituteRepository(persistence, dispatcher), persistence.Monitor);
            return repositories;
        }
    }
}