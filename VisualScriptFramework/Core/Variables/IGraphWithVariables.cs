using System;
using System.Collections.Generic;
using IoTLogic.Core.Graph;

namespace IoTLogic.Core.Variables
{
    public interface IGraphWithVariables : IGraph
    {
        VariableDeclarations Variables { get; }

        IEnumerable<string> GetDynamicVariableNames(VariableKind kind, GraphReference reference);
    }
}
