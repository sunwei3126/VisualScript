using System;
using System.Collections.Generic;
using VisualScript.Core.Graph;

namespace VisualScript.Core.Variables
{
    public interface IGraphWithVariables : IGraph
    {
        VariableDeclarations Variables { get; }

        IEnumerable<string> GetDynamicVariableNames(VariableKind kind, GraphReference reference);
    }
}
