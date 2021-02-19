using NUnit.Framework;

namespace Sensemaking.Dapper.Specs
{
    [SetUpFixture]
    public class Startup
    {
        private const string db_server = @"(localdb)\MSSQLLocalDB";

        internal class Database
        {
            internal static readonly string connection_string = $@"Server={db_server};Integrated Security=true;";
        }

        [OneTimeSetUp]
        public void Start()
        {
            // using (var con = new SqlConnection($@"Server={db_server};Database=master;Integrated Security=true;"))
            //     con.Execute($@"IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = '{database_name}') CREATE DATABASE {database_name}");
        }

        [OneTimeTearDown]
        public void Stop()
        {
            // using (var con = new SqlConnection($@"Server={db_server};Database=master;Integrated Security=true;"))
            //     con.Execute($"ALTER DATABASE {database_name} SET single_user WITH ROLLBACK IMMEDIATE; DROP DATABASE {database_name}");
        }
    }
}