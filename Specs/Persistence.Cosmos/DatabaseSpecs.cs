using NUnit.Framework;
using Sensemaking.Bdd;

namespace Fdb.Rx.Testing.Persistence.Cosmos;

public partial class DatabaseSpecs : Specification
{
    [Test]
    public void the_same_cosmos_client_is_available()
    {
        When(we_get_the_cosmos_client);
        And(we_get_the_cosmos_client_again);
        Then(the_cosmos_client_is_the_same);
    }

    [Test]
    public void the_cosmos_client_is_updated()
    {
        Given(we_get_the_cosmos_client);
        When(we_configure_cosmos_again);
        And(we_get_the_cosmos_client_again);
        Then(the_cosmos_client_is_not_the_same);
    }
}