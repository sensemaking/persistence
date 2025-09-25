using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FastMember;
using Sensemaking.Persistence.Security;
using NodaTime;

namespace Sensemaking.Persistence.Dapper
{
    public interface IDb : IQueryDb
    {
        void Execute(string sql, object? param = null, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null);
        Task ExecuteAsync(string sql, object? param = null, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null);
        long Copy<T>(string tableName, IEnumerable<T> data, int batchSize = 5000, int bulkCopyTimeoutInMinutes = 10, SqlBulkCopyOptions sqlBulkCopyOptions = SqlBulkCopyOptions.TableLock);
        Task<long> CopyAsync<T>(string tableName, IEnumerable<T> data, int batchSize = 5000, int bulkCopyTimeoutInMinutes = 10, SqlBulkCopyOptions sqlBulkCopyOptions = SqlBulkCopyOptions.TableLock);
        long Copy(string tableName, IDbCommand data, int batchSize = 5000, int bulkCopyTimeoutInMinutes = 10, SqlBulkCopyOptions sqlBulkCopyOptions = SqlBulkCopyOptions.TableLock);
        Task<long> CopyAsync(string tableName, IDbCommand data, int batchSize = 5000, int bulkCopyTimeoutInMinutes = 10, SqlBulkCopyOptions sqlBulkCopyOptions = SqlBulkCopyOptions.TableLock);
    }

    public class Db : DbReader, IDb
    {
        private const SqlBulkCopyOptions DefaultOptions = SqlBulkCopyOptions.TableLock;

        internal Db(string connectionString)
            : this(connectionString, new NoAccessTokens()) { }

        internal Db(string connectionString, IProvideAccessTokens tokenProvider)
            : base(connectionString, tokenProvider) { }

        public void Execute(string sql, object? param = null, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null)
        {
            using var connection = CreateConnection(ConnectionString);
            connection.Execute(sql, param, commandType: commandType, commandTimeout: commandTimeout);
        }

        public async Task ExecuteAsync(string sql, object? param = null, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null)
        {
            await using var connection = CreateConnection(ConnectionString);
            await connection.ExecuteAsync(sql, param, commandType: commandType, commandTimeout: commandTimeout).ConfigureAwait(false);
        }

        public long Copy<T>(string tableName, IEnumerable<T> data, int batchSize = 5000, int bulkCopyTimeoutInMinutes = 10, SqlBulkCopyOptions sqlBulkCopyOptions = DefaultOptions)
        {
            return TimeThis(() =>
            {
                using var connection = CreateConnection(ConnectionString);
                using var bulkCopy = CreateSqlBulkCopy(connection, tableName, batchSize, bulkCopyTimeoutInMinutes, sqlBulkCopyOptions);
                bulkCopy.WriteToServer(ObjectReader.Create(data));
            });
        }

        public Task<long> CopyAsync<T>(string tableName, IEnumerable<T> data, int batchSize = 5000, int bulkCopyTimeoutInMinutes = 10, SqlBulkCopyOptions sqlBulkCopyOptions = DefaultOptions)
        {
            return TimeThisAsync(async () =>
            {
                using var connection = CreateConnection(ConnectionString);
                using var bulkCopy = CreateSqlBulkCopy(connection, tableName, batchSize, bulkCopyTimeoutInMinutes, sqlBulkCopyOptions);
                await bulkCopy.WriteToServerAsync(ObjectReader.Create(data));
            });
        }

        public long Copy(string tableName, IDbCommand data, int batchSize = 5000, int bulkCopyTimeoutInMinutes = 10, SqlBulkCopyOptions sqlBulkCopyOptions = DefaultOptions)
        {
            return TimeThis(() =>
            {
                using var connection = CreateConnection(ConnectionString);
                using var bulkCopy = CreateSqlBulkCopy(connection, tableName, batchSize, bulkCopyTimeoutInMinutes, sqlBulkCopyOptions);
                using var executeReader = data.ExecuteReader();
                bulkCopy.WriteToServer(executeReader);
            });
        }

        public Task<long> CopyAsync(string tableName, IDbCommand data, int batchSize = 5000, int bulkCopyTimeoutInMinutes = 10, SqlBulkCopyOptions sqlBulkCopyOptions = DefaultOptions)
        {
            return TimeThisAsync(async () =>
            {
                using var connection = CreateConnection(ConnectionString);
                using var bulkCopy = CreateSqlBulkCopy(connection, tableName, batchSize, bulkCopyTimeoutInMinutes, sqlBulkCopyOptions);
                using var executeReader = data.ExecuteReader();
                await bulkCopy.WriteToServerAsync(executeReader).ConfigureAwait(false);
            });
        }

        private long TimeThis(Action action)
        {
            var timer = new Stopwatch();
            timer.Start();
            action();
            timer.Stop();
            return timer.ElapsedMilliseconds;
        }

        private async Task<long> TimeThisAsync(Func<Task> action)
        {
            var timer = new Stopwatch();
            timer.Start();
            await action().ConfigureAwait(false);
            timer.Stop();
            return timer.ElapsedMilliseconds;
        }

        private SqlBulkCopy CreateSqlBulkCopy(SqlConnection connection, string tableName, int batchSize, int bulkCopyTimeoutInMinutes, SqlBulkCopyOptions sqlBulkCopyOptions)
        {
            connection.Open();
            var bulkCopy = new SqlBulkCopy(connection, sqlBulkCopyOptions, null)
            {
                BatchSize = batchSize,
                EnableStreaming = true,
                DestinationTableName = tableName,
                BulkCopyTimeout = (int)Duration.FromMinutes(bulkCopyTimeoutInMinutes).ToTimeSpan().TotalSeconds,
            };

            tableName.GetColumns(this).ForEach(column => bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(column, column)));
            return bulkCopy;
        }
    }

    public static class SqlBulkCopyExtensions
    {
        public static string[] GetColumns(this string tableName, IDb db)
        {
            var (name, schema) = GetNameAndSchema(tableName);
            return db.Query<string>("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @name and TABLE_SCHEMA = @schema", new { name, schema }).ToArray();
        }

        public static Task<IReadOnlyList<string>> GetColumnsAsync(this string tableName, IDb db)
        {
            var (name, schema) = GetNameAndSchema(tableName);
            return db.QueryAsync<string>("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @name and TABLE_SCHEMA = @schema", new { name, schema });
        }

        private static (string name, string schema) GetNameAndSchema(string tableName)
        {
            var tableParts = tableName.Split('.');
            var (name, schema) = tableParts.Length == 1 ? (name: tableParts[0].StripSquareBrackets(), schema: "dbo") : (name: tableParts[1].StripSquareBrackets(), schema: tableParts[0].StripSquareBrackets());
            return (name, schema);
        }

        private static string StripSquareBrackets(this string sqlName) => sqlName.Replace("[", "").Replace("]", "");
    }
}