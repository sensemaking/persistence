using Sensemaking.Monitoring;
using Microsoft.Azure.Cosmos;

namespace Sensemaking.Persistence.Cosmos
{
    public class Monitor : InstanceMonitor
    {
        private static string? EndPoint;
        private static string? AuthorisationKey;
        internal static string? DatabaseName;

        public Monitor(string endPoint, string authenticationKey, string databaseName) : base(new MonitorInfo("Cosmos Monitor", endPoint, databaseName))
        {
            EndPoint = endPoint;
            AuthorisationKey = authenticationKey;
            DatabaseName = databaseName;
        }

        private static void CheckConnectivity()
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