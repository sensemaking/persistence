using NUnit.Framework;

namespace Sensemaking.Dapper.Specs
{
    [TestFixture]
    public partial class DbRepositorySpecs
    {
        [Test]
        public void gets_aggregates_from_a_sql_database()
        {
            Given(an_aggregate);
            And(it_is_saved);
            When(getting_the_aggregate);
            Then(the_aggregate_is_retrieved);
        }
        
        [Test]
        public void deletes_aggregates_from_a_sql_database()
        {
            Given(an_aggregate);
            And(it_is_saved);
            And(it_is_deleted);
            When(getting_the_aggregate);
            Then(the_aggregate_is_not_retrieved);
        }

        [Test]
        public void gets_all_published_versions_of_a_particular_aggregate_from_a_sql_database()
        {
            Given(an_aggregate);
            And(it_is_published);
            And(it_is_changed);
            And(it_is_saved);
            When(getting_all_published_versions);
            Then(the_published_version_is_retrieved);
        }

        [Test]
        public void unpublishes_published_versions_of_aggregates_from_a_sql_database()
        {
            Given(an_aggregate);
            And(it_is_published2);
            And(it_is_un_published);
            When(getting_all_published_versions);
            Then(the_published_version_is_not_retrieved);
        }
    }
}