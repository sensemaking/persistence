﻿using System;
using System.Data;
using System.Data.SqlClient;
using Sensemaking.Monitoring;

namespace Sensemaking.Dapper
{
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
                Db.ExecuteAsync("SELECT 1", commandType: CommandType.Text).GetAwaiter().GetResult();
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
            return new MonitorInfo("Sql Monitor", connection.InitialCatalog, connection.DataSource);
        }
    }
}
