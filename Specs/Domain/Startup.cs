using Microsoft.Data.SqlClient;
using System;
using Dapper;
using NUnit.Framework;

namespace Fdb.Rx.Testing.Domain;

[SetUpFixture]
public class Startup
{
    private const string database_name = "FdbRxCommonTest";
    private static readonly string db_server = "localhost";

    internal class Server
    {
        internal static readonly string connection_string = $"Server={db_server};Integrated Security=true;Encrypt=False";

        static Server()
        {
            var serverConnectionString = Environment.GetEnvironmentVariable("ServerConnectionString");
            if (!serverConnectionString.IsNullOrEmpty())
            {
                connection_string = $"{serverConnectionString}";
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
        using var con = new SqlConnection($"{Server.connection_string};Database=master");
        con.Execute($"IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = '{database_name}') CREATE DATABASE {database_name}");
    }

    [OneTimeTearDown]
    public void Stop()
    {
        using var con = new SqlConnection($"{Server.connection_string};Database=master");
        con.Execute($"ALTER DATABASE {database_name} SET single_user WITH ROLLBACK IMMEDIATE; DROP DATABASE {database_name}");
    }
}