using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualScript.Core.EditorBinding;
using VisualScript.Core.Ensure;
using VisualScript.Core.Graph;
using VisualScript.Core.Pooling;
using VisualScript.Core.Reflection;
using VisualScript.Core.Utities;
using VisualScript.Core.Variables;
using VisualScript.Flow.Connections;
using VisualScript.Flow.Ports;
using VVisualScript.Flow.Ports;

namespace VisualScript.Flow
{
    public sealed class Flow : IPoolable, IDisposable
    {
        // We need to check for recursion by passing some additional
        // context information to avoid the same port in multiple different
        // nested flow graphs to count as the same item. Naively,
        // we're using the parent as the context, which seems to work;
        // it won't theoretically catch recursive nesting, but then recursive
        // nesting already isn't supported anyway, so this way we avoid hashing
        // or turning the stack into a reference.
        // https://support.ludiq.io/communities/5/topics/2122-r
        // We make this an equatable struct to avoid any allocation.
        private struct RecursionNode : IEquatable<RecursionNode>
        {
            public IUnitPort port { get; }

            public IGraphParent context { get; }

            public RecursionNode(IUnitPort port, GraphPointer pointer)
            {
                this.port = port;
                this.context = pointer.Parent;
            }

            public bool Equals(RecursionNode other)
            {
                return other.port == port && other.context == context;
            }

            public override bool Equals(object obj)
            {
                return obj is RecursionNode other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashUtility.GetHashCode(port, context);
            }
        }

        public GraphStack stack { get; private set; }

        private Recursion<RecursionNode> recursion;

        private readonly Dictionary<IUnitValuePort, object> locals = new Dictionary<IUnitValuePort, object>();

        public readonly VariableDeclarations variables = new VariableDeclarations();

        private readonly Stack<int> loops = new Stack<int>();

        private readonly HashSet<GraphStack> preservedStacks = new HashSet<GraphStack>();

        public bool isPrediction { get; private set; }

        private bool disposed;

        public bool enableDebug
        {
            get
            {
                if (isPrediction)
                {
                    return false;
                }

                if (!stack.HasDebugData)
                {
                    return false;
                }

                return true;
            }
        }

        public static Func<GraphPointer, bool> isInspectedBinding { get; set; }

        public bool isInspected => isInspectedBinding?.Invoke(stack) ?? false;


        #region Lifecycle

        private Flow() { }

        public static Flow New(GraphReference reference)
        {
            Ensure.That(nameof(reference)).IsNotNull(reference);

            var flow = GenericPool<Flow>.New(() => new Flow()); ;
            flow.stack = reference.ToStackPooled();

            return flow;
        }

        void IPoolable.New()
        {
            disposed = false;
            recursion = Recursion<RecursionNode>.New();
        }

        public void Dispose()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(ToString());
            }

            GenericPool<Flow>.Free(this);
        }

        void IPoolable.Free()
        {
            stack?.Dispose();
            recursion?.Dispose();
            locals.Clear();
            loops.Clear();
            variables.Clear();

            // Preserved stacks could remain if coroutine was interrupted
            foreach (var preservedStack in preservedStacks)
            {
                preservedStack.Dispose();
            }

            preservedStacks.Clear();

            loopIdentifier = -1;
            stack = null;
            recursion = null;
         
            isPrediction = false;
            disposed = true;
        }

        public GraphStack PreserveStack()
        {
            var preservedStack = stack.Clone();
            preservedStacks.Add(preservedStack);
            return preservedStack;
        }

        public void RestoreStack(GraphStack stack)
        {
            this.stack.CopyFrom(stack);
        }

        public void DisposePreservedStack(GraphStack stack)
        {
            stack.Dispose();
            preservedStacks.Remove(stack);
        }

        #endregion


        #region Loops

        public int loopIdentifier = -1;

        public int currentLoop
        {
            get
            {
                if (loops.Count > 0)
                {
                    return loops.Peek();
                }
                else
                {
                    return -1;
                }
            }
        }

        public bool LoopIsNotBroken(int loop)
        {
            return currentLoop == loop;
        }

        public int EnterLoop()
        {
            var loop = ++loopIdentifier;

            loops.Push(loop);

            return loop;
        }

        public void BreakLoop()
        {
            if (currentLoop < 0)
            {
                throw new InvalidOperationException("No active loop to break.");
            }

            loops.Pop();
        }

        public void ExitLoop(int loop)
        {
            if (loop != currentLoop)
            {
                // Already exited through break
                return;
            }

            loops.Pop();
        }

        #endregion


        #region Control

        public void Run(ControlOutput port)
        {
            Invoke(port);
            Dispose();
        }

        public void Invoke(ControlOutput output)
        {
            Ensure.That(nameof(output)).IsNotNull(output);

            var connection = output.Connection;

            if (connection == null)
            {
                return;
            }

            var input = connection.Destination;

            var recursionNode = new RecursionNode(output, stack);

            BeforeInvoke(output, recursionNode);

            try
            {
                var nextPort = InvokeDelegate(input);

                if (nextPort != null)
                {
                    Invoke(nextPort);
                }
            }
            finally
            {
                AfterInvoke(output, recursionNode);
            }
        }

        private IEnumerable InvokeCoroutine(ControlOutput output)
        {
            var connection = output.Connection;

            if (connection == null)
            {
                yield break;
            }

            var input = connection.Destination;

            var recursionNode = new RecursionNode(output, stack);

            BeforeInvoke(output, recursionNode);

            if (input.supportsCoroutine)
            {
                foreach (var instruction in InvokeCoroutineDelegate(input))
                {
                    if (instruction is ControlOutput)
                    {
                        foreach (var unwrappedInstruction in InvokeCoroutine((ControlOutput)instruction))
                        {
                            yield return unwrappedInstruction;
                        }
                    }
                    else
                    {
                        yield return instruction;
                    }
                }
            }
            else
            {
                ControlOutput nextPort = InvokeDelegate(input);

                if (nextPort != null)
                {
                    foreach (var instruction in InvokeCoroutine(nextPort))
                    {
                        yield return instruction;
                    }
                }
            }

            AfterInvoke(output, recursionNode);
        }

        private RecursionNode BeforeInvoke(ControlOutput output, RecursionNode recursionNode)
        {
            try
            {
                recursion?.Enter(recursionNode);
            }
            catch (StackOverflowException ex)
            {
                output.Unit.HandleException(stack, ex);
                throw;
            }

            var connection = output.Connection;
            var input = connection.Destination;

            if (enableDebug)
            {
                var connectionEditorData = stack.GetElementDebugData<IUnitConnectionDebugData>(connection);
                var inputUnitEditorData = stack.GetElementDebugData<IUnitDebugData>(input.Unit);

                connectionEditorData.LastInvokeFrame = EditorTimeBinding.frame;
                connectionEditorData.LastInvokeTime = EditorTimeBinding.time;
                inputUnitEditorData.LastInvokeFrame = EditorTimeBinding.frame;
                inputUnitEditorData.LastInvokeTime = EditorTimeBinding.time;
            }

            return recursionNode;
        }

        private void AfterInvoke(ControlOutput output, RecursionNode recursionNode)
        {
            recursion?.Exit(recursionNode);
        }

        private ControlOutput InvokeDelegate(ControlInput input)
        {
            try
            {
                if (input.requiresCoroutine)
                {
                    throw new InvalidOperationException($"Port '{input.Key}' on '{input.Unit}' can only be triggered in a coroutine.");
                }

                return input.action(this);
            }
            catch (Exception ex)
            {
                input.Unit.HandleException(stack, ex);
                throw;
            }
        }

        private IEnumerable InvokeCoroutineDelegate(ControlInput input)
        {
            var instructions = input.coroutineAction(this);

            while (true)
            {
                object instruction;

                try
                {
                    if (!instructions.MoveNext())
                    {
                        break;
                    }

                    instruction = instructions.Current;
                }
                catch (Exception ex)
                {
                    input.Unit.HandleException(stack, ex);
                    throw;
                }

                yield return instruction;
            }
        }

        #endregion


        #region Values

        public bool IsLocal(IUnitValuePort port)
        {
            Ensure.That(nameof(port)).IsNotNull(port);

            return locals.ContainsKey(port);
        }

        public void SetValue(IUnitValuePort port, object value)
        {
            Ensure.That(nameof(port)).IsNotNull(port);
            Ensure.That(nameof(value)).IsOfType(value, port.Type);

            if (locals.ContainsKey(port))
            {
                locals[port] = value;
            }
            else
            {
                locals.Add(port, value);
            }
        }

        public object GetValue(ValueInput input)
        {
            if (locals.TryGetValue(input, out var local))
            {
                return local;
            }

            var connection = input.Connection;

            if (connection != null)
            {
                if (enableDebug)
                {
                    var connectionEditorData = stack.GetElementDebugData<IUnitConnectionDebugData>(connection);

                    connectionEditorData.LastInvokeFrame = EditorTimeBinding.frame;
                    connectionEditorData.LastInvokeTime = EditorTimeBinding.time;
                }

                var output = connection.Source;

                var value = GetValue(output);

                if (enableDebug)
                {
                    var connectionEditorData = stack.GetElementDebugData<ValueConnection.DebugData>(connection);

                    connectionEditorData.lastValue = value;
                    connectionEditorData.assignedLastValue = true;
                }

                return value;
            }
            else if (TryGetDefaultValue(input, out var defaultValue))
            {
                return defaultValue;
            }
            else
            {
                throw new MissingValuePortInputException(input.Key);
            }
        }

        private object GetValue(ValueOutput output)
        {
            if (locals.TryGetValue(output, out var local))
            {
                return local;
            }

            if (!output.supportsFetch)
            {
                throw new InvalidOperationException($"The value of '{output.Key}' on '{output.Unit}' cannot be fetched dynamically, it must be assigned.");
            }

            var recursionNode = new RecursionNode(output, stack);

            try
            {
                recursion?.Enter(recursionNode);
            }
            catch (StackOverflowException ex)
            {
                output.Unit.HandleException(stack, ex);
                throw;
            }

            try
            {
                if (enableDebug)
                {
                    var outputUnitEditorData = stack.GetElementDebugData<IUnitDebugData>(output.Unit);

                    outputUnitEditorData.LastInvokeFrame = EditorTimeBinding.frame;
                    outputUnitEditorData.LastInvokeTime = EditorTimeBinding.time;
                }

                var value = GetValueDelegate(output);

                return value;
            }
            finally
            {
                recursion?.Exit(recursionNode);
            }
        }

        public object GetValue(ValueInput input, Type type)
        {
            return ConversionUtility.Convert(GetValue(input), type);
        }

        public T GetValue<T>(ValueInput input)
        {
            return (T)GetValue(input, typeof(T));
        }

        public object GetConvertedValue(ValueInput input)
        {
            return GetValue(input, input.Type);
        }

        private object GetDefaultValue(ValueInput input)
        {
            if (!TryGetDefaultValue(input, out var defaultValue))
            {
                throw new InvalidOperationException("Value input port does not have a default value.");
            }

            return defaultValue;
        }

        public bool TryGetDefaultValue(ValueInput input, out object defaultValue)
        {
            if (!input.Unit.DefaultValues.TryGetValue(input.Key, out defaultValue))
            {
                return false;
            }
            return true;
        }

        private object GetValueDelegate(ValueOutput output)
        {
            try
            {
                return output.getValue(this);
            }
            catch (Exception ex)
            {
                output.Unit.HandleException(stack, ex);
                throw;
            }
        }

        public static object FetchValue(ValueInput input, GraphReference reference)
        {
            var flow = New(reference);

            var result = flow.GetValue(input);

            flow.Dispose();

            return result;
        }

        public static object FetchValue(ValueInput input, Type type, GraphReference reference)
        {
            return ConversionUtility.Convert(FetchValue(input, reference), type);
        }

        public static T FetchValue<T>(ValueInput input, GraphReference reference)
        {
            return (T)FetchValue(input, typeof(T), reference);
        }

        #endregion


        #region Value Prediction

        public static bool CanPredict(IUnitValuePort port, GraphReference reference)
        {
            Ensure.That(nameof(port)).IsNotNull(port);

            var flow = New(reference);

            flow.isPrediction = true;

            bool canPredict;

            if (port is ValueInput)
            {
                canPredict = flow.CanPredict((ValueInput)port);
            }
            else if (port is ValueOutput)
            {
                canPredict = flow.CanPredict((ValueOutput)port);
            }
            else
            {
                throw new NotSupportedException();
            }

            flow.Dispose();

            return canPredict;
        }

        private bool CanPredict(ValueInput input)
        {
            if (!input.HasValidConnection)
            {
                if (!TryGetDefaultValue(input, out var defaultValue))
                {
                    return false;
                }

                if (typeof(Component).IsAssignableFrom(input.Type))
                {
                    defaultValue = defaultValue?.ConvertTo(input.Type);
                }

                if (!input.allowsNull && defaultValue == null)
                {
                    return false;
                }

                return true;
            }

            var output = input.ValidConnectedPorts.Single();

            if (!CanPredict(output))
            {
                return false;
            }

            var connectedValue = GetValue(output);

            if (!ConversionUtility.CanConvert(connectedValue, input.Type, false))
            {
                return false;
            }

            if (typeof(Component).IsAssignableFrom(input.Type))
            {
                connectedValue = connectedValue?.ConvertTo(input.Type);
            }

            if (!input.allowsNull && connectedValue == null)
            {
                return false;
            }

            return true;
        }

        private bool CanPredict(ValueOutput output)
        {
            // Shortcircuit the expensive check if the port isn't marked as predictable
            if (!output.supportsPrediction)
            {
                return false;
            }

            var recursionNode = new RecursionNode(output, stack);

            if (!recursion?.TryEnter(recursionNode) ?? false)
            {
                return false;
            }

            // Check each value dependency
            foreach (var relation in output.Unit.Relations.WithDestination(output))
            {
                if (relation.Source is ValueInput)
                {
                    var source = (ValueInput)relation.Source;

                    if (!CanPredict(source))
                    {
                        recursion?.Exit(recursionNode);
                        return false;
                    }
                }
            }

            var value = CanPredictDelegate(output);
            recursion?.Exit(recursionNode);
            return value;
        }

        private bool CanPredictDelegate(ValueOutput output)
        {
            try
            {
                return output.canPredictValue(this);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Prediction check failed for '{output.Key}' on '{output.Unit}':\n{ex}");

                return false;
            }
        }

        public static object Predict(IUnitValuePort port, GraphReference reference)
        {
            Ensure.That(nameof(port)).IsNotNull(port);

            var flow = New(reference);

            flow.isPrediction = true;

            object value;

            if (port is ValueInput)
            {
                value = flow.GetValue((ValueInput)port);
            }
            else if (port is ValueOutput)
            {
                value = flow.GetValue((ValueOutput)port);
            }
            else
            {
                throw new NotSupportedException();
            }

            flow.Dispose();

            return value;
        }

        public static object Predict(IUnitValuePort port, GraphReference reference, Type type)
        {
            return ConversionUtility.Convert(Predict(port, reference), type);
        }

        public static T Predict<T>(IUnitValuePort port, GraphReference pointer)
        {
            return (T)Predict(port, pointer, typeof(T));
        }

        #endregion
    }
}
