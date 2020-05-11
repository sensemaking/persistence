using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using Sensemaking.Persistence.Dapper;

namespace Persistence.Dapper.Bdd
{
    public class TestDbUser
    {
        public TestDbUser(string connectionString)
        {
            Connection = GetAdminDb(connectionString);
        }

        public TestDbUser(string connectionString, string user, string password, string role)
        {
            var database = GetAdminDb(connectionString);
            var scope = new TransactionScope(TransactionScopeOption.Suppress);
            database.Execute($@"IF NOT EXISTS(SELECT * FROM sys.server_principals WHERE name = '{user}')
                CREATE LOGIN {user} WITH PASSWORD = '{password}'", commandType: CommandType.Text);

            database.Execute($@"IF NOT EXISTS(SELECT * FROM sys.database_principals WHERE name = '{user}')
                CREATE USER {user} FOR LOGIN {user}", commandType: CommandType.Text);

            database.Execute($"sp_addrolemember", new { rolename = $"{role}", membername = $"{user}" });
            scope.Dispose();

            Connection = GetUserDb(connectionString, user, password);
        }

        public Db Connection { get; }

        private static Db GetAdminDb(string connectionString)
        {
            var connection = new SqlConnectionStringBuilder(connectionString);
            return new Db($"Server={connection.DataSource};Database={connection.InitialCatalog};Integrated Security=true;");
        }

        private static Db GetUserDb(string connectionString, string user, string password)
        {
            var connection = new SqlConnectionStringBuilder(connectionString);
            return new Db($"Server={connection.DataSource};Database={connection.InitialCatalog};User={user};Password={password};");
        }
    }
}