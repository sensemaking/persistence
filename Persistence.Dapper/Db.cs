using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Dapper.NodaTime;
using Sensemaking.Monitoring;

namespace Sensemaking.Persistence.Dapper
{
    public interface IDb : ICanBeMonitored
    {
        void Execute(string sql, object? param = null, CommandType commandType = CommandType.StoredProcedure, Func<SqlException, Exception>? exceptionConversion = null, int? commandTimeout = null);
        IEnumerable<T> Query<T>(string sql, object? param = null, CommandType commandType = CommandType.Text, Func<SqlException, Exception>? exceptionConversion = null, int? commandTimeout = null);
        T Query<T>(string sql, Func<SqlMapper.GridReader, T> resultSelector, object? param = null, CommandType commandType = CommandType.StoredProcedure, Func<SqlException, Exception>? exceptionConversion = null, int? commandTimeout = null);
    }

    public class Db : IDb
    {
        static Db()
        {
            DapperNodaTimeSetup.Register();
            SqlMapper.AddTypeHandler(new PeriodHandler());
        }

        internal string ConnectionString { get; set; }

        private static readonly Func<SqlException, Exception> DefaultExceptionConversion = ex => ex;

        internal Db(string connectionString)
        {
            ConnectionString = connectionString;
            Monitor = new DbMonitor(this);
        }

        private SqlConnection CreateConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        public void Execute(string sql, object? param = null, CommandType commandType = CommandType.StoredProcedure, Func<SqlException, Exception>? exceptionConversion = null, int? commandTimeout = null)
        {
            Try(exceptionConversion, () =>
            {
                using (var connection = CreateConnection(ConnectionString))
                    connection.Execute(sql, param, commandType: commandType, commandTimeout: commandTimeout);
            });
        }

        public IEnumerable<T> Query<T>(string sql, object? param = null, CommandType commandType = CommandType.Text, Func<SqlException, Exception>? exceptionConversion = null, int? commandTimeout = null)
        {
            return Try(exceptionConversion, () =>
            {
                using (var connection = CreateConnection(ConnectionString))
                    return connection.Query<T>(sql, param, commandType: commandType, commandTimeout: commandTimeout);
            });
        }

        public T Query<T>(string sql, Func<SqlMapper.GridReader, T> resultSelector, object? param = null, CommandType commandType = CommandType.StoredProcedure, Func<SqlException, Exception>? exceptionConversion = null, int? commandTimeout = null)
        {
            return Try(exceptionConversion, () =>
            {
                using (var connection = CreateConnection(ConnectionString))
                    return resultSelector(connection.QueryMultiple(sql, param, commandType: commandType, commandTimeout: commandTimeout));
            });
        }

        private static void Try(Func<SqlException, Exception>? exceptionConversion, Action execution)
        {
            try
            {
                execution();
            }
            catch (SqlException e)
            {
                throw (exceptionConversion ?? DefaultExceptionConversion)(e);
            }
        }

        private static T Try<T>(Func<SqlException, Exception>? exceptionHandler, Func<T> execution)
        {
            try
            {
                return execution();
            }
            catch (SqlException e)
            {
                throw (exceptionHandler ?? DefaultExceptionConversion)(e);
            }
        }

        public IMonitor Monitor { get; private set; }
    }
}