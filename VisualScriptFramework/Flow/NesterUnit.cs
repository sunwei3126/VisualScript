using System;
using System.Collections.Generic;
using VisualScript.Core.Graph;
using VisualScript.Core.Macros;
using VisualScript.Core.Utities;

namespace VisualScript.Flow
{
    //[SpecialUnit]
    public abstract class NesterUnit<TGraph, TMacro> : Unit, INesterUnit
        where TGraph : class, IGraph, new()
        where TMacro : Macro<TGraph>
    {
        protected NesterUnit()
        {
            nest.Nester = this;
        }

        protected NesterUnit(TMacro macro)
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

  
        protected void CopyFrom(NesterUnit<TGraph, TMacro> source)
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
