using System;
using System.Collections.Generic;
using System.Linq;
using Fdb.Rx.Persistence.Dapper;
using Sensemaking.Bdd;

namespace Fdb.Rx.Testing.Persistence.Dapper;

public partial class DbFactorySpecs
{
    private const string default_server = "MyServer";
    private const string default_server_place_holder = "$SERVER$";

    private static readonly string default_connection_string =
        $"Server={default_server_place_holder};Database=MyDatabase;Integrated Security=true";

    private string server_place_holder;
    private List<string> db_servers;
    private string connection_string;
    private IDb the_db;

    protected override void before_each()
    {
        base.before_each();
        server_place_holder = default_server_place_holder;
        db_servers = new List<string>();
        connection_string = default_connection_string;
        the_db = null;
    }

    private void a_connection_string()
    {
        connection_string = default_connection_string.Replace(default_server_place_holder, default_server);
    }

    private void a_db_server()
    {
        db_servers.Add("MyServer");
    }

    private void a_connection_string_with_server_place_holder()
    {
    }

    private void a_list_of_db_servers()
    {
        db_servers.Add("MyServer");
        db_servers.Add("MyOtherServer");
    }

    private void no_connection_string()
    {
        connection_string = null;
    }

    private void empty_connection_string()
    {
        connection_string = string.Empty;
    }

    private void no_database_servers()
    {
        db_servers = null;
    }

    private void empty_database_servers()
    {
        db_servers = new List<string>();
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
        the_db = DbFactory.Create(connection_string, db_servers, server_place_holder);
    }

    private void creating_database()
    {
        the_db = DbFactory.Create(connection_string);
    }

    private void provides_a_database()
    {
        (the_db as Db).ConnectionString.should_be(connection_string.Replace(server_place_holder, default_server));
    }

    private void provides_a_database_from_server_placeholder()
    {
        (the_db as Db).ConnectionString.should_be(connection_string.Replace(server_place_holder,
            db_servers.Single()));
    }

    private void provides_a_multi_database_for_each_server()
    {
        var multipleDb = (the_db as MultipleDb);

        multipleDb.Dbs.Length.should_be(db_servers.Count);
        db_servers.ForEach((s, i) =>
            (multipleDb.Dbs.ElementAt(i) as Db).ConnectionString.should_be(
                connection_string.Replace(server_place_holder, s)));
    }
}