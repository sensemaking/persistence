using System.Data;
using Microsoft.Data.SqlClient;
using Sensemaking.Monitoring;

namespace Fdb.Rx.Persistence.Dapper
{
    using System;

    public class DbMonitor(DbReader db) : InstanceMonitor(db.GetMonitorInfo())
    {
        private DbReader Db { get; } = db;

        public override Availability Availability()
        {
            try
            {
                Db.Query<byte>("SELECT 1", commandType: CommandType.Text);
                return Sensemaking.Monitoring.Availability.Up();
            }
            catch (Exception ex)
            {
                return Sensemaking.Monitoring.Availability.Down(AlertFactory.ServiceUnavailable(Info, $"Database is down: {ex.Message}"));
            }
        }
    }

    internal static class DbExtensions
    {
        internal static MonitorInfo GetMonitorInfo(this DbReader db)
        {
            var connection = new SqlConnectionStringBuilder(db.ConnectionString);
            return new MonitorInfo("Sql Database Monitor", connection.InitialCatalog, connection.DataSource);
        }
    }
}
