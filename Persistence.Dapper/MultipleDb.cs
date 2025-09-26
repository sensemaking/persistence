using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Sensemaking.Monitoring;

namespace Sensemaking.Persistence.Dapper;

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

    public Task ExecuteAsync(string sql, object? param = null, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null)
    {
        return Task.WhenAll(Dbs.GetAvailable().Select(x => x.ExecuteAsync(sql, param, commandType, commandTimeout)));
    }

    public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param = null, CommandType commandType = CommandType.Text, Func<SqlException, Exception>? exceptionConversion = null, int? commandTimeout = null)
    {
        var results = new List<T>();
        foreach (var db in options == QueryOptions.One ? Dbs.PickAvailableOne() : Dbs.GetAvailable())
        {
            results.AddRange(await db.QueryAsync<T>(sql, param, commandType, exceptionConversion, commandTimeout).ConfigureAwait(false));
            if (results.Any() && options != QueryOptions.Merge)
                break;
        }

        if (options == QueryOptions.Merge)
            results = results.Distinct().ToList();

        return results;
    }

    public async Task<T> QueryAsync<T>(string sql, Func<SqlMapper.GridReader, T> resultSelector, object? param = null, CommandType commandType = CommandType.StoredProcedure, Func<SqlException, Exception>? exceptionConversion = null, int? commandTimeout = null)
    {
        var result = default(T);
        foreach (var db in options == QueryOptions.One ? Dbs.PickAvailableOne() : Dbs.GetAvailable())
        {
            result = await db.QueryAsync(sql, resultSelector, param, commandType, exceptionConversion, commandTimeout).ConfigureAwait(false);
            if (!(result == null || result.Equals(default(T))))
                break;
        }
        return result!;
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

    private static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
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