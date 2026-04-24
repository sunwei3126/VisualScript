using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTLogic.Core.Variables
{
    public enum VariableKind
    {
        /// <summary>
        /// Temporary variables local to the execution flow.
        /// </summary>
        Flow,

        /// <summary>
        /// Variables local to the current graph.
        /// </summary>
        Graph,

        /// <summary>
        /// Variables shared across the current host object.
        /// </summary>
        Object,

        /// <summary>
        /// Variables shared across the current host scope.
        /// </summary>
        Scene,

        /// <summary>
        /// Variables shared across host scopes for the current process.
        /// </summary>
        Application,

        /// <summary>
        /// Variables that persist beyond the current process lifetime.
        /// </summary>
        Saved
    }
}
