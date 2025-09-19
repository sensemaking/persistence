using System;
using Sensemaking.Monitoring;
using Microsoft.Azure.Cosmos;

namespace Sensemaking.Persistence.Cosmos
{
    public class CosmosMonitor(IConnectToCosmos connection, string databaseName) : InstanceMonitor(new MonitorInfo("Cosmos Monitor", databaseName))
    {
        private readonly CosmosClient Client = connection.Client();
        private readonly string DatabaseName = databaseName;

        private void CheckConnectivity()
        {
            Client.GetDatabase(DatabaseName).ReadAsync().Await();
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
                    "Cosmos is unavailable or not configured."));
            }
        }
    }
}