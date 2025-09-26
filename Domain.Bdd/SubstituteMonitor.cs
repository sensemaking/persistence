using Sensemaking.Monitoring;

namespace Sensemaking.Bdd.Domain
{
    internal class Monitor : InstanceMonitor
    {
        public Monitor() : base(new MonitorInfo("Substitute Monitor", "Substitute Monitor"))
        {
        }

        public override Availability Availability()
        {
            return Sensemaking.Monitoring.Availability.Up();
        }
    }
}
