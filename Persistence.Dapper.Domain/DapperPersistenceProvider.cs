using Fdb.Rx.Domain;
using Fdb.Rx.Persistence.Dapper;

namespace  Fdb.Rx.Domain
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