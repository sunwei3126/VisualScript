using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualScriptFramework.Flow.Framework
{
    public class ScalarAbsolute : Absolute<float>
    {
        protected override float Operation(float input)
        {
            return Math.Abs(input);
        }
    }
}
