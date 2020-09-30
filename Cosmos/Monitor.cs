using Microsoft.Azure.Cosmos;
using Sensemaking.Monitoring;

namespace Sensemaking.Cosmos
{
    public class Monitor : InstanceMonitor
    {
        private readonly string EndPoint;
        private readonly string AuthorisationKey;
        internal readonly string DatabaseName;

        public Monitor(string endPoint, string authenticationKey, string databaseName) : base(new MonitorInfo("Cosmos Monitor", endPoint, databaseName))
        {
            EndPoint = endPoint;
            AuthorisationKey = authenticationKey;
            DatabaseName = databaseName;
        }

        private void CheckConnectivity()
        {
            using (var cosmosClient = new CosmosClient(EndPoint, AuthorisationKey))
                cosmosClient.GetDatabase(DatabaseName).ReadAsync().Wait();
        }

        public override Availability Availability()
        {
            try
            {
                CheckConnectivity();
                return Monitoring.Availability.Up();
            }
            catch
            {
                return Monitoring.Availability.Down(AlertFactory.ServiceUnavailable(Info, "Cosmos is unavailable or not configured."));
            }
        }
    }
}