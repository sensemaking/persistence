using System.Transactions;
using Sensemaking.Bdd;

namespace Persistence.Dapper.Bdd
{
    public abstract class DbSpecification : Specification
    {
        private TransactionScope scope;

        protected override void before_each()
        {
            base.before_each();
            scope?.Dispose();
            scope = new TransactionScope(TransactionScopeOption.RequiresNew);
        }

        protected override void after_each()
        {
            base.after_each();
            scope?.Dispose();
        }
    }
}