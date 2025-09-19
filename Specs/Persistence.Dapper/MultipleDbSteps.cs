using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using Dapper;
using Fdb.Rx.Persistence.Dapper;
using Sensemaking.Specs.Persistence.Dapper.Monitoring;
using NSubstitute;
using NSubstitute.ClearExtensions;
using Sensemaking.Bdd;

namespace Sensemaking.Specs.Persistence.Dapper;

public partial class MultipleDbSpecs
{
    private const string command_text = "INSERT INTO SomeTable";
    private static readonly string[] results = { "Result", "Initial Result" };
    private static readonly string[] results_2 = { "Result", "Additional Result" };
    private static readonly string[] merged_results = { "Result", "Initial Result", "Additional Result" };
    private static readonly object constructed_result = new { Result = "Constructed" };
    private MultipleDb.QueryOptions query_option;
    private IDb available_database;
    private IDb available_database_2;
    private IDb unavailable_database;
    private IList<IDb> all_databases;
    private IEnumerable<string> query_result;
    private object constructed_query_result;

    protected override void before_each()
    {
        base.before_each();
        available_database = get_substitute_db();
        available_database_2 = get_substitute_db();
        unavailable_database = get_substitute_db();
        all_databases = new List<IDb>();
        query_result = null;
        constructed_query_result = null;
    }

    private void query_coalesce_option()
    {
        query_option = MultipleDb.QueryOptions.Coalesce;
    }

    private void query_one_option()
    {
        query_option = MultipleDb.QueryOptions.One;
    }

    private void query_merge_option()
    {
        query_option = MultipleDb.QueryOptions.Merge;
    }

    private void an_available_database()
    {
        all_databases.Add(available_database);
    }

    private void another_available_database()
    {
        all_databases.Add(available_database_2);
    }

    private void it_has_an_unavailable_database()
    {
        unavailable_database.ClearSubstitute();
        unavailable_database
            .Query<string>(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CommandType>(),
                Arg.Any<Func<SqlException, Exception>>()).Returns(x => throw new Exception("Why query me Willis"));
        unavailable_database
            .Query(Arg.Any<string>(), Arg.Any<Func<SqlMapper.GridReader, object>>(), Arg.Any<object>(),
                Arg.Any<CommandType>(), Arg.Any<Func<SqlException, Exception>>())
            .Returns(x => throw new Exception("Why query me Willis"));
        unavailable_database.Monitor.Returns(new FakeInstanceMonitor("Somtething went wrongtastic."));
        all_databases.Add(unavailable_database);
    }

    private void the_first_has_results()
    {
        available_database.Query<string>(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CommandType>(),
            Arg.Any<Func<SqlException, Exception>>()).Returns(results);
        available_database
            .Query(Arg.Any<string>(), Arg.Any<Func<SqlMapper.GridReader, object>>(), Arg.Any<object>(),
                Arg.Any<CommandType>(), Arg.Any<Func<SqlException, Exception>>()).Returns(constructed_result);
        available_database_2
            .Query<string>(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CommandType>(),
                Arg.Any<Func<SqlException, Exception>>()).Returns(Enumerable.Empty<string>());
        available_database_2.Query(Arg.Any<string>(), Arg.Any<Func<SqlMapper.GridReader, object>>(), Arg.Any<object>(),
            Arg.Any<CommandType>(), Arg.Any<Func<SqlException, Exception>>()).Returns(null);
    }

    private void the_second_has_results()
    {
        available_database
            .Query<string>(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CommandType>(),
                Arg.Any<Func<SqlException, Exception>>()).Returns(Enumerable.Empty<string>());
        available_database.Query(Arg.Any<string>(), Arg.Any<Func<SqlMapper.GridReader, object>>(), Arg.Any<object>(),
            Arg.Any<CommandType>(), Arg.Any<Func<SqlException, Exception>>()).Returns(null);
        available_database_2.Query<string>(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CommandType>(),
            Arg.Any<Func<SqlException, Exception>>()).Returns(results);
        available_database_2
            .Query(Arg.Any<string>(), Arg.Any<Func<SqlMapper.GridReader, object>>(), Arg.Any<object>(),
                Arg.Any<CommandType>(), Arg.Any<Func<SqlException, Exception>>()).Returns(constructed_result);
    }

    private void the_second_has_different_results()
    {
        available_database_2.Query<string>(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CommandType>(),
            Arg.Any<Func<SqlException, Exception>>()).Returns(results_2);
        available_database_2
            .Query(Arg.Any<string>(), Arg.Any<Func<SqlMapper.GridReader, object>>(), Arg.Any<object>(),
                Arg.Any<CommandType>(), Arg.Any<Func<SqlException, Exception>>()).Returns(constructed_result);
    }

    private void neither_has_results()
    {
        available_database
            .Query<string>(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CommandType>(),
                Arg.Any<Func<SqlException, Exception>>()).Returns(Enumerable.Empty<string>());
        available_database.Query(Arg.Any<string>(), Arg.Any<Func<SqlMapper.GridReader, object>>(), Arg.Any<object>(),
            Arg.Any<CommandType>(), Arg.Any<Func<SqlException, Exception>>()).Returns(null);
        available_database_2
            .Query<string>(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CommandType>(),
                Arg.Any<Func<SqlException, Exception>>()).Returns(Enumerable.Empty<string>());
        available_database_2.Query(Arg.Any<string>(), Arg.Any<Func<SqlMapper.GridReader, object>>(), Arg.Any<object>(),
            Arg.Any<CommandType>(), Arg.Any<Func<SqlException, Exception>>()).Returns(null);
    }

    private void executing_a_command()
    {
        new MultipleDb(all_databases).Execute(command_text);
    }

    private void querying()
    {
        query_result = new MultipleDb(all_databases, query_option).Query<string>("SELECT *");
    }

    private void querying(Action<Action> then)
    {
        then(querying);
    }

    private void querying_and_constructing_results()
    {
        constructed_query_result = new MultipleDb(all_databases, query_option).Query<object>("SELECT *", reader => new { Result = "I haven't been returned by the query" });
    }

    private void querying_and_constructing_results(Action<Action> then)
    {
        then(querying_and_constructing_results);
    }

    private void it_executes_against_all_available_databases()
    {
        all_databases.ForEach(x =>
            (x.Monitor.Availability() ? x.Received() : x.DidNotReceive()).Execute(command_text));
    }

    private void returns_the_results()
    {
        query_result.should_be(results);
    }

    private void returns_the_merged_results()
    {
        query_result.OrderBy(x => x).should_be(merged_results.OrderBy(x => x));
    }

    private void returns_empty_results()
    {
        query_result.should_be_empty();
    }

    private void returns_the_constructed_results()
    {
        constructed_query_result.should_be(constructed_result);
    }

    private void returns_empty_constructed_results()
    {
        constructed_query_result.should_be_null();
    }

    private void single_available_databases_is_queried_with_which_one_chosen_randomly(Action when)
    {
        var first_available_chosen = false;

        20.Times(() =>
        {
            available_database.ClearReceivedCalls();
            available_database_2.ClearReceivedCalls();
            when();
            if (available_database.ReceivedCalls().Any(c => c.GetMethodInfo().Name == "Query"))
            {
                available_database_2.ReceivedCalls().Any(c => c.GetMethodInfo().Name == "Query").should_be_false();
                first_available_chosen = true;
            }

            if (available_database_2.ReceivedCalls().Any(c => c.GetMethodInfo().Name == "Query") &&
                first_available_chosen)
            {
                available_database.ReceivedCalls().Any(c => c.GetMethodInfo().Name == "Query").should_be_false();
                "it".should_pass();
            }
        });

        "it".should_fail();
    }

    private IDb get_substitute_db()
    {
        var substitute = Substitute.For<IDb>();
        substitute.Monitor.Returns(new FakeInstanceMonitor());
        return substitute;
    }
}