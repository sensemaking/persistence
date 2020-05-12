using System.Data;
using System.Data.SqlClient;
using Sensemaking.Monitoring;

namespace Sensemaking.Persistence.Dapper
{
    using System;

    public class DbMonitor : InstanceMonitor
    {
        private Db Db { get; }

        public DbMonitor(Db db) : base(db.GetMonitorInfo())
        {
            Db = db;
        }

        public override Availability Availability()
        {
            try
            {
                Db.Execute("SELECT 1", commandType: CommandType.Text);
                return Monitoring.Availability.Up();
            }
            catch (Exception ex)
            {
                return Monitoring.Availability.Down(AlertFactory.ServiceUnavailable(Info, $"Database is down: {ex.Message}"));
            }
        }
    }

    internal static class DbExtensions
    {
        internal static MonitorInfo GetMonitorInfo(this Db db)
        {
            var connection = new SqlConnectionStringBuilder(db.ConnectionString);
            return new MonitorInfo("Database Monitor", "Wibble", connection.DataSource);
        }
    }
}
