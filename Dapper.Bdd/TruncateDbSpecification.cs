using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using Fdb.Rx.Persistence.Dapper;
using Sensemaking.Bdd;

namespace Fdb.Rx.Test.Dapper
{
    public abstract class TruncateDbSpecification : Specification
    {
        private readonly (string ConnectionString, string[] ExcludeFromTruncate)[] connections;
        private static string[] static_data_tables { get; set; } = Array.Empty<string>();

        protected TruncateDbSpecification(string connectionString, params string[] excludedTables)
            : this(new[] { (connectionString, schemaVersions: excludedTables) })
        {
        }

        protected TruncateDbSpecification(IEnumerable<(string ConnectionString, string[] excludedTables)> connections)
        {
            this.connections = connections
                .Select(c => (c.ConnectionString, ExcludeFromTruncate(c.excludedTables).Concat(static_data_tables).ToArray()))
                .ToArray();
        }

        public static void RegisterStaticDataTables(params string[] tables)
        {
            static_data_tables = tables;
        }
        
        protected override void before_each()
        {
            base.before_each();
            Truncate();
        }

        protected override void after_each()
        {
            Truncate();
            base.after_each();
        }

        private static string[] ExcludeFromTruncate(string[] excludedTables) => new [] { "dbo.SchemaVersions" }.Concat(excludedTables).ToArray();

        private void Truncate()
        {
            connections.ForEach(connection => Truncate(connection.ConnectionString, connection.ExcludeFromTruncate));
        }

        private void Truncate(string connectionString, string[] excludeFromTruncate)
        {
            if (!connectionString.IsLocallyConnected()) return;
            new Db(connectionString).Query<string>(
                   """
                       SELECT DISTINCT '[' + t.TABLE_SCHEMA + '].[' + t.TABLE_NAME + ']' As tableName
                       FROM INFORMATION_SCHEMA.TABLES t
                       WHERE TABLE_TYPE = 'BASE TABLE'
                   """)
                .FilterExcluded(excludeFromTruncate)
                .ForEach(tt => new Db(connectionString).Execute($"DELETE FROM {tt}", commandType: CommandType.Text));
        }
    }
    
    internal static class ConnectionStringExtensions
    {
        internal static bool IsLocallyConnected(this string connectionString)
        {
            var server = new SqlConnectionStringBuilder(connectionString).DataSource;
            return new[] { ".\\", ".", "(local)", "(localdb)\\mssqllocaldb", "(localdb)", "localhost", "127.0.0.1", Environment.MachineName }.Any(localServerName => localServerName.Equals(server, StringComparison.OrdinalIgnoreCase));
        }
        
        internal static IEnumerable<string> FilterExcluded(this IEnumerable<string> allTables, string[] excludeFromTruncate) => 
            allTables.Where(t => excludeFromTruncate.None(exclusion => exclusion.SchemaTableEquals(t)));
        internal static bool SchemaTableEquals(this string schemaTable,  string otherSchemaTable) => 
            string.Equals(schemaTable.NormalizeSchema(),otherSchemaTable.NormalizeSchema(),StringComparison.OrdinalIgnoreCase);
        internal static string NormalizeSchema(this string schemaTable) => 
            schemaTable.Replace("[", "").Replace("]", "");
    }
    
 
    
}