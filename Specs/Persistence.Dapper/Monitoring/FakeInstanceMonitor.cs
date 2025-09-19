 using System;
 using Sensemaking.Monitoring;

namespace Fdb.Rx.Testing.Persistence.Dapper.Monitoring;

public class FakeInstanceMonitor : InstanceMonitor
{
    public static MonitorInfo FakeInfo = new MonitorInfo("Fake", "FakeSystem", "FakeInstance");

    public FakeInstanceMonitor() : base(FakeInfo)
    {
        IsAvailable = true;
    }

    public FakeInstanceMonitor(string alertMessage) : base(FakeInfo)
    {
        IsAvailable = false;
        Alert = AlertFactory.ServiceUnavailable(FakeInfo, alertMessage);
    }

    public override Availability Availability()
    {
        return IsAvailable ? Sensemaking.Monitoring.Availability.Up() : Sensemaking.Monitoring.Availability.Down(Alert);
    }

    private bool IsAvailable { get; set; }
    private MonitoringAlert Alert { get; set; }
}