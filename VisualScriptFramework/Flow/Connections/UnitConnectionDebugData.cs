using System;

namespace VisualScript.Flow.Connections
{
    public class UnitConnectionDebugData : IUnitConnectionDebugData
    {
        public int LastInvokeFrame { get; set; }

        public float LastInvokeTime { get; set; }

        public Exception RuntimeException { get; set; }
    }
}
