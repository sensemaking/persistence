using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Sensemaking.Monitoring;
using Sensemaking.NodaTime.Dapper;
using Sensemaking.Persistence.Security;
using Sensemaking.Resilience;

namespace Sensemaking.Persistence.Dapper
{
    public interface IQueryDb : ICanBeMonitored
    {
        IEnumerable<T> Query<T>(string sql, object? param = null, CommandType commandType = CommandType.Text, Func<SqlException, Exception>? exceptionConversion = null, int? commandTimeout = null);
        Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param = null, CommandType commandType = CommandType.Text, Func<SqlException, Exception>? exceptionConversion = null, int? commandTimeout = null);
        T Query<T>(string sql, Func<SqlMapper.GridReader, T> resultSelector, object? param = null, CommandType commandType = CommandType.StoredProcedure, Func<SqlException, Exception>? exceptionConversion = null, int? commandTimeout = null);
        Task<T> QueryAsync<T>(string sql, Func<SqlMapper.GridReader, T> resultSelector, object? param = null, CommandType commandType = CommandType.StoredProcedure, Func<SqlException, Exception>? exceptionConversion = null, int? commandTimeout = null);
    }

    public class DbReader : IQueryDb
    {
        static DbReader()
        {
            NodaTimeForDapper.Register();
        }

        public IMonitor Monitor { get; private set; }
        internal string ConnectionString { get; set; }
        private readonly IProvideAccessTokens TokenProvider;
        protected static readonly Func<SqlException, Exception> DefaultExceptionConversion = ex => ex;

        internal DbReader(string connectionString)
            : this(connectionString, new NoAccessTokens()) { }

        internal DbReader(string connectionString, IProvideAccessTokens tokenProvider)
        {
            ConnectionString = connectionString;
            TokenProvider = tokenProvider;
            Monitor = new DbMonitor(this);
        }

        protected SqlConnection CreateConnection(string connectionString)
        {
            var conn = new SqlConnection(connectionString);
            var token = TokenProvider.GetToken();
            if (!token.IsNullOrEmpty())
                conn.AccessToken = TokenProvider.GetToken();

            return conn;
        }

        public IEnumerable<T> Query<T>(string sql, object? param = null, CommandType commandType = CommandType.Text, Func<SqlException, Exception>? exceptionConversion = null, int? commandTimeout = null)
        {
            using var connection = CreateConnection(ConnectionString);
            return connection.Query<T>(sql, param, commandType: commandType, commandTimeout: commandTimeout);
        }

        public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param = null, CommandType commandType = CommandType.Text, Func<SqlException, Exception>? exceptionConversion = null, int? commandTimeout = null)
        {
            await using var connection = CreateConnection(ConnectionString);
            return (await connection.QueryAsync<T>(sql, param, commandType: commandType, commandTimeout: commandTimeout).ConfigureAwait(false)).ToList();
        }

        public T Query<T>(string sql, Func<SqlMapper.GridReader, T> resultSelector, object? param = null, CommandType commandType = CommandType.StoredProcedure, Func<SqlException, Exception>? exceptionConversion = null, int? commandTimeout = null)
        {
            using var connection = CreateConnection(ConnectionString);
            return resultSelector(connection.QueryMultiple(sql, param, commandType: commandType, commandTimeout: commandTimeout));
        }

        public async Task<T> QueryAsync<T>(string sql, Func<SqlMapper.GridReader, T> resultSelector, object? param = null, CommandType commandType = CommandType.StoredProcedure, Func<SqlException, Exception>? exceptionConversion = null, int? commandTimeout = null)
        {
            await using var connection = CreateConnection(ConnectionString);
            return resultSelector(await connection.QueryMultipleAsync(sql, param, commandType: commandType, commandTimeout: commandTimeout).ConfigureAwait(false));
        }
    }
}