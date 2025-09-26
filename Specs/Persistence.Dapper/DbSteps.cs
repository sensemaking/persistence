using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using Dapper;
using Sensemaking.Persistence.Dapper;
using Sensemaking.Bdd;

namespace Sensemaking.Specs.Persistence.Dapper;

public partial class DbSpecs
{
    private const string table_name = "[MyTable]";
    private const string temp_table_name = "MyTempTable";
    private const string proc_name = "MyProc";
    private static readonly Guid new_id = Guid.NewGuid();
    private static readonly Guid existing_id = Guid.NewGuid();

    private static InputIds[]
        copiedIds = Enumerable.Range(0, 5).Select(i => new InputIds(Guid.NewGuid())).ToArray();

    private const string exception_message = "My Exception";
    private Db db;
    private IEnumerable<Guid> query_result;
    private string constructed_query_result;
    private Exception execption;
    private SqlCommand command;
    private long duration;
    private readonly Action<Db> onQueryRetry = _db =>
    {
        _db.ExecuteAsync($@"INSERT INTO {temp_table_name} (Id) SELECT @new_id;", new { new_id }, CommandType.Text).Await();
    };

    protected override void before_all()
    {
        base.before_all();
        using (var con = new SqlConnection(Startup.Database.connection_string))
        {
            con.Execute($"CREATE TABLE {table_name} (Id UNIQUEIDENTIFIER NOT NULL); ");
            con.Execute($"CREATE TABLE {temp_table_name} (Id UNIQUEIDENTIFIER NOT NULL); ");
            con.Execute($"CREATE PROC {proc_name} @id UNIQUEIDENTIFIER AS BEGIN SELECT Id FROM {table_name} WHERE Id = @id END; ");
        }
    }

    protected override void after_all()
    {
        base.after_all();
        using (var con = new SqlConnection(Startup.Database.connection_string))
            con.Execute($"DROP TABLE {table_name};DROP PROC {proc_name}");
    }

    protected override void before_each()
    {
        base.before_each();
        query_result = null;
        constructed_query_result = null;
        execption = null;
        db = null;
        using (var con = new SqlConnection(Startup.Database.connection_string))
            con.Execute($"TRUNCATE TABLE {temp_table_name}");
        using (var con = new SqlConnection(Startup.Database.connection_string))
            con.Execute($"TRUNCATE TABLE {table_name}; INSERT INTO {table_name} SELECT @existing_id;", new { existing_id });
        duration = -1;
    }

    private void a_database()
    {
        db = new Db(Startup.Database.connection_string);
    }

    private void executing()
    {
        db.ExecuteAsync($"INSERT INTO {table_name} SELECT @new_id", new { new_id }, CommandType.Text).Await();
    }

    private void querying()
    {
        query_result = db.QueryAsync<Guid>($"SELECT Id FROM {table_name} WHERE Id = @existing_id", new { existing_id }).Await();
    }

    private void querying_async()
    {
        query_result = db.QueryAsync<Guid>($"SELECT Id FROM {table_name} WHERE Id = @existing_id", new { existing_id }).Await();
    }

    private void there_is_a_command()
    {
        var sql = $"SELECT Id FROM {table_name} WHERE Id = '{existing_id}'";
        var sqlConnection = new SqlConnection(Startup.Database.connection_string);
        sqlConnection.Open();
        command = new SqlCommand(sql, sqlConnection) { CommandTimeout = 0 };
        command.Prepare();
    }

    private void querying_constructed_result()
    {
        constructed_query_result = db.QueryAsync(proc_name, reader => reader.ReadSingle<Guid>().ToString(), new { id = existing_id }).Await();
    }

    private void querying_constructed_result_async()
    {
        constructed_query_result = db.QueryAsync(proc_name, reader => reader.ReadSingle<Guid>().ToString(), new { id = existing_id }).Await();
    }

    private void it_is_carried_out()
    {
        using (var con = new SqlConnection(Startup.Database.connection_string))
            con.Query<Guid>($"SELECT Id FROM {table_name} WHERE Id = @new_id", new { new_id }).should_not_be_empty();
    }

    private void results_are_returned()
    {
        query_result.should_not_be_empty();
    }

    private void constructed_result_is_returned()
    {
        constructed_query_result.should_be(existing_id.ToString());
    }

    private void can_be_converted_into_another_exception()
    {
        execption.Message.should_be(exception_message);
    }

    private void it_is_copied()
    {
        var expected = copiedIds.Append(new InputIds(existing_id)).Select(i => i.Id);
        var actual = db.QueryAsync<Guid>($"SELECT Id FROM {table_name}", CommandType.Text).Await();
        actual.should_be(expected, false);
    }

    private void the_duration_is_returned()
    {
        duration.should_be_greater_than(0);
    }

    private void it_is_copied_twice()
    {
        var expected = copiedIds.Append(new InputIds(existing_id)).Select(i => i.Id).ToList();
        expected.Add(existing_id);
        var actual = db.QueryAsync<Guid>($"SELECT Id FROM {table_name}", CommandType.Text).Await();
        actual.should_be(expected, false);
    }

    private void it_is_copied_thrice()
    {
        var expected = copiedIds.Append(new InputIds(existing_id)).Select(i => i.Id).ToList();
        expected.Add(existing_id);
        expected.Add(existing_id);
        expected.Add(existing_id);
        var actual = db.QueryAsync<Guid>($"SELECT Id FROM {table_name}", CommandType.Text).Await();
        actual.should_be(expected, false);
    }
}

public class InputIds
{
    public Guid Id { get; }

    public InputIds(Guid id)
    {
        Id = id;
    }
}