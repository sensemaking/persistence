using System;
using Azure.Storage.Blobs;
using Sensemaking.Persistence.Blob;
using Sensemaking.Test;
using Sensemaking.Bdd;

namespace Sensemaking.Specs.Persistence.Blob;

public partial class DatabaseSpecs
{
    private BlobServiceClient first_blob_client;
    private BlobServiceClient second_blob_client;

    protected override void before_each()
    {
        base.before_each();
        first_blob_client = null;
        second_blob_client = null;
    }

    private void we_configure_blob()
    {
        Account.Configure(Settings.Storage.EmulatorConnectionString);
    }

    private void we_configure_blob_again() => we_configure_blob();

    private void we_get_the_blob_client()
    {
        first_blob_client = Account.GetClient();
    }

    private void we_get_the_blob_client_again()
    {
        second_blob_client = Account.GetClient();
    }

    private void the_blob_client_is_the_same()
    {
        ReferenceEquals(first_blob_client, second_blob_client).should_be_true();
    }

    private void the_blob_client_is_not_the_same()
    {
        ReferenceEquals(first_blob_client, second_blob_client).should_be_false();
    }
}