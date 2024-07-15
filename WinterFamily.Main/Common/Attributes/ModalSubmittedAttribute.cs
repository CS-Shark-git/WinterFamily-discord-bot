namespace WinterFamily.Main.Common.Attributes;

internal class ModalSubmittedAttribute : Attribute
{
    public string CustomId { get; }
    public ModalSubmittedAttribute(string name) => CustomId = name;
}
