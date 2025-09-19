using NUnit.Framework;
using Sensemaking.Bdd;

namespace Fdb.Rx.Testing.Persistence.Cosmos.Query;

public abstract partial class QuerySpecs : Specification
{
    [Test]
    public void gets_results()
    {
        Given(existing_documents);
        When(getting_results_via_query);
        Then(documents_are_retrieved);
    }
}