using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace IoTLogic.Core.Ensure
{
    public static class Ensure
    {
        private static readonly EnsureThat instance = new EnsureThat();

        public static bool IsActive { get; set; }

        public static void Off() => IsActive = false;

        public static void On() => IsActive = true;

        public static EnsureThat That(string paramName)
        {
            instance.paramName = paramName;
            return instance;
        }

        internal static void OnRuntimeMethodLoad()
        {
            IsActive = true;
        }
    }
}
