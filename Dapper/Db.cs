using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Sensemaking.Monitoring;

namespace Sensemaking.Dapper
{
    public interface IDb : ICanBeMonitored
    {
        Task ExecuteAsync(string sql, object? param = null, CommandType commandType = CommandType.StoredProcedure);
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, CommandType commandType = CommandType.Text);
        Task<T> QueryAsync<T>(string sql, Func<SqlMapper.GridReader, T> resultSelector, object? param = null, CommandType commandType = CommandType.StoredProcedure);
    }

    public class Db : IDb
    {
        static Db()
        {
            SqlMapper.AddTypeHandler(InstantHandler.Default);
            SqlMapper.AddTypeHandler(LocalDateHandler.Default);
            SqlMapper.AddTypeHandler(LocalDateTimeHandler.Default);
            SqlMapper.AddTypeHandler(LocalTimeHandler.Default);
            SqlMapper.AddTypeHandler(OffsetDateTimeHandler.Default);
            SqlMapper.AddTypeHandler(NullableInstantHandler.Default);
            SqlMapper.AddTypeHandler(NullableLocalDateHandler.Default);
            SqlMapper.AddTypeHandler(NullableLocalDateTimeHandler.Default);
            SqlMapper.AddTypeHandler(NullableLocalTimeHandler.Default);
            SqlMapper.AddTypeHandler(NullableOffsetDateTimeHandler.Default);
        }

        internal string ConnectionString { get; set; }

        private static readonly Func<SqlException, Exception> DefaultExceptionConversion = ex => ex;

        public Db(string connectionString)
        {
            Validation.BasedOn(errors =>
            {
                if(connectionString.IsNullOrEmpty())
                  errors.Add("A connection string is required.");
            });

            ConnectionString = connectionString;
            Monitor = new DbMonitor(this);
        }

        private SqlConnection CreateConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        public async Task ExecuteAsync(string sql, object? param = null,
            CommandType commandType = CommandType.StoredProcedure)
        {
            using (var connection = CreateConnection(ConnectionString))
                await connection.ExecuteAsync(sql, param, commandType: commandType);
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, CommandType commandType = CommandType.Text)
        {
            using (var connection = CreateConnection(ConnectionString))
                return await connection.QueryAsync<T>(sql, param, commandType: commandType);
        }

        public async Task<IEnumerable<T>> QueryAsync<T>((string Sql, object? Parameters) query, CommandType commandType = CommandType.Text)
        {
            return await QueryAsync<T>(query.Sql, query.Parameters, commandType);
        }

        public async Task<T> QueryAsync<T>(string sql, Func<SqlMapper.GridReader, T> resultSelector, object? param = null, CommandType commandType = CommandType.StoredProcedure)
        {
            using (var connection = CreateConnection(ConnectionString))
                return resultSelector(await connection.QueryMultipleAsync(sql, param, commandType: commandType));
        }

        public IMonitor Monitor { get; private set; }
    }
}