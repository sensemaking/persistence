using Fdb.Rx.Persistence.Blob;

namespace Fdb.Rx.Domain
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
