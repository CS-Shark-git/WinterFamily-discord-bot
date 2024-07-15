namespace WinterFamily.Main.Common.Attributes;

internal class ComponentInteractionAttribute : Attribute
{
    public string CustomId { get; }
    public ComponentInteractionAttribute(string name) => CustomId = name;
}
