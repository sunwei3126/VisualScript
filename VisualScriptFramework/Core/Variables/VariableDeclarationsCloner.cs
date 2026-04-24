using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualScript.Core.Reflection;

namespace VisualScript.Core.Variables
{
    // Variable declarations are cloned for every graph instantiation, it's worth speeding it up.

    public sealed class VariableDeclarationsCloner : Cloner<VariableDeclarations>
    {
        public static readonly VariableDeclarationsCloner instance = new VariableDeclarationsCloner();

        public override bool Handles(Type type)
        {
            return type == typeof(VariableDeclarations);
        }

        public override VariableDeclarations ConstructClone(Type type, VariableDeclarations original)
        {
            return new VariableDeclarations();
        }

        public override void FillClone(Type type, ref VariableDeclarations clone, VariableDeclarations original, CloningContext context)
        {
            foreach (var variable in original)
            {
                clone[variable.Name] = variable.Value.CloneViaFakeSerialization();
            }
        }
    }
}
