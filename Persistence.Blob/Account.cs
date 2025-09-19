using System;
using System.Threading;
using Azure.Identity;
using Azure.Storage.Blobs;

namespace Fdb.Rx.Persistence.Blob
{
    public struct StorageSettings
    {
        public StorageSettings(string accountName, string accountKey)
        {
            Validation.BasedOn(errors =>
            {
                if (accountName.IsNullOrEmpty())
                    errors.Add("Account name is required");

                if (accountKey.IsNullOrEmpty())
                    errors.Add("Account key is required");
            });
            AccountName = accountName;
            AccountKey = accountKey;
        }

        public string AccountName { get; private set; }
        public string AccountKey { get; private set; }
    }

    public static class Account
    {
        private static string StorageConnectionString = null!;
        private static bool UsingManagedIdentity;
        private static string? UserAssignedClientId = null!;
        private static Uri ServiceUri = null!;
        private static bool HasChanged;
        public static string Name { get; set; } = null!;

        private static Lazy<BlobServiceClient> LazyClient = null!;

        private const string EndpointSuffix = "core.windows.net";

        internal static string BuildStorageConnectionString(string accountName, string accountKey) => $"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accountKey};EndpointSuffix={EndpointSuffix}";

        internal static BlobServiceClient CreateStorageAccount()
        {
            if (!UsingManagedIdentity)
                return new BlobServiceClient(StorageConnectionString);
            
            var defaultAzureCredential = UserAssignedClientId != null 
                ? new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = UserAssignedClientId }) 
                : new DefaultAzureCredential();
            return new BlobServiceClient(ServiceUri, defaultAzureCredential);
        }

        private static readonly Func<Lazy<BlobServiceClient>> Client = () =>
        {
            if (HasChanged)
            {
                LazyClient = new Lazy<BlobServiceClient>(CreateStorageAccount, LazyThreadSafetyMode.PublicationOnly);
                HasChanged = false;
            }

            return LazyClient;
        };

        public static void UseManagedIdentity(string accountName, string userAssignedClientId)
        {
            Validation.BasedOn(errors =>
            {
                if (accountName.IsNullOrEmpty())
                    errors.Add("Account name is required");

                if (userAssignedClientId.IsNullOrEmpty())
                    errors.Add("User Assigned Client Id is required");
            });
            UseManagedIdentityInternal(accountName, userAssignedClientId);
        }

        public static void UseManagedIdentity(string accountName)
        {
            Validation.BasedOn(errors =>
            {
                if (accountName.IsNullOrEmpty())
                    errors.Add("Account name is required");
            });
            UseManagedIdentityInternal(accountName, null);
        }

        private static void UseManagedIdentityInternal(string accountName, string? userAssignedClientId)
        {
            UsingManagedIdentity = true;
            UserAssignedClientId = userAssignedClientId;
            ServiceUri = new Uri($"https://{accountName}.blob.{EndpointSuffix}");
            HasChanged = true;
        }

        public static void Configure(StorageSettings storageSettings)
        {
            Name = storageSettings.AccountName;
            Configure(BuildStorageConnectionString(storageSettings.AccountName, storageSettings.AccountKey));
        }

        public static void Configure(string storageConnectionString)
        {
            StorageConnectionString = storageConnectionString;
            var search = Array.Find(storageConnectionString.Split(';'), x => x.StartsWith("AccountName="));
            Name = search != null ? search.Split('=')[1] : "Unknown Account Name";
            UsingManagedIdentity = false;
            HasChanged = true;
        }

        internal static BlobServiceClient GetClient()
        {
            if (!IsConfigured)
                throw new Exception("Please provide storage account name and account key or use managed identity.");

            return Client().Value;
        }

        private static bool IsConfigured => !StorageConnectionString.IsNullOrEmpty() || UsingManagedIdentity;
    }
}