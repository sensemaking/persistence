using System.ComponentModel.DataAnnotations;
using Fdb.Rx.Test;
using NUnit.Framework;
using Sensemaking.Bdd;

namespace Fdb.Rx.Testing.Persistence.Dapper;

[TestFixture]
public partial class DbReaderFactorySpecs : Specification
{
    [Test]
    public void creates_a_database_from_a_connection_string()
    {
        Given(a_connection_string);
        When(creating_database);
        Then(provides_a_database);
    }

    [Test]
    public void creates_a_database_from_a_connection_string_and_a_single_server()
    {
        Given(a_db_server);
        And(a_connection_string_with_server_place_holder);
        When(creating_database_from_placeholder);
        Then(provides_a_database_from_server_placeholder);
    }

    [Test]
    public void informs_if_no_connection_string_is_provided()
    {
        scenario(() =>
        {
            Given(no_connection_string);
            When(() => trying(creating_database));
            Then(() => informs<ValidationException>("A connection string is required."));
        });

        scenario(() =>
        {
            Given(empty_connection_string);
            When(() => trying(creating_database));
            Then(() => informs<ValidationException>("A connection string is required."));
        });

        scenario(() =>
        {
            Given(empty_connection_string);
            When(() => trying(creating_database_from_placeholder));
            Then(() => informs<ValidationException>("A connection string is required."));
        });

        scenario(() =>
        {
            Given(no_connection_string);
            When(() => trying(creating_database_from_placeholder));
            Then(() => informs<ValidationException>("A connection string is required."));
        });
    }

    [Test]
    public void informs_if_no_servers_are_provided()
    {
        scenario(() =>
        {
            Given(no_database_server);
            And(a_connection_string_with_server_place_holder);
            When(() => trying(creating_database_from_placeholder));
            Then(() => informs<ValidationException>("A database server is required."));
        });

        scenario(() =>
        {
            Given(empty_database_server);
            And(a_connection_string_with_server_place_holder);
            When(() => trying(creating_database_from_placeholder));
            Then(() => informs<ValidationException>("A database server is required."));
        });
    }

    [Test]
    public void informs_if_there_is_no_server_placeholder()
    {
        scenario(() =>
        {
            Given(a_db_server);
            And(no_server_placeholder);
            When(() => trying(creating_database_from_placeholder));
            Then(() => informs<ValidationException>("A server placeholder is required."));
        });

        scenario(() =>
        {
            Given(a_db_server);
            And(empty_server_placeholder);
            When(() => trying(creating_database_from_placeholder));
            Then(() => informs<ValidationException>("A server placeholder is required."));
        });
    }

    [Test]
    public void informs_if_the_connection_string_does_not_contain_the_server_placeholder()
    {
        Given(a_db_server);
        And(a_connection_without_the_server_placeholder);
        When(() => trying(creating_database_from_placeholder));
        Then(() => informs<ValidationException>("The connection string does not include the server placeholder."));
    }
}