using System;

namespace IoTLogic.Flow.Connections
{
    public class UnitConnectionDebugData : IUnitConnectionDebugData
    {
        public int LastInvokeFrame { get; set; }

        public float LastInvokeTime { get; set; }

        public Exception RuntimeException { get; set; }
    }
}
