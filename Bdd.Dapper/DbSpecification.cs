using System.Transactions;

namespace Sensemaking.Bdd.Dapper
{
    public abstract class DbSpecification : Specification
    {
        private TransactionScope scope;

        protected override void before_each()
        {
            base.before_each();
            scope?.Dispose();
            scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        }

        protected override void after_each()
        {
            base.after_each();
            scope?.Dispose();
        }
    }
}