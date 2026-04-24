using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualScript.Core.Utities;

namespace VisualScript.Core.Graph
{
    public abstract class Graph : IGraph
    {
        protected Graph()
        {
            Elements = new MergedGraphElementCollection();
        }

        public override string ToString()
        {
            return StringUtility.FallbackWhitespace(Title, base.ToString());
        }

        public abstract IGraphData CreateData();

        public virtual IGraphDebugData CreateDebugData()
        {
            return new GraphDebugData(this);
        }

        public virtual void Instantiate(GraphReference instance)
        {
            // Debug.Log($"Instantiating graph {instance}");

            foreach (var element in Elements)
            {
                element.Instantiate(instance);
            }
        }

        public virtual void Uninstantiate(GraphReference instance)
        {
            // Debug.Log($"Uninstantiating graph {instance}");

            foreach (var element in Elements)
            {
                element.Uninstantiate(instance);
            }
        }

        #region Elements

        //[SerializeAs(nameof(Elements))]
        private List<IGraphElement> _elements = new List<IGraphElement>();

        //[DoNotSerialize]
        public MergedGraphElementCollection Elements { get; }

        #endregion


        #region Metadata

       // [Serialize]
        public string Title { get; set; }

       // [Serialize]
        //[InspectorTextArea(minLines = 1, maxLines = 10)]
        public string Summary { get; set; }

        #endregion


        #region Canvas

        //[Serialize]
       // public Vector2 pan { get; set; }

        //[Serialize]
       // public float zoom { get; set; } = 1;

        #endregion

        #region Poutine

        private bool prewarmed;
        public void Prewarm()
        {
            if (prewarmed)
            {
                return;
            }

            foreach (var element in Elements)
            {
                element.Prewarm();
            }

            prewarmed = true;
        }

        public virtual void Dispose()
        {
            foreach (var element in Elements)
            {
                element.Dispose();
            }
        }

        #endregion
    }
}
