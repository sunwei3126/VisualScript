using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualScript.Core.EditorBinding
{
    public static class EditorTimeBinding
    {
        public static Func<int> frameBinding;

        public static Func<float> timeBinding;

        public static int frame => (frameBinding != null) ? frameBinding() : 0;

        public static float time => (timeBinding != null) ? timeBinding() : 0;

        static EditorTimeBinding()
        {
            frameBinding = () => 0;
            timeBinding = () => DateTime.Now.Millisecond;
        }
    }
}
