using System;
using Sensemaking.Persistence.Dapper;
using Sensemaking.Bdd;

namespace Sensemaking.Specs.Persistence.Dapper;

public partial class DbReaderFactorySpecs
{
    private const string default_server = "MyServer";
    private const string default_server_place_holder = "$SERVER$";

    private static readonly string default_connection_string =
        $"Server={default_server_place_holder};Database=MyDatabase;Integrated Security=true";

    private string server_place_holder;
    private string db_server;
    private string connection_string;
    private IQueryDb the_read_only_db;

    protected override void before_each()
    {
        base.before_each();
        server_place_holder = default_server_place_holder;
        db_server = default_server;
        connection_string = default_connection_string;
        the_read_only_db = null;
    }

    private void a_connection_string()
    {
        connection_string = default_connection_string.Replace(default_server_place_holder, default_server);
    }

    private void a_db_server() { }

    private void a_connection_string_with_server_place_holder()
    {
    }

    private void no_connection_string()
    {
        connection_string = null;
    }

    private void empty_connection_string()
    {
        connection_string = string.Empty;
    }

    private void no_database_server()
    {
        db_server = null;
    }

    private void empty_database_server()
    {
        db_server = string.Empty;
    }

    private void no_server_placeholder()
    {
        server_place_holder = null;
    }

    private void empty_server_placeholder()
    {
        server_place_holder = string.Empty;
    }

    private void a_connection_without_the_server_placeholder()
    {
        connection_string = default_connection_string.Replace(server_place_holder, "aint got no palceholder fool");
    }

    private void creating_database_from_placeholder()
    {
        the_read_only_db = DbReaderFactory.Create(connection_string, db_server, server_place_holder);
    }

    private void creating_database()
    {
        the_read_only_db = DbFactory.Create(connection_string);
    }

    private void provides_a_database()
    {
        (the_read_only_db as DbReader).ConnectionString.should_be(connection_string.Replace(server_place_holder, default_server));
    }

    private void provides_a_database_from_server_placeholder()
    {
        (the_read_only_db as DbReader).ConnectionString.should_be(connection_string.Replace(server_place_holder, db_server));
    }
}