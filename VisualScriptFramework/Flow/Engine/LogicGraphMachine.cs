using System;
using IoTLogic.Core.Graph;
using IoTLogic.Core.Macros;
using IoTLogic.Core.Machines;

namespace IoTLogic.Flow.Engine
{
    /// <summary>
    /// Hosts a <see cref="LogicGraph"/> as an <see cref="IMachine"/> root so it can be
    /// instantiated through the standard <see cref="GraphReference"/> / <see cref="GraphInstances"/>
    /// infrastructure without a game-engine scene object.
    /// </summary>
    public sealed class LogicGraphMachine : IMachine
    {
        private readonly LogicGraph _graph;
        private GraphReference _reference;

        public LogicGraphMachine(LogicGraph graph)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        }

        // ©¤©¤ IMachine ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
        public IGraphData GraphData { get; set; }

        // ©¤©¤ IGraphRoot ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
        public GraphPointer GetReference()
        {
            if (_reference == null)
                _reference = GraphReference.New(this, ensureValid: true);
            return _reference;
        }

        // ©¤©¤ IGraphParent / IGraphNester ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
        public IGraph ChildGraph => _graph;
        public bool IsSerializationRoot => true;
        public IGraph DefaultGraph() => new LogicGraph();

        // ©¤©¤ IGraphNester ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
        public IGraphNest Nest => throw new NotSupportedException(
            "LogicGraphMachine does not support nesting.");

        public void InstantiateNest() { }
        public void UninstantiateNest() { }

        // ©¤©¤ IGraphItem ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
        public Guid Guid { get; set; } = Guid.NewGuid();

        public override string ToString() => $"Machine[{_graph.Title ?? _graph.GetType().Name}]";
    }
}
