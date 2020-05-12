using NUnit.Framework;
using Sensemaking.Bdd.Dapper;

namespace Persistence.Dapper.Specs
{
    [TestFixture]
    public partial class DbSpecs : DbSpecification
    {
        [Test]
        public void can_execute_commands()
        {
            Given(a_database);
            When(executing);
            Then(it_is_carried_out);
        }

        [Test]
        public void can_query()
        {
            Given(a_database);
            When(querying);
            Then(results_are_returned);
        }

        [Test]
        public void can_query_and_construct_results()
        {
            Given(a_database);
            When(querying_constructed_result);
            Then(constructed_result_is_returned);
        }
    }
}