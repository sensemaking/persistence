using System;
using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using ConfigurationBuilder = Microsoft.Extensions.Configuration.ConfigurationBuilder;

namespace Fdb.Rx.Testing.Persistence.Dapper;

[SetUpFixture]
public class Startup
{
    private const string database_name = "FdbRxCommonTest";
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
    }

    [OneTimeSetUp]
    public void Start()
    {
        CreateDB(database_name);
    }

    private static void CreateDB(string database_name)
    {
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
    }
}