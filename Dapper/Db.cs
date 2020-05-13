using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Sensemaking.Monitoring;

namespace Sensemaking.Dapper
{
    public interface IDb : ICanBeMonitored
    {
        void Execute(string sql, object? param = null, CommandType commandType = CommandType.StoredProcedure);
        IEnumerable<T> Query<T>(string sql, object? param = null, CommandType commandType = CommandType.Text);
        T Query<T>(string sql, Func<SqlMapper.GridReader, T> resultSelector, object? param = null, CommandType commandType = CommandType.StoredProcedure);
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

        public void Execute(string sql, object? param = null, CommandType commandType = CommandType.StoredProcedure)
        {
            using (var connection = CreateConnection(ConnectionString))
                connection.Execute(sql, param, commandType: commandType);
        }

        public IEnumerable<T> Query<T>(string sql, object? param = null, CommandType commandType = CommandType.Text)
        {
            using (var connection = CreateConnection(ConnectionString))
                return connection.Query<T>(sql, param, commandType: commandType);
        }

        public T Query<T>(string sql, Func<SqlMapper.GridReader, T> resultSelector, object? param = null, CommandType commandType = CommandType.StoredProcedure)
        {
            using (var connection = CreateConnection(ConnectionString))
                return resultSelector(connection.QueryMultiple(sql, param, commandType: commandType));
        }

        public IMonitor Monitor { get; private set; }
    }
}