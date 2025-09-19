using System.Data;
using Microsoft.Data.SqlClient;
using System.Transactions;
using Fdb.Rx.Persistence.Dapper;

namespace Fdb.Rx.Test.Dapper;

public class TestDbUser
{
    public TestDbUser(string adminConnectionString)
    {
            Connection =  new Db(adminConnectionString);
            ReaderConnection = GetDbReader(adminConnectionString);
        }

    public TestDbUser(string adminConnectionString, string user, string password, string role)
    {
            var connection = new SqlConnectionStringBuilder(adminConnectionString);
            if (connection.IntegratedSecurity == false && (string.IsNullOrEmpty(connection.UserID) || string.IsNullOrEmpty(connection.Password)))
                throw new DataException("IntegratedSecurity is false and UserID or Password is not set");

            var database = new Db(adminConnectionString);
            var scope = new TransactionScope(TransactionScopeOption.Suppress);
            database.Execute($@"IF NOT EXISTS(SELECT * FROM sys.server_principals WHERE name = '{user}')
                CREATE LOGIN {user} WITH PASSWORD = '{password}'", commandType: CommandType.Text);

            database.Execute($@"IF NOT EXISTS(SELECT * FROM sys.database_principals WHERE name = '{user}')
                CREATE USER {user} FOR LOGIN {user}", commandType: CommandType.Text);

            database.Execute($"sp_addrolemember", new { rolename = $"{role}", membername = $"{user}" });
            scope.Dispose();

            Connection = GetUserDb(adminConnectionString, user, password);
            ReaderConnection = GetUserDbReader(adminConnectionString, user, password);
        }
    public Db Connection { get; }
    public DbReader ReaderConnection { get; }

    private static DbReader GetDbReader(string connectionString) => new($"{connectionString};ApplicationIntent=READONLY");

    private static Db GetUserDb(string connectionString, string user, string password)
    {
            var connection = new SqlConnectionStringBuilder(connectionString);
            return new Db($"Server={connection.DataSource};Database={connection.InitialCatalog};User={user};Password={password};Encrypt=False;TrustServerCertificate={connection.TrustServerCertificate}");
        }

    private static DbReader GetUserDbReader(string connectionString, string user, string password)
    {
            var connection = new SqlConnectionStringBuilder(connectionString);
            return new DbReader($"Server={connection.DataSource};Database={connection.InitialCatalog};User={user};Password={password};ApplicationIntent=READONLY;Encrypt=False;TrustServerCertificate={connection.TrustServerCertificate}");
        }
}