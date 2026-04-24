namespace VisualScript.Flow.Ports
{
    public interface IUnitPortDefinition
    {
        string Key { get; }
        string Label { get; }
        string Summary { get; }
        bool HideLabel { get; }
        bool IsValid { get; }
    }
}
