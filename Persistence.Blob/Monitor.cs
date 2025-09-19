using Sensemaking.Monitoring;

namespace Fdb.Rx.Persistence.Blob
{
    public class Monitor : InstanceMonitor
    {
        public Monitor(StorageSettings settings) : base(new MonitorInfo("Blob Storage Monitor", settings.AccountName))
        {
        }

        public Monitor(string accountName) : base(new MonitorInfo("Blob Storage Monitor", accountName))
        {
        }

        private static void CheckConnectivity()
        {
            var blobServiceClient = Account.CreateStorageAccount();
            blobServiceClient.GetBlobContainers();
        }

        public override Availability Availability()
        {
            try
            {
                CheckConnectivity();
                return Sensemaking.Monitoring.Availability.Up();
            }
            catch
            {
                return Sensemaking.Monitoring.Availability.Down(AlertFactory.ServiceUnavailable(Info,
                    "Azure Storage is unavailable or not configured."));
            }
        }
    }
}