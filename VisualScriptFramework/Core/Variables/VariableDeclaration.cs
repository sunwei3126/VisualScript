using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTLogic.Core.Variables
{
    //[SerializationVersion("A")]
    public sealed class VariableDeclaration
    {
        [Obsolete("ConstructorWarning")]
        public VariableDeclaration() { }

        public VariableDeclaration(string name, object value)
        {
            this.Name = name;
            this.Value = value;
        }

        //[Serialize]
        public string Name { get; private set; }

        //[Serialize, Value]
        public object Value { get; set; }

       // [Serialize]
        //public SerializableType typeHandle { get; set; }
    }
}
