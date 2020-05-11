using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using Dapper;
using Dapper.NodaTime;
using Sensemaking.Monitoring;

namespace Sensemaking.Persistence.Dapper
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