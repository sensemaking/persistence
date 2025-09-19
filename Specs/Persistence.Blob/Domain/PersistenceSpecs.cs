using NUnit.Framework;

namespace Sensemaking.Specs.Persistence.Blob.Domain;

[TestFixture]
public partial class PersistenceSteps
{
    [Test]
    public override void save_and_gets_aggregates()
    {
        Given(an_aggregate);
        And(another_aggregate);
        And(a_third_aggregate);

        When(getting_the_aggregate);
        Then(the_aggregate_is_retrieved);

        When(getting_the_aggregates);
        Then(the_aggregates_are_retrieved);
    }
}