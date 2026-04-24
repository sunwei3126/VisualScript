using System;

namespace IoTLogic.Flow.Ports
{
    public abstract class ValuePortDefinition : UnitPortDefinition, IUnitValuePortDefinition
    {
        // For the virtual inheritors
       // [SerializeAs(nameof(_type))]
        private Type _type { get; set; }

      //  [Inspectable]
     //   [DoNotSerialize]
        public virtual Type Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }

        public override bool IsValid => base.IsValid && Type != null;
    }
}
