using System;
using IoTLogic.Core.Graph;

namespace IoTLogic.Core.Macros
{
    //[DisableAnnotation]
    public abstract class Macro<TGraph> : IMacro
          where TGraph : class, IGraph, new()
    {
       // [SerializeAs(nameof(graph))]
        private TGraph _graph = new TGraph();

        //[DoNotSerialize]
        public TGraph Graph
        {
            get => _graph;
            set
            {
                if (value == null)
                {
                    throw new InvalidOperationException("Macros must have a graph.");
                }

                if (value == Graph)
                {
                    return;
                }

                _graph = value;
            }
        }

       // [DoNotSerialize]
        IGraph IMacro.Graph
        {
            get => Graph;
            set => Graph = (TGraph)value;
        }

       // [DoNotSerialize]
        IGraph IGraphParent.ChildGraph => Graph;

       

       // [DoNotSerialize]
        bool IGraphParent.IsSerializationRoot => true;


        //[DoNotSerialize]
        private GraphReference _reference = null;

       // [DoNotSerialize]
        protected GraphReference reference => _reference == null ? GraphReference.New(this, false) : _reference;

        public bool isDescriptionValid
        {
            get => true;
            set { }
        }

        

        public abstract TGraph DefaultGraph();

        IGraph IGraphParent.DefaultGraph()
        {
            return DefaultGraph();
        }

        // This seems to fix the legendary undo bug!
        // https://support.ludiq.io/communities/5/topics/4434-undo-bug-isolated
        // The issue seems to be that newly created assets don't receive OnAfterDeserialize,
        // and therefore never notify the dependencies system that they became available.
        // Therefore, if any graph relied on a macro dependency (super LogicNode, super state, flow state, state LogicNode)
        // that was created before a deserialization of that dependency (usually enter/exit play mode, restart Unity),
        // it would silently never load, not throwing any error or warning along the way.
        // For example, creating a new flow macro, dragging it to create a super node in another graph,
        // then undoing, would corrupt the parent graph.
        // Note: this *could* go in Awake, but OnEnable seems to be more reliable and consistent. Awake
        // doesn't get called in play mode entry for example (but that doesn't matter because OnAfterDeserialize does anyway).
        protected virtual void OnEnable()
        {
            //Serialization.NotifyDependencyAvailable(this);
        }

        // ScriptableObjects actually call OnDisable not OnDestroy when unloaded ("goes out of scope"),
        // so we need to unregister the dependency here.
        // https://forum.unity.com/threads/scriptableobject-behaviour-discussion-how-scriptable-objects-work.541212/
        // The doc also guarantees it will be called before OnDestroy, so no need to repeat that in OnDestory.
        protected virtual void OnDisable()
        {
            //Serialization.NotifyDependencyUnavailable(this);
        }

        public GraphPointer GetReference()
        {
            return reference;
        }
    }
}
