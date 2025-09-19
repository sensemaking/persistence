using System;
using System.Data;
using System.Linq;
using Dapper;
using Fdb.Rx.Persistence.Dapper;
using Microsoft.Data.SqlClient;
using Sensemaking.Bdd;

namespace Sensemaking.Specs.PersistenceTest.Dapper;

public partial class TruncateMultipleDbSpecificationSpecs
{
    internal static string table_name = "TruncateDbSpecificationTable";
    internal static string referenced_table_name = "dbo.TruncateDbSpecificationReferencedTable";
    internal static string referenced_table_view_name = "dbo.TruncateDbSpecificationReferencedTableView";
    internal static string table_name_to_skip = "TruncateDbSpecificationMemoryOptimizedTableToSkip";
    private static readonly Guid new_id = Guid.NewGuid();

    public TruncateMultipleDbSpecificationSpecs()
        : base(new[]
        {
            (Startup.Database.connection_string, new[] { "[dbo].[SchemaVersions]", $"[dbo].{table_name_to_skip}" }),
            (Startup.AnotherDatabase.connection_string, Array.Empty<string>())
        })
    {
    }

    protected override void before_all()
    {
        base.before_all();
        using (var con = new SqlConnection(Startup.Database.connection_string))
        {
            con.Execute($"""
                         CREATE TABLE {table_name} (Id UNIQUEIDENTIFIER NOT NULL);

                         CREATE TABLE {referenced_table_name} (Id UNIQUEIDENTIFIER NOT NULL)

                         CREATE TABLE {table_name_to_skip} (
                             Id UNIQUEIDENTIFIER NOT NULL,
                             constraint {table_name_to_skip}_PK  primary key nonclustered ([Id])
                         );

                         INSERT INTO {table_name_to_skip} SELECT @id

                         CREATE TABLE dbo.SchemaVersions (Id UNIQUEIDENTIFIER NOT NULL);

                         INSERT INTO dbo.SchemaVersions SELECT @id
                         """, new { id = Guid.NewGuid() }, commandType: CommandType.Text);
        }

        using (var con = new SqlConnection(Startup.Database.connection_string))
        {
            con.Execute($"""CREATE VIEW {referenced_table_view_name} WITH SCHEMABINDING AS SELECT Id FROM {referenced_table_name}""");
        }

        using (var con = new SqlConnection(Startup.Database.connection_string))
        {
            con.Execute($"""CREATE UNIQUE CLUSTERED INDEX [CIX_TruncateDbSpecificationReferencedTableView] ON {referenced_table_view_name} (Id)""");
        }

        using (var con = new SqlConnection(Startup.AnotherDatabase.connection_string))
        {
            con.Execute($"""
                         CREATE TABLE {table_name} (Id UNIQUEIDENTIFIER NOT NULL);

                         CREATE TABLE {referenced_table_name} (Id UNIQUEIDENTIFIER NOT NULL);

                         CREATE TABLE dbo.SchemaVersions (Id UNIQUEIDENTIFIER NOT NULL);

                         INSERT INTO dbo.SchemaVersions SELECT @id
                         """, new { id = Guid.NewGuid() }, commandType: CommandType.Text);
        }

        using (var con = new SqlConnection(Startup.AnotherDatabase.connection_string))
        {
            con.Execute($"""CREATE VIEW {referenced_table_view_name} WITH SCHEMABINDING AS SELECT Id FROM {referenced_table_name}""");
        }

        using (var con = new SqlConnection(Startup.AnotherDatabase.connection_string))
        {
            con.Execute($"""CREATE UNIQUE CLUSTERED INDEX [CIX_TruncateDbSpecificationReferencedTableView] ON {referenced_table_view_name} (Id)""");
        }
    }

    protected override void after_all()
    {
        using (var con = new SqlConnection(Startup.Database.connection_string))
            con.Execute($"""
                         DROP TABLE {table_name};
                         DROP VIEW {referenced_table_view_name};
                         DROP TABLE {referenced_table_name};
                         DROP TABLE {table_name_to_skip};
                         DROP TABLE dbo.SchemaVersions;
                         """);
        using (var con = new SqlConnection(Startup.AnotherDatabase.connection_string))
            con.Execute($"""
                         DROP TABLE {table_name};
                         DROP VIEW {referenced_table_view_name};
                         DROP TABLE {referenced_table_name};
                         DROP TABLE dbo.SchemaVersions;
                         """);
        base.after_all();
    }

    private void a_database()
    {
    }

    private void another_database()
    {
    }

    private void inserting_test_data()
    {
        new Db(Startup.Database.connection_string).Execute($"INSERT INTO {table_name} SELECT @new_id", new { new_id }, CommandType.Text);
        new Db(Startup.Database.connection_string).Execute($"INSERT INTO {referenced_table_name} SELECT @new_id", new { new_id }, CommandType.Text);
    }

    private void inserting_test_data_into_another_database()
    {
        new Db(Startup.AnotherDatabase.connection_string).Execute($"INSERT INTO {table_name} SELECT @new_id", new { new_id }, CommandType.Text);
        new Db(Startup.AnotherDatabase.connection_string).Execute($"INSERT INTO {referenced_table_name} SELECT @new_id", new { new_id }, CommandType.Text);
    }

    private void cleaning_up_the_test() => after_each();

    private void all_test_data_is_removed()
    {
        new Db(Startup.Database.connection_string).Query<dynamic>($"SELECT * FROM {table_name}", commandType: CommandType.Text).should_be_empty();
        new Db(Startup.Database.connection_string).Query<dynamic>($"SELECT * FROM {referenced_table_name}", commandType: CommandType.Text).should_be_empty();
        new Db(Startup.AnotherDatabase.connection_string).Query<dynamic>($"SELECT * FROM {table_name}", commandType: CommandType.Text).should_be_empty();
        new Db(Startup.AnotherDatabase.connection_string).Query<dynamic>($"SELECT * FROM {referenced_table_name}", commandType: CommandType.Text).should_be_empty();
    }

    private void excluded_table_data_is_not_removed()
    {
        new Db(Startup.Database.connection_string).Query<dynamic>($"SELECT * FROM dbo.SchemaVersions", commandType: CommandType.Text).ToArray().Length.should_be(1);
        new Db(Startup.Database.connection_string).Query<dynamic>($"SELECT * FROM dbo.{table_name_to_skip}", commandType: CommandType.Text).ToArray().Length.should_be(1);

        new Db(Startup.AnotherDatabase.connection_string).Query<dynamic>($"SELECT * FROM dbo.SchemaVersions", commandType: CommandType.Text).ToArray().Length.should_be(1);
    }

    private void starting_up_the_test() => before_each();
}