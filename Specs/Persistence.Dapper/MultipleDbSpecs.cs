using Sensemaking.Bdd;
using NUnit.Framework;

namespace Sensemaking.Specs.Persistence.Dapper;

[TestFixture]
public partial class MultipleDbSpecs : Specification
{
    [Test]
    public void executes_against_all_available_databases()
    {
        Given(it_has_an_unavailable_database);
        And(an_available_database);
        And(another_available_database);
        When(executing_a_command);
        Then(it_executes_against_all_available_databases);
    }

    [Test]
    public void queries_all_available_databases_returning_first_with_results()
    {
        scenario(() =>
        {
            Given(query_coalesce_option);
            And(it_has_an_unavailable_database);
            And(an_available_database);
            And(another_available_database);
            And(the_first_has_results);
            When(querying);
            Then(returns_the_results);
        });

        scenario(() =>
        {
            Given(query_coalesce_option);
            And(it_has_an_unavailable_database);
            And(it_has_an_unavailable_database);
            And(an_available_database);
            And(another_available_database);
            And(the_second_has_results);
            When(querying);
            Then(returns_the_results);
        });

        scenario(() =>
        {
            Given(query_coalesce_option);
            And(it_has_an_unavailable_database);
            And(an_available_database);
            And(another_available_database);
            And(neither_has_results);
            When(querying);
            Then(returns_empty_results);
        });
    }

    [Test]
    public void queries_all_available_databases_returning_merged_results()
    {
        scenario(() =>
        {
            Given(query_merge_option);
            And(it_has_an_unavailable_database);
            And(an_available_database);
            And(another_available_database);
            And(the_first_has_results);
            And(the_second_has_different_results);
            When(querying);
            Then(returns_the_merged_results);
        });

        scenario(() =>
        {
            Given(query_merge_option);
            And(it_has_an_unavailable_database);
            And(it_has_an_unavailable_database);
            And(an_available_database);
            And(another_available_database);
            And(the_second_has_results);
            When(querying);
            Then(returns_the_results);
        });

        scenario(() =>
        {
            Given(query_merge_option);
            And(it_has_an_unavailable_database);
            And(an_available_database);
            And(another_available_database);
            And(neither_has_results);
            When(querying);
            Then(returns_empty_results);
        });
    }

    [Test]
    public void queries_all_available_databases_returning_first_constructed_results()
    {
        scenario(() =>
        {
            Given(query_coalesce_option);
            And(it_has_an_unavailable_database);
            And(an_available_database);
            And(another_available_database);
            And(the_first_has_results);
            When(querying_and_constructing_results);
            Then(returns_the_constructed_results);
        });

        scenario(() =>
        {
            Given(query_coalesce_option);
            And(it_has_an_unavailable_database);
            And(an_available_database);
            And(another_available_database);
            And(the_second_has_results);
            When(querying_and_constructing_results);
            Then(returns_the_constructed_results);
        });

        scenario(() =>
        {
            Given(query_coalesce_option);
            And(it_has_an_unavailable_database);
            And(an_available_database);
            And(another_available_database);
            And(neither_has_results);
            When(querying_and_constructing_results);
            Then(returns_empty_constructed_results);
        });
    }

    [Test]
    [Description("Due to random nature may fail incorrectly 1 in every 1,000,000")]
    public void queries_one_randomly_chosen_available_database_returning_its_results()
    {
        Given(query_one_option);
        And(it_has_an_unavailable_database);
        And(an_available_database);
        And(another_available_database);
        When(() => querying(single_available_databases_is_queried_with_which_one_chosen_randomly));
    }

    [Test]
    [Description("Due to random nature may fail incorrectly 1 in every 1,000,000")]
    public void queries_one_randomly_chosen_available_database_returning_its_constructed_results()
    {
        Given(query_one_option);
        And(it_has_an_unavailable_database);
        And(an_available_database);
        And(another_available_database);
        When(() => querying_and_constructing_results(
            single_available_databases_is_queried_with_which_one_chosen_randomly));
    }
}