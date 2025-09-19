using Sensemaking.Persistence.Blob;

namespace Sensemaking.Domain
{
    public static class BlobPersistenceProvider
    {
        public static IProvideRepositories Blob(this RepositoriesConfiguration repositories)
        {
            repositories.Persistence = new BlobPersistence();
            return repositories;
        }
    }
}
