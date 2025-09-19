using System;
using Dapper;
using Fdb.Rx.Test.Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using ConfigurationBuilder = Microsoft.Extensions.Configuration.ConfigurationBuilder;

namespace Sensemaking.Specs.PersistenceTest.Dapper;

[SetUpFixture]
public class Startup
{
    private const string database_name = "FdbRxCommonTest";
    private const string database_name2 = "FdbRxCommonTest2";
    internal static readonly string db_server = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true).Build()["Db.Server"];

    internal class Server
    {
        internal static readonly string connection_string = $"Server={db_server};Integrated Security=true;Encrypt=False";
        internal static readonly bool isWindows = true;

        static Server()
        {
            var serverConnectionString = Environment.GetEnvironmentVariable("ServerConnectionString");
            if (!serverConnectionString.IsNullOrEmpty())
            {
                connection_string = $"{serverConnectionString}";
                isWindows = false;
            }
        }
    }

    internal class Database
    {
        internal static readonly string connection_string = $"{Server.connection_string};Database={database_name}";
        internal static string static_data_table_name = "dbo.TruncateDbSpecificationStaticDataTable";
    }

    internal class AnotherDatabase
    {
        internal static readonly string connection_string = $"{Server.connection_string};Database={database_name2}";
    }

    [OneTimeSetUp]
    public void Start()
    {
        CreateDB(database_name);
        CreateDB(database_name2);
        TruncateDbSpecification.RegisterStaticDataTables(Database.static_data_table_name);
    }

    private static void CreateDB(string database_name)
    {
        var modFileName = Server.isWindows ? $@"c:\{database_name}_mod" : $"/tmp/{database_name}_mod";
        using var con = new SqlConnection($"{Server.connection_string};Database=master");
        con.Execute($"""
                     IF EXISTS(SELECT * FROM sys.databases WHERE name = '{database_name}')
                     BEGIN
                        DROP DATABASE {database_name};
                     END
                     CREATE DATABASE {database_name}
                     """);
    }

    [OneTimeTearDown]
    public void Stop()
    {
        using var con = new SqlConnection($"{Server.connection_string};Database=master");
        con.Execute($"ALTER DATABASE {database_name} SET single_user WITH ROLLBACK IMMEDIATE; DROP DATABASE {database_name}");
        con.Execute($"ALTER DATABASE {database_name2} SET single_user WITH ROLLBACK IMMEDIATE; DROP DATABASE {database_name2}");
    }
}