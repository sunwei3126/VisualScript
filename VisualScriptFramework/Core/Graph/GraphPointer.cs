using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IoTLogic.Core.Machines;
using IoTLogic.Core.Macros;
using IoTLogic.Core.Utities;

namespace IoTLogic.Core.Graph
{
    public abstract class GraphPointer
    {
        #region Lifecycle

        protected static bool IsValidRoot(IGraphRoot root)
        {
            return root?.ChildGraph != null;
        }

        internal GraphPointer() { }

        protected void Initialize(IGraphRoot root)
        {
            if (!IsValidRoot(root))
            {
                throw new ArgumentException("Graph pointer root must provide a non-null child graph.", nameof(root));
            }

            if (!(root is IMachine  || root is IMacro))
            {
                throw new ArgumentException("Graph pointer root must be either a machine or a macro.", nameof(root));
            }

            this.Root = root;

            parentStack.Add(root);

            graphStack.Add(root.ChildGraph);

            dataStack.Add(Machine?.GraphData);

            debugDataStack.Add(FetchRootDebugDataBinding?.Invoke(root));

        }

        protected void Initialize(IGraphRoot root, IEnumerable<IGraphParentElement> parentElements, bool ensureValid)
        {
            Initialize(root);

            Ensure.Ensure.That(nameof(parentElements)).IsNotNull(parentElements);

            foreach (var parentElement in parentElements)
            {
                if (!TryEnterParentElement(parentElement, out var error))
                {
                    if (ensureValid)
                    {
                        throw new GraphPointerException(error, this);
                    }

                    break;
                }
            }
        }

        #endregion


        #region Conversion

        public abstract GraphReference AsReference();

        public virtual void CopyFrom(GraphPointer other)
        {
            Root = other.Root;
 
            parentStack.Clear();
            parentElementStack.Clear();
            graphStack.Clear();
            dataStack.Clear();
            debugDataStack.Clear();

            foreach (var parent in other.parentStack)
            {
                parentStack.Add(parent);
            }

            foreach (var parentElement in other.parentElementStack)
            {
                parentElementStack.Add(parentElement);
            }

            foreach (var graph in other.graphStack)
            {
                graphStack.Add(graph);
            }

            foreach (var data in other.dataStack)
            {
                dataStack.Add(data);
            }

            foreach (var debugData in other.debugDataStack)
            {
                debugDataStack.Add(debugData);
            }
        }

        #endregion


        #region Stack

        public IGraphRoot Root { get; protected set; }

        public IMachine Machine => Root as IMachine;

        public IMacro Macro => Root as IMacro;

        protected readonly List<IGraphParent> parentStack = new List<IGraphParent>();

        protected readonly List<IGraphParentElement> parentElementStack = new List<IGraphParentElement>();

        protected readonly List<IGraph> graphStack = new List<IGraph>();

        protected readonly List<IGraphData> dataStack = new List<IGraphData>();

        protected readonly List<IGraphDebugData> debugDataStack = new List<IGraphDebugData>();

        public IEnumerable<Guid> ParentElementGuids => parentElementStack.Select(parentElement => parentElement.Guid);

        #endregion


        #region Utility

        public int Depth => parentStack.Count;

        public bool IsRoot => Depth == 1;

        public bool IsChild => Depth > 1;

        public void EnsureDepthValid(int depth)
        {
            Ensure.Ensure.That(nameof(depth)).IsGte(depth, 1);

            if (depth > this.Depth)
            {
                throw new GraphPointerException($"Trying to fetch a graph pointer level above depth: {depth} > {this.Depth}", this);
            }
        }

        public void EnsureChild()
        {
            if (!IsChild)
            {
                throw new GraphPointerException("Graph pointer does not point to a child graph.", this);
            }
        }

        public bool IsWithin<T>() where T : IGraphParent
        {
            return Parent is T;
        }

        public void EnsureWithin<T>() where T : IGraphParent
        {
            if (!IsWithin<T>())
            {
                throw new GraphPointerException($"Graph pointer must be within a {typeof(T)} for this operation.", this);
            }
        }

        public IGraphParent Parent => parentStack[parentStack.Count - 1];

        public T GetParent<T>() where T : IGraphParent
        {
            EnsureWithin<T>();

            return (T)Parent;
        }

        public IGraphParentElement ParentElement
        {
            get
            {
                EnsureChild();

                return parentElementStack[parentElementStack.Count - 1];
            }
        }

        public IGraph RootGraph => graphStack[0];

        public IGraph Graph => graphStack[graphStack.Count - 1];

        protected IGraphData _data
        {
            get => dataStack[dataStack.Count - 1];
            set => dataStack[dataStack.Count - 1] = value;
        }

        public IGraphData Data
        {
            get
            {
                EnsureDataAvailable();
                return _data;
            }
        }

        protected IGraphData _parentData => dataStack[dataStack.Count - 2];

        public bool HasData => _data != null;

        public void EnsureDataAvailable()
        {
            if (!HasData)
            {
                throw new GraphPointerException($"Graph data is not available.", this);
            }
        }

        public T GetGraphData<T>() where T : IGraphData
        {
            var data = this.Data;

            if (data is T t)
            {
                return t;
            }

            throw new GraphPointerException($"Graph data type mismatch. Found {data.GetType()}, expected {typeof(T)}.", this);
        }

        public T GetElementData<T>(IGraphElementWithData element) where T : IGraphElementData
        {
            if (_data.TryGetElementData(element, out var elementData))
            {
                if (elementData is T t)
                {
                    return t;
                }

                throw new GraphPointerException($"Graph element data type mismatch. Found {elementData.GetType()}, expected {typeof(T)}.", this);
            }

            throw new GraphPointerException($"Missing graph element data for {element}.", this);
        }

        public static Func<IGraphRoot, IGraphDebugData> FetchRootDebugDataBinding { get; set; }

        public bool HasDebugData => _debugData != null;

        public void EnsureDebugDataAvailable()
        {
            if (!HasDebugData)
            {
                throw new GraphPointerException($"Graph debug data is not available.", this);
            }
        }

        protected IGraphDebugData _debugData
        {
            get => debugDataStack[debugDataStack.Count - 1];
            set => debugDataStack[debugDataStack.Count - 1] = value;
        }

        public IGraphDebugData DebugData
        {
            get
            {
                EnsureDebugDataAvailable();
                return _debugData;
            }
        }

        public T GetGraphDebugData<T>() where T : IGraphDebugData
        {
            var debugData = this.DebugData;

            if (debugData is T t)
            {
                return t;
            }

            throw new GraphPointerException($"Graph debug data type mismatch. Found {debugData.GetType()}, expected {typeof(T)}.", this);
        }

        public T GetElementDebugData<T>(IGraphElementWithDebugData element)
        {
            var elementDebugData = DebugData.GetOrCreateElementData(element);

            if (elementDebugData is T t)
            {
                return t;
            }

            throw new GraphPointerException($"Graph element runtime debug data type mismatch. Found {elementDebugData.GetType()}, expected {typeof(T)}.", this);
        }

        #endregion


        #region Traversal

        protected bool TryEnterParentElement(Guid parentElementGuid, out string error, int? maxRecursionDepth = null)
        {
            if (!Graph.Elements.TryGetValue(parentElementGuid, out var element))
            {
                error = "Trying to enter a graph parent element with a GUID that is not within the current graph.";
                return false;
            }

            if (!(element is IGraphParentElement))
            {
                error = "Provided element GUID does not point to a graph parent element.";
                return false;
            }

            var parentElement = (IGraphParentElement)element;

            return TryEnterParentElement(parentElement, out error, maxRecursionDepth);
        }

        protected bool TryEnterParentElement(IGraphParentElement parentElement, out string error, int? maxRecursionDepth = null, bool skipContainsCheck = false)
        {
            // The contains check is expensive because variant+merged collection checks
            // If we already know for sure this error cannot happen, skipping it provides a significant optim
            if (!skipContainsCheck && !Graph.Elements.Contains(parentElement))
            {
                error = "Trying to enter a graph parent element that is not within the current graph.";
                return false;
            }

            var childGraph = parentElement.ChildGraph;

            if (childGraph == null)
            {
                error = "Trying to enter a graph parent element without a child graph.";
                return false;
            }

            if (Recursion.SafeMode)
            {
                var recursionDepth = 0;
                var _maxRecursionDepth = maxRecursionDepth ?? Recursion.DefaultMaxDepth;

                foreach (var parentGraph in graphStack)
                {
                    if (parentGraph == childGraph)
                    {
                        recursionDepth++;
                    }
                }

                if (recursionDepth > _maxRecursionDepth)
                {
                    error = $"Max recursion depth of {_maxRecursionDepth} has been exceeded. Are you nesting a graph within itself?\nIf not, consider increasing '{nameof(Recursion)}.{nameof(Recursion.DefaultMaxDepth)}'.";
                    return false;
                }
            }

            EnterValidParentElement(parentElement);
            error = null;
            return true;
        }

        protected void EnterParentElement(IGraphParentElement parentElement)
        {
            if (!TryEnterParentElement(parentElement, out var error))
            {
                throw new GraphPointerException(error, this);
            }
        }

        protected void EnterParentElement(Guid parentElementGuid)
        {
            if (!TryEnterParentElement(parentElementGuid, out var error))
            {
                throw new GraphPointerException(error, this);
            }
        }

        private void EnterValidParentElement(IGraphParentElement parentElement)
        {
            var childGraph = parentElement.ChildGraph;

            parentStack.Add(parentElement);
            parentElementStack.Add(parentElement);
            graphStack.Add(childGraph);

            IGraphData childGraphData = null;
            _data?.TryGetChildGraphData(parentElement, out childGraphData);
            dataStack.Add(childGraphData);

            var childGraphDebugData = _debugData?.GetOrCreateChildGraphData(parentElement);
            debugDataStack.Add(childGraphDebugData);
        }

        protected void ExitParentElement()
        {
            if (!IsChild)
            {
                throw new GraphPointerException("Trying to exit the root graph.", this);
            }

            parentStack.RemoveAt(parentStack.Count - 1);
            parentElementStack.RemoveAt(parentElementStack.Count - 1);
            graphStack.RemoveAt(graphStack.Count - 1);
            dataStack.RemoveAt(dataStack.Count - 1);
            debugDataStack.RemoveAt(debugDataStack.Count - 1);
        }

        #endregion


        #region Validation

        public bool IsValid
        {
            get
            {
                try
                {
 
                    if (RootGraph != Root.ChildGraph)
                    {
                        // Root graph has changed
                        return false;
                    }

                    for (var depth = 1; depth < this.Depth; depth++)
                    {
                        var parentElement = parentElementStack[depth - 1];
                        var parentGraph = graphStack[depth - 1];
                        var childGraph = graphStack[depth];

                        // Important to check by object and not by GUID here,
                        // because object stack integrity has to be guaranteed
                        // (GUID integrity is implied because they're immutable)
                        if (!parentGraph.Elements.Contains(parentElement))
                        {
                            // Parent graph no longer contains the parent element
                            return false;
                        }

                        if (parentElement.ChildGraph != childGraph)
                        {
                            // Child graph has changed
                            return false;
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to check graph pointer validity: \n" + ex);
                    return false;
                }
            }
        }

        public void EnsureValid()
        {
            if (!IsValid)
            {
                throw new GraphPointerException("Graph pointer is invalid.", this);
            }
        }

        #endregion


        #region Equality

        public bool InstanceEquals(GraphPointer other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (!DefinitionEquals(other))
            {
                return false;
            }

            var depth = this.Depth; // Micro optimization

            for (int d = 0; d < depth; d++)
            {
                var data = dataStack[d];
                var otherData = other.dataStack[d];

                if (data != otherData)
                {
                    return false;
                }
            }

            return true;
        }

        public bool DefinitionEquals(GraphPointer other)
        {
            if (other == null)
            {
                return false;
            }

            if (RootGraph != other.RootGraph)
            {
                return false;
            }

            var depth = this.Depth; // Micro optimization

            if (depth != other.Depth)
            {
                return false;
            }

            for (int d = 1; d < depth; d++)
            {
                var parentElement = parentElementStack[d - 1];
                var otherParentElement = other.parentElementStack[d - 1];

                if (parentElement != otherParentElement)
                {
                    return false;
                }
            }

            return true;
        }

        public int ComputeHashCode()
        {
            var hashCode = 17;

            hashCode = hashCode * 23 + (RootGraph?.GetHashCode() ?? 0);

            var depth = this.Depth; // Micro optimization

            for (int d = 1; d < depth; d++)
            {
                var parentElementGuid = parentElementStack[d - 1].Guid;

                hashCode = hashCode * 23 + parentElementGuid.GetHashCode();
            }

            return hashCode;
        }

        #endregion


        #region Breadcrumbs

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("[ ");

            for (var depth = 1; depth < this.Depth; depth++)
            {
                sb.Append(" > ");

                var parentElementIndex = depth - 1;

                if (parentElementIndex >= parentElementStack.Count)
                {
                    sb.Append("?");
                    break;
                }

                var parentElement = parentElementStack[parentElementIndex];

                sb.Append(parentElement);
            }

            sb.Append(" ]");

            return sb.ToString();
        }

        #endregion
    }
}
