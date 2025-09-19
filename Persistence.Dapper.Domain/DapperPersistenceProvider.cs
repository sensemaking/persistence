using Sensemaking.Domain;
using Sensemaking.Persistence.Dapper;

namespace Sensemaking.Domain
{
    public static class DapperPersistenceProvider
    {
        public static IProvideRepositories Dapper(this RepositoriesConfiguration repositories, IDb db)
        {
            repositories.Persistence = new DapperPersistence(db);
            return repositories;
        }
    }
}