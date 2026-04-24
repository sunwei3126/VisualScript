using System.Windows.Media;
using VisualScript.Core.Graph;

namespace VisualScript.Core.Groups
{
    //[SerializationVersion("A")]
    public sealed class GraphGroup : GraphElement<IGraph>
    {
        //[DoNotSerialize]
        public static readonly Color defaultColor = Colors.White;

        public GraphGroup() : base() { }

        //[Serialize]
        //public Rect position { get; set; }

        //[Serialize]
        public string Label { get; set; } = "Group";

       // [Serialize]
       // [InspectorTextArea(minLines = 1, maxLines = 10)]
        public string Comment { get; set; }

       // [Serialize]
       // [Inspectable]
        public Color Color { get; set; } = defaultColor;
    }
}
