using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Sensemaking.Monitoring;

namespace Sensemaking.Persistence.Dapper
{
    public class MultipleDb : IDb
    {
        public enum QueryOptions
        {
            One = 0,
            Coalesce = 1,
            Merge = 2
        }

        internal readonly IDb[] Dbs;
        private readonly MultiInstanceMonitor monitor;
        private readonly QueryOptions options;

        internal MultipleDb(IEnumerable<IDb> dbs, QueryOptions options = QueryOptions.One)
        {
            this.options = options;
            this.Dbs = dbs.ToArray();
            monitor = new MultiInstanceMonitor(this.Dbs.Select(x => x.Monitor));
        }

        public void Execute(string sql, object? param = null, CommandType commandType = CommandType.StoredProcedure, Func<SqlException, Exception>? exceptionConversion = null, int? commandTimeout = null)
        {
            Parallel.ForEach(Dbs.GetAvailable(), x => x.Execute(sql, param, commandType, exceptionConversion, commandTimeout));
        }

        public IEnumerable<T> Query<T>(string sql, object? param = null, CommandType commandType = CommandType.Text, Func<SqlException, Exception>? exceptionConversion = null, int? commandTimeout = null)
        {
            var results = new List<T>();
            foreach (var db in options == QueryOptions.One ? Dbs.PickAvailableOne() : Dbs.GetAvailable())
            {
                results.AddRange(db.Query<T>(sql, param, commandType, exceptionConversion, commandTimeout));
                if (results.Any() && options != QueryOptions.Merge)
                    break;
            }

            if (options == QueryOptions.Merge)
                results = results.Distinct().ToList();

            return results;
        }

        [return: MaybeNull]
        public T Query<T>(string sql, Func<SqlMapper.GridReader, T> resultSelector, object? param = null, CommandType commandType = CommandType.StoredProcedure, Func<SqlException, Exception>? exceptionConversion = null, int? commandTimeout = null)
        {
            var result = default(T);
            foreach (var db in options == QueryOptions.One ? Dbs.PickAvailableOne() : Dbs.GetAvailable())
            {
                result = db.Query(sql, resultSelector, param, commandType, exceptionConversion, commandTimeout);
                if (!result!.Equals(default(T)))
                    break;
            }
            return result;
        }

        public IMonitor Monitor => monitor;
    }

    internal static class DatabasesExtensions
    {
        private static readonly Random Rand = new Random();
        private static readonly object randLock = new object();

        internal static IEnumerable<IDb> GetAvailable(this IEnumerable<IDb> dbs)
        {
            var available = dbs.Where(x => x.Monitor.Availability());

            if (available.Any())
                return available.Shuffle();

            throw new ServiceAvailabilityException();
        }

        internal static IEnumerable<IDb> PickAvailableOne(this IEnumerable<IDb> dbs)
        {
            return dbs.GetAvailable().Take(1);
        }

        internal static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            var elements = source.ToArray();
            for (var i = elements.Length - 1; i >= 0; i--)
            {
                int swapIndex;
                lock (randLock)
                    swapIndex = Rand.Next(i + 1);

                yield return elements[swapIndex];
                elements[swapIndex] = elements[i];
            }
        }
    }
}