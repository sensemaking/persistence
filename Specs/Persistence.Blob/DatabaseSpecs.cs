using NUnit.Framework;
using Sensemaking.Bdd;

namespace Sensemaking.Specs.Persistence.Blob;

public partial class DatabaseSpecs : Specification
{
    [Test]
    public void the_same_blob_client_is_available()
    {
        Given(we_configure_blob);
        When(we_get_the_blob_client);
        And(we_get_the_blob_client_again);
        Then(the_blob_client_is_the_same);
    }

    [Test]
    public void the_blob_client_is_updated()
    {
        Given(we_configure_blob);
        And(we_get_the_blob_client);
        When(we_configure_blob_again);
        And(we_get_the_blob_client_again);
        Then(the_blob_client_is_not_the_same);
    }
}