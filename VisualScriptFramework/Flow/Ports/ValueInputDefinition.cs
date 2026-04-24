using System;
using VisualScript.Core.Reflection;
using VisualScript.Core.Ensure;

namespace VisualScript.Flow.Ports
{
    public sealed class ValueInputDefinition : ValuePortDefinition, IUnitInputPortDefinition
    {
        //[SerializeAs(nameof(defaultValue))]
        private object _defaultvalue;

       // [Inspectable]
      //  [DoNotSerialize]
        public override Type Type
        {
            get
            {
                return base.Type;
            }
            set
            {
                base.Type = value;

                if (!Type.IsAssignableFrom(defaultValue))
                {
                    if (ValueInput.SupportsDefaultValue(Type))
                    {
                        _defaultvalue = Type.PseudoDefault();
                    }
                    else
                    {
                        hasDefaultValue = false;
                        _defaultvalue = null;
                    }
                }
            }
        }

        //[Serialize]
        //[Inspectable]
        public bool hasDefaultValue { get; set; }

      //  [DoNotSerialize]
       // [Inspectable]
        public object defaultValue
        {
            get
            {
                return _defaultvalue;
            }
            set
            {
                if (Type == null)
                {
                    throw new InvalidOperationException("A type must be defined before setting the default value.");
                }

                if (!ValueInput.SupportsDefaultValue(Type))
                {
                    throw new InvalidOperationException("The selected type does not support default values.");
                }

                Ensure.That(nameof(value)).IsOfType(value, Type);

                _defaultvalue = value;
            }
        }
    }
}
