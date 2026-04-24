using System;
using System.Collections.Generic;
using IoTLogic.Core.Graph;
using IoTLogic.Core.Macros;
using IoTLogic.Core.Utities;

namespace IoTLogic.Flow
{
    //[SpecialUnit]
    public abstract class NesterLogicNode<TGraph, TMacro> : LogicNode, INesterUnit
        where TGraph : class, IGraph, new()
        where TMacro : Macro<TGraph>
    {
        protected NesterLogicNode()
        {
            nest.Nester = this;
        }

        protected NesterLogicNode(TMacro macro)
        {
            nest.Nester = this;
            nest.Macro = macro;
            nest.Source = GraphSource.Macro;
        }

        public override bool CanDefine => nest.Graph != null;

        //[Serialize]
        public GraphNest<TGraph, TMacro> nest { get; private set; } = new GraphNest<TGraph, TMacro>();

        //[DoNotSerialize]
        IGraphNest IGraphNester.Nest => nest;

        //[DoNotSerialize]
        IGraph IGraphParent.ChildGraph => nest.Graph;

       // [DoNotSerialize]
        bool IGraphParent.IsSerializationRoot => nest.Source == GraphSource.Macro;

  
        protected void CopyFrom(NesterLogicNode<TGraph, TMacro> source)
        {
            base.CopyFrom(source);

            nest = source.nest;
        }

        public abstract TGraph DefaultGraph();

        IGraph IGraphParent.DefaultGraph() => DefaultGraph();

        void IGraphNester.InstantiateNest() => InstantiateNest();

        void IGraphNester.UninstantiateNest() => UninstantiateNest();
    }
}
