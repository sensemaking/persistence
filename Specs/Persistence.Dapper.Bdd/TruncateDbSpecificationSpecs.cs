using Fdb.Rx.Test.Dapper;
using NUnit.Framework;

namespace Fdb.Rx.Testing.PersistenceTest.Dapper;

[TestFixture]
public partial class TruncateDbSpecificationSpecs : TruncateDbSpecification
{
    [Test]
    public void cleans_up_data_added_during_the_test()
    {
        Given(a_database);
        And(inserting_test_data);
        When(cleaning_up_the_test);
        Then(all_test_data_is_removed);
        And(excluded_table_data_is_not_removed);
    }

    [Test]
    public void cleans_up_data_that_exists_prior_to_running_the_test()
    {
        Given(a_database);
        And(inserting_test_data);
        When(starting_up_the_test);
        Then(all_test_data_is_removed);
        And(excluded_table_data_is_not_removed);
    }
}