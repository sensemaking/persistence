using Sensemaking.Monitoring;
using Fdb.Rx.Test;
using NUnit.Framework;
using Sensemaking.Bdd;

namespace Fdb.Rx.Testing.Persistence.Dapper.Monitoring;

[TestFixture]
public partial class DbMonitorSpecs : Specification
{
    [Test]
    public void has_database_and_server_for_monitoring_info()
    {
        Given(a_database);
        Then(monitor_info_has_database);
        And(monitor_info_has_server);
    }

    [Test]
    public void is_available_when_database_can_be_accessed()
    {
        Given(a_database);
        When(getting_availability);
        Then(() => available(true));
    }

    [Test]
    public void is_unavailable_when_database_cannot_be_accessed()
    {
        Given(an_inaccessible_database);
        When(getting_availability);
        Then(() => available(false));
        And(() => hasAlertWith(monitor.Info));
    }
}