using Microsoft.Data.SqlClient;
using System.Transactions;
using Dapper;
using Fdb.Rx.Domain;
using Fdb.Rx.Persistence.Dapper;
using NUnit.Framework;

namespace Sensemaking.Specs.Persistence.Dapper.Domain;

[TestFixture]
public class PersistenceSteps() : PersistenceSpecsTemplate(() => RepositoryBuilder.For.Dapper(new Db(Startup.Database.connection_string))
    .Register<AnAggregate>(container_name, null).Get().Repository)
{
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
            con.Execute($"CREATE TABLE {container_name} (Id varchar(200), Document varchar(MAX))");
    }

    protected override void after_all()
    {
        using (var con = new SqlConnection(Startup.Database.connection_string))
            con.Execute($"DROP TABLE {container_name}");

        base.after_all();
    }
}