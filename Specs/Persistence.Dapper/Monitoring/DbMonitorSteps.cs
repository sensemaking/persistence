using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using Sensemaking.Monitoring;
using Sensemaking.Persistence.Dapper;
using Sensemaking.Bdd;

namespace Sensemaking.Specs.Persistence.Dapper.Monitoring;

public partial class DbMonitorSpecs
{
    private const string database_name = "master";
    private Availability availability;
    private IMonitor monitor;

    protected override void before_each()
    {
        base.before_each();
        monitor = null;
    }

    private void a_database()
    {
        monitor = new Db($"{Startup.Server.connection_string};Database={database_name}").Monitor;
    }

    private void an_inaccessible_database()
    {
        monitor = new Db($"{Startup.Server.connection_string};Database=ThisWillNotWork;Connect Timeout=1")
            .Monitor;
    }

    private void getting_availability()
    {
        availability = monitor.Availability();
    }

    private void monitor_info_has_database()
    {
        monitor.Info.Type.should_be("Sql Database Monitor");
        monitor.Info.Name.should_be(database_name);
    }

    private void monitor_info_has_server()
    {
        monitor.Info.Instances.Single().should_be(Startup.db_server);
    }

    private void available(bool available)
    {
        ((bool)availability).should_be(available);
    }

    private void hasAlertWith(MonitorInfo targetMonitor)
    {
        availability.Alerts.should_contain(x => x.Monitor == targetMonitor);
    }
}