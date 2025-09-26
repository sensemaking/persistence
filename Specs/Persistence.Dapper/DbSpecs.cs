using System;
using NUnit.Framework;
using Sensemaking.Bdd;

namespace Sensemaking.Specs.Persistence.Dapper;

[TestFixture]
public partial class DbSpecs : Specification
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
    public void can_query_async()
    {
        Given(a_database);
        When(querying_async);
        Then(results_are_returned);
    }

    [Test]
    public void can_query_and_construct_results()
    {
        Given(a_database);
        When(querying_constructed_result);
        Then(constructed_result_is_returned);
    }

    [Test]
    public void can_query_and_construct_results_async()
    {
        Given(a_database);
        When(querying_constructed_result_async);
        Then(constructed_result_is_returned);
    }
}