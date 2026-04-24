namespace VisualScript.Flow.Ports
{
    public abstract class UnitPortDefinition : IUnitPortDefinition
    {
       // [Serialize, Inspectable, InspectorDelayed]
       // [WarnBeforeEditing("Edit Port Key", "Changing the key of this definition will break any existing connection to this port. Are you sure you want to continue?", null, "")]
        public string Key { get; set; }

       // [Serialize, Inspectable]
        public string Label { get; set; }

        //[Serialize, Inspectable, InspectorTextArea]
        public string Summary { get; set; }

       // [Serialize, Inspectable]
        public bool HideLabel { get; set; }

       // [DoNotSerialize]
        public virtual bool IsValid => !string.IsNullOrEmpty(Key);
    }
}
