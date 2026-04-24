using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualScript.Flow;
using VisualScript.Flow.Ports;

namespace VisualScriptFramework.Flow.Framework
{
    public sealed class If: Unit, IBranchUnit
    {
         public ControlInput enter { get; private set; }

         public  ValueInput condition { get; private set; } 

         public ControlOutput ifTrue { get; private set; }

         public ControlOutput ifFalse { get;private set; }


        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Enter);
            condition = ValueInput<bool>(nameof(condition));
            ifTrue = ControlOutput(nameof(ifTrue));
            ifFalse = ControlOutput(nameof(ifFalse));

            Requirement(condition, enter);
            Succession(enter, ifTrue);
            Succession(enter, ifFalse);
        }

        public ControlOutput Enter(VisualScript.Flow.Flow flow)
        {
            return flow.GetValue<bool>(condition) ? ifTrue : ifFalse;
        }
    }
}
