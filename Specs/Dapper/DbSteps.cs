using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Sensemaking.Bdd;

namespace Sensemaking.Dapper.Specs
{
    public partial class DbSpecs
    {
        internal static string table_name = "MyTable";
        internal static string proc_name = "MyProc";
        private static readonly Guid new_id = Guid.NewGuid();
        internal static readonly Guid existing_id = Guid.NewGuid();
        internal static readonly Guid wibble = Guid.NewGuid();
        private Db db;
        private IEnumerable<Guid> query_result;
        private string constructed_query_result;

        protected override void before_all()
        {
            base.before_all();
            using (var con = new SqlConnection(Startup.Database.connection_string))
            {
                con.Execute($"CREATE TABLE {table_name} (Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY); INSERT INTO {table_name} SELECT @existing_id;", new { existing_id });
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
            using (var con = new SqlConnection(Startup.Database.connection_string))
                con.Execute($"INSERT INTO {table_name} SELECT @wibble;", new { wibble });
            db = null;
        }

        private void a_database()
        {
            db = new Db(Startup.Database.connection_string);
        }

        private void executing()
        {
            db.Execute($"INSERT INTO {table_name} SELECT @new_id", new { new_id }, CommandType.Text);
        }

        private void querying()
        {
            query_result = db.Query<Guid>($"SELECT Id FROM {table_name} WHERE Id = @existing_id", new { existing_id });
        }

        private void querying_constructed_result()
        {
            constructed_query_result = db.Query(proc_name, reader => reader.ReadSingle<Guid>().ToString(), new { id = existing_id });
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
    }
}