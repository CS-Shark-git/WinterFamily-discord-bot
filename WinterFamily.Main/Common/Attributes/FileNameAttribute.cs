namespace WinterFamily.Main.Common.Attributes;

internal class FileNameAttribute : Attribute
{
    public string Name { get; }
    public FileNameAttribute(string name) => Name = name;
}
