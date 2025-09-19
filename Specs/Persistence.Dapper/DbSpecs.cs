using System;
using NUnit.Framework;
using Sensemaking.Bdd;

namespace Fdb.Rx.Testing.Persistence.Dapper;

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
    public void can_copy_in_bulk()
    {
        Given(a_database);
        When(copying_in_bulk);
        Then(it_is_copied);
        And(the_duration_is_returned);
    }

    [Test]
    public void can_copy_in_bulk_async()
    {
        Given(a_database);
        When(copying_in_bulk_async);
        Then(it_is_copied);
        And(the_duration_is_returned);
    }

    [Test]
    public void can_copy_in_bulk_from_a_previous_command()
    {
        Given(a_database);
        When(copying_in_bulk);
        And(there_is_a_command);
        When(copying_in_bulk_from_the_previous_command);
        Then(it_is_copied_twice);
    }

    [Test]
    public void can_copy_in_bulk_from_a_previous_command_async()
    {
        Given(a_database);
        When(copying_in_bulk);
        And(there_is_a_command);
        When(copying_in_bulk_from_the_previous_command_async);
        Then(it_is_copied_twice);
    }

    [Test]
    public void can_copy_in_bulk_from_a_previous_command_twice()
    {
        Given(a_database);
        When(copying_in_bulk);
        And(there_is_a_command);
        And(copying_in_bulk_from_the_previous_command);
        And(copying_in_bulk_from_the_previous_command);
        Then(it_is_copied_thrice);
    }

    [Test]
    public void can_copy_in_bulk_from_a_previous_command_twice_async()
    {
        Given(a_database);
        When(copying_in_bulk);
        And(there_is_a_command);
        And(copying_in_bulk_from_the_previous_command_async);
        And(copying_in_bulk_from_the_previous_command_async);
        Then(it_is_copied_thrice);
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