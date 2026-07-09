namespace gitko.CLI.Objects;

public class TreeEntry
{
    public TreeEntry(ObjectType type, string name, string hash)
    {
        Type = type;
        Name = name;
        Hash = hash;
    }

    public ObjectType Type { get; set; }
    public string Name { get; set; }
    public string Hash { get; set; }
}