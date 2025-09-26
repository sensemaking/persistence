using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Sensemaking.Persistence.Security;

namespace Sensemaking.Persistence.Dapper;

public interface IDb : IQueryDb
{
    Task ExecuteAsync(string sql, object? param = null, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null);
}

public class Db : DbReader, IDb
{
    private const SqlBulkCopyOptions DefaultOptions = SqlBulkCopyOptions.TableLock;

    internal Db(string connectionString)
        : this(connectionString, new NoAccessTokens()) { }

    internal Db(string connectionString, IProvideAccessTokens tokenProvider)
        : base(connectionString, tokenProvider) { }

    public async Task ExecuteAsync(string sql, object? param = null, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null)
    {
        await using var connection = CreateConnection(ConnectionString);
        await connection.ExecuteAsync(sql, param, commandType: commandType, commandTimeout: commandTimeout).ConfigureAwait(false);
    }
}