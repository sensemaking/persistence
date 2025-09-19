using System;
using System.Transactions;
using Sensemaking.Bdd;

namespace Sensemaking.Test.Dapper;

public abstract class DbSpecification : Specification
{
    private TransactionScope? scope;

    protected override void before_each()
    {
        base.before_each();
        scope?.Dispose();

        var transactionOptions = new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted, Timeout = TimeSpan.FromMinutes(2) };
        scope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionOptions, TransactionScopeAsyncFlowOption.Enabled);
    }

    protected override void after_each()
    {
        base.after_each();
        scope?.Dispose();
    }
}