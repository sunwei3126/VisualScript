using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IoTLogic.Core.Connections;
using IoTLogic.Core.Events;
using IoTLogic.Core.Graph;
using IoTLogic.Core.Utities;
using IoTLogic.Flow.Connections;
using IoTLogic.Flow.Ports;

namespace IoTLogic.Flow
{
    //[SerializationVersion("A")]
    public abstract class LogicNode : GraphElement<LogicGraph>, ILogicNode
    {
        public class DebugData : IUnitDebugData
        {
            public int LastInvokeFrame { get; set; }
            public float LastInvokeTime { get; set; }
            public Exception RuntimeException { get; set; }
        }

        protected LogicNode() : base()
        {
            ControlInputs = new UnitPortCollection<ControlInput>(this);
            ControlOutputs = new UnitPortCollection<ControlOutput>(this);
            ValueInputs = new UnitPortCollection<ValueInput>(this);
            ValueOutputs = new UnitPortCollection<ValueOutput>(this);
            InvalidInputs = new UnitPortCollection<InvalidInput>(this);
            InvalidOutputs = new UnitPortCollection<InvalidOutput>(this);

            Relations = new ConnectionCollection<IUnitRelation, IUnitPort, IUnitPort>();

            DefaultValues = new Dictionary<string, object>();
        }

        public virtual IGraphElementDebugData CreateDebugData()
        {
            return new DebugData();
        }

        public override void AfterAdd()
        {
            // Important to define before notifying instances
            Define();

            base.AfterAdd();
        }

        public override void BeforeRemove()
        {
            base.BeforeRemove();

            Disconnect();
        }

        public override void Instantiate(GraphReference instance)
        {
            base.Instantiate(instance);

            if (this is IGraphEventListener listener && XGraphEventListener.IsHierarchyListening(instance))
            {
                listener.StartListening(instance);
            }
        }

        public override void Uninstantiate(GraphReference instance)
        {
            if (this is IGraphEventListener listener)
            {
                listener.StopListening(instance);
            }

            base.Uninstantiate(instance);
        }

        #region Poutine

        protected void CopyFrom(LogicNode source)
        {
            base.CopyFrom(source);

            DefaultValues = source.DefaultValues;
        }

        #endregion

        #region Definition

        //[DoNotSerialize]
        public virtual bool CanDefine => true;

        //[DoNotSerialize]
        public bool FailedToDefine => DefinitionException != null;

        //[DoNotSerialize]
        public bool IsDefined { get; private set; }

        protected abstract void Definition();

        protected virtual void AfterDefine() { }

        protected virtual void BeforeUndefine() { }

        private void Undefine()
        {
            // Because a node is always undefined on definition,
            // even if it wasn't defined before, we make sure the user
            // code for undefinition can safely presume it was defined.
            if (IsDefined)
            {
                BeforeUndefine();
            }

            Disconnect();
            DefaultValues.Clear();
            ControlInputs.Clear();
            ControlOutputs.Clear();
            ValueInputs.Clear();
            ValueOutputs.Clear();
            InvalidInputs.Clear();
            InvalidOutputs.Clear();
            Relations.Clear();
            IsDefined = false;
        }

        public void EnsureDefined()
        {
            if (!IsDefined)
            {
                Define();
            }
        }

        public void Define()
        {
            var preservation = UnitPreservation.Preserve(this);

            // A node needs to undefine even if it wasn't defined,
            // because there might be invalid ports and connections
            // that we need to clear to avoid duplicates on definition.
            Undefine();

            if (CanDefine)
            {
                try
                {
                    Definition();
                    IsDefined = true;
                    DefinitionException = null;
                    AfterDefine();
                }
                catch (Exception ex)
                {
                    Undefine();
                    DefinitionException = ex;
                    Console.WriteLine($"Failed to define {this}:\n{ex}");
                }
            }

            preservation.RestoreTo(this);
        }

        public void RemoveUnconnectedInvalidPorts()
        {
            foreach (var unconnectedInvalidInput in InvalidInputs.Where(p => !p.HasAnyConnection).ToArray())
            {
                InvalidInputs.Remove(unconnectedInvalidInput);
            }

            foreach (var unconnectedInvalidOutput in InvalidOutputs.Where(p => !p.HasAnyConnection).ToArray())
            {
                InvalidOutputs.Remove(unconnectedInvalidOutput);
            }
        }

        #endregion

        #region Ports

        //[DoNotSerialize]
        public IUnitPortCollection<ControlInput> ControlInputs { get; }

       // [DoNotSerialize]
        public IUnitPortCollection<ControlOutput> ControlOutputs { get; }

        //[DoNotSerialize]
        public IUnitPortCollection<ValueInput> ValueInputs { get; }

       //[DoNotSerialize]
        public IUnitPortCollection<ValueOutput> ValueOutputs { get; }

       //[DoNotSerialize]
        public IUnitPortCollection<InvalidInput> InvalidInputs { get; }

        //[DoNotSerialize]
        public IUnitPortCollection<InvalidOutput> InvalidOutputs { get; }

        //[DoNotSerialize]
        public IEnumerable<IUnitInputPort> Inputs => LinqUtility.Concat<IUnitInputPort>(ControlInputs, ValueInputs, InvalidInputs);

       // [DoNotSerialize]
        public IEnumerable<IUnitOutputPort> Outputs => LinqUtility.Concat<IUnitOutputPort>(ControlOutputs, ValueOutputs, InvalidOutputs);

        //[DoNotSerialize]
        public IEnumerable<IUnitInputPort> ValidInputs => LinqUtility.Concat<IUnitInputPort>(ControlInputs, ValueInputs);

        //[DoNotSerialize]
        public IEnumerable<IUnitOutputPort> ValidOutputs => LinqUtility.Concat<IUnitOutputPort>(ControlOutputs, ValueOutputs);

       // [DoNotSerialize]
        public IEnumerable<IUnitPort> Ports => LinqUtility.Concat<IUnitPort>(Inputs, Outputs);

       // [DoNotSerialize]
        public IEnumerable<IUnitPort> InvalidPorts => LinqUtility.Concat<IUnitPort>(InvalidInputs, InvalidOutputs);

       // [DoNotSerialize]
        public IEnumerable<IUnitPort> ValidPorts => LinqUtility.Concat<IUnitPort>(ValidInputs, ValidOutputs);

        public event Action OnPortsChanged;

        public void PortsChanged()
        {
            OnPortsChanged?.Invoke();
        }

        #endregion

        #region Default Values

        //[Serialize]
        public Dictionary<string, object> DefaultValues { get; private set; }

        #endregion

        #region Connections

        //[DoNotSerialize]
        public IConnectionCollection<IUnitRelation, IUnitPort, IUnitPort> Relations { get; private set; }

        //[DoNotSerialize]
        public IEnumerable<IUnitConnection> Connections => Ports.SelectMany(p => p.Connections);

        public void Disconnect()
        {
            // Can't use a foreach because invalid ports may get removed as they disconnect
            while (Ports.Any(p => p.HasAnyConnection))
            {
                Ports.First(p => p.HasAnyConnection).Disconnect();
            }
        }

        #endregion

        #region Analysis

        //[DoNotSerialize]
        public virtual bool IsControlRoot { get; protected set; } = false;

        #endregion

        #region Helpers

        protected void EnsureUniqueInput(string key)
        {
            if (ControlInputs.Contains(key) || ValueInputs.Contains(key) || InvalidInputs.Contains(key))
            {
                throw new ArgumentException($"Duplicate input for '{key}' in {GetType()}.");
            }
        }

        protected void EnsureUniqueOutput(string key)
        {
            if (ControlOutputs.Contains(key) || ValueOutputs.Contains(key) || InvalidOutputs.Contains(key))
            {
                throw new ArgumentException($"Duplicate output for '{key}' in {GetType()}.");
            }
        }

        protected ControlInput ControlInput(string key, Func<Flow, ControlOutput> action)
        {
            EnsureUniqueInput(key);
            var port = new ControlInput(key, action);
            ControlInputs.Add(port);
            return port;
        }

        protected ControlInput ControlInputCoroutine(string key, Func<Flow, IEnumerator> coroutineAction)
        {
            EnsureUniqueInput(key);
            var port = new ControlInput(key, coroutineAction);
            ControlInputs.Add(port);
            return port;
        }

        protected ControlInput ControlInputCoroutine(string key, Func<Flow, ControlOutput> action, Func<Flow, IEnumerator> coroutineAction)
        {
            EnsureUniqueInput(key);
            var port = new ControlInput(key, action, coroutineAction);
            ControlInputs.Add(port);
            return port;
        }

        protected ControlOutput ControlOutput(string key)
        {
            EnsureUniqueOutput(key);
            var port = new ControlOutput(key);
            ControlOutputs.Add(port);
            return port;
        }

        protected ValueInput ValueInput(Type type, string key)
        {
            EnsureUniqueInput(key);
            var port = new ValueInput(key, type);
            ValueInputs.Add(port);
            return port;
        }

        protected ValueInput ValueInput<T>(string key)
        {
            return ValueInput(typeof(T), key);
        }

        protected ValueInput ValueInput<T>(string key, T @default)
        {
            var port = ValueInput<T>(key);
            port.SetDefaultValue(@default);
            return port;
        }

        protected ValueOutput ValueOutput(Type type, string key)
        {
            EnsureUniqueOutput(key);
            var port = new ValueOutput(key, type);
            ValueOutputs.Add(port);
            return port;
        }

        protected ValueOutput ValueOutput(Type type, string key, Func<Flow, object> getValue)
        {
            EnsureUniqueOutput(key);
            var port = new ValueOutput(key, type, getValue);
            ValueOutputs.Add(port);
            return port;
        }

        protected ValueOutput ValueOutput<T>(string key)
        {
            return ValueOutput(typeof(T), key);
        }

        protected ValueOutput ValueOutput<T>(string key, Func<Flow, T> getValue)
        {
            return ValueOutput(typeof(T), key, (recursion) => getValue(recursion));
        }

        private void Relation(IUnitPort source, IUnitPort destination)
        {
            Relations.Add(new UnitRelation(source, destination));
        }

        /// <summary>
        /// Triggering the destination may fetch the source value.
        /// </summary>
        protected void Requirement(ValueInput source, ControlInput destination)
        {
            Relation(source, destination);
        }

        /// <summary>
        /// Getting the value of the destination may fetch the value of the source.
        /// </summary>
        protected void Requirement(ValueInput source, ValueOutput destination)
        {
            Relation(source, destination);
        }

        /// <summary>
        /// Triggering the source may assign the destination value on the flow.
        /// </summary>
        protected void Assignment(ControlInput source, ValueOutput destination)
        {
            Relation(source, destination);
        }

        /// <summary>
        /// Triggering the source may trigger the destination.
        /// </summary>
        protected void Succession(ControlInput source, ControlOutput destination)
        {
            Relation(source, destination);
        }

        #endregion

        #region Widget

       // [Serialize]
        //public Vector2 position { get; set; }

        //[DoNotSerialize]
        public Exception DefinitionException { get; protected set; }

        #endregion

        #region Analytics

        public override AnalyticsIdentifier GetAnalyticsIdentifier()
        {
            var aid = new AnalyticsIdentifier
            {
                Identifier = GetType().FullName,
                Namespace = GetType().Namespace,
            };
            aid.Hashcode = aid.Identifier.GetHashCode();
            return aid;
        }

        #endregion
    }
}
