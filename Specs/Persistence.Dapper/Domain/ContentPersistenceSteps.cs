using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Transactions;
using Dapper;
using Sensemaking.Persistence.Dapper;
using NUnit.Framework;

namespace Sensemaking.Specs.Persistence.Dapper.Domain;

[TestFixture]
public abstract class ContentPersistenceSteps() : ContentPersistenceSpecsTemplate(() => new DapperPersistence(new Db(Startup.Database.connection_string)))
{
    private const string publication_table_suffix = "AsPublished";
    private TransactionScope scope;

    protected override void before_each()
    {
        base.before_each();
        scope?.Dispose();
        scope = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled);
    }

    protected override void after_each()
    {
        base.after_each();
        scope?.Dispose();
    }

    protected override void before_all()
    {
        base.before_all();
        using (var con = new SqlConnection(Startup.Database.connection_string))
        {
            con.Execute($"CREATE TABLE {container_name} (Id varchar(200), Document varchar(MAX))");
            con.Execute($"CREATE TABLE {container_name}{publication_table_suffix} (Id varchar(200), Document varchar(MAX))");
        }
    }

    protected override void after_all()
    {
        using (var con = new SqlConnection(Startup.Database.connection_string))
        {
            con.Execute($"DROP TABLE {container_name}");
            con.Execute($"DROP TABLE {container_name}{publication_table_suffix}");
        }

        base.after_all();
    }
}