using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualScript.Core.Variables
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
        /// Variables shared across the current game object.
        /// </summary>
        Object,

        /// <summary>
        /// Variables shared across the current scene.
        /// </summary>
        Scene,

        /// <summary>
        /// Variables shared across scenes.
        /// These will be reset when the application quits.
        /// </summary>
        Application,

        /// <summary>
        /// Variables that persist even after the application quits.
        /// Unity object references are not supported.
        /// </summary>
        Saved
    }
}