using System;

namespace Fdb.Rx.Domain
{
    [Flags]
    public enum Transitions
    {
        Change = 1,
        MakeReadyForQc = 2,
        Suspend = 4,
        Retire = 8,
        Qc = 16,
        Reactivate = 32,
    }
}