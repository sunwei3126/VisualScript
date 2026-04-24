using System;
using VisualScript.Core.Exceptions;
using VisualScript.Core.Macros;

namespace VisualScript.Core.Graph
{
    public sealed class GraphNest<TGraph, TMacro> : IGraphNest
         where TGraph : class, IGraph, new()
         where TMacro : Macro<TGraph>
    {
        //[DoNotSerialize]
        public IGraphNester Nester { get; set; }

       // [DoNotSerialize]
        private GraphSource _source = GraphSource.Macro;

        //[DoNotSerialize]
        private TMacro _macro;

       // [DoNotSerialize]
        private TGraph _embed;

       // [Serialize]
        public GraphSource Source
        {
            get => _source;
            set
            {
                if (value == Source)
                {
                    return;
                }

                BeforeGraphChange();

                _source = value;

                AfterGraphChange();
            }
        }

        //[Serialize]
        public TMacro Macro
        {
            get => _macro;
            set
            {
                if (value == Macro)
                {
                    return;
                }

                BeforeGraphChange();

                _macro = value;

                AfterGraphChange();
            }
        }

        //[Serialize]
        public TGraph Embed
        {
            get => _embed;
            set
            {
                if (value == Embed)
                {
                    return;
                }

                BeforeGraphChange();

                _embed = value;

                AfterGraphChange();
            }
        }

        //[DoNotSerialize]
        public TGraph Graph
        {
            get
            {
                switch (Source)
                {
                    case GraphSource.Embed:
                        return Embed;

                    case GraphSource.Macro:
                        return Macro?.Graph;

                    default:
                        throw new UnexpectedEnumValueException<GraphSource>(Source);
                }
            }
        }

        IMacro IGraphNest.Macro
        {
            get => Macro;
            set => Macro = (TMacro)value;
        }

        IGraph IGraphNest.Embed
        {
            get => Embed;
            set => Embed = (TGraph)value;
        }

        IGraph IGraphNest.Graph => Graph;

        Type IGraphNest.GraphType => typeof(TGraph);

        Type IGraphNest.MacroType => typeof(TMacro);

        // TODO: Use these in the editor when appropriate to minimize change events
        public void SwitchToEmbed(TGraph embed)
        {
            if (Source == GraphSource.Embed && this.Embed == embed)
            {
                return;
            }

            BeforeGraphChange();

            _source = GraphSource.Embed;
            _embed = embed;
            _macro = null;

            AfterGraphChange();
        }

        public void SwitchToMacro(TMacro macro)
        {
            if (Source == GraphSource.Macro && this.Macro == macro)
            {
                return;
            }

            BeforeGraphChange();

            _source = GraphSource.Macro;
            _embed = null;
            _macro = macro;

            AfterGraphChange();
        }

        public event Action beforeGraphChange;

        public event Action afterGraphChange;

        private void BeforeGraphChange()
        {
            if (Graph != null)
            {
                Nester.UninstantiateNest();
            }

            beforeGraphChange?.Invoke();
        }

        private void AfterGraphChange()
        {
            afterGraphChange?.Invoke();

            if (Graph != null)
            {
                Nester.InstantiateNest();
            }
        }
        #region Poutine

        //[DoNotSerialize]
        public bool HasBackgroundEmbed => Source == GraphSource.Macro && Embed != null;

        #endregion
    }
}
