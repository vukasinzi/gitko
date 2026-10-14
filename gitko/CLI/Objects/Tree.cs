using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gitko.CLI.Objects;

public class Tree
{
    public List<TreeEntry> TreeEntries { get; set; } = new();

    [JsonIgnore]
    public string Hash { get; set; }

    public void Save()
    {
        string json = JsonSerializer.Serialize(TreeEntries);
        byte[] data = Encoding.UTF8.GetBytes(json);

        ObjectStore os = new();
        Hash = os.Store(data, ObjectType.Tree);
    }

    public void Rebuild(string targetDirectory)
    {
        foreach (TreeEntry treeEntry in TreeEntries)
        {
            if (treeEntry.Type == ObjectType.Blob)
            {
                ObjectStore os = new();
                byte[] content = os.Read(treeEntry.Hash, ObjectType.Blob);

                File.WriteAllBytes(Path.Combine(targetDirectory, treeEntry.Name), content);
                
            }
            else if (treeEntry.Type == ObjectType.Tree)
            {
                string temp = Path.Combine(targetDirectory, treeEntry.Name);
                Directory.CreateDirectory(temp);
                
                Tree novi = new();
                novi.Load(treeEntry.Hash);
                novi.Rebuild(temp);
            }


        }
    }
    public void Load(string hash)
    {
        ObjectStore os = new();
        byte[] content = os.Read(hash, ObjectType.Tree);
        string json = Encoding.UTF8.GetString(content);

        List<TreeEntry> entries = JsonSerializer.Deserialize<List<TreeEntry>>(json);
        if (entries == null)
            throw new GitkoException("Tree nije pronađen");

        TreeEntries = entries;
        Hash = hash;
    }

    public void Flatten(string prefix, Dictionary<string, string> entries)
    {
        foreach (TreeEntry entry in TreeEntries)
        {
            string path = prefix == "" ? entry.Name : prefix + "/" + entry.Name;

            if (entry.Type == ObjectType.Blob)
            {
                entries[path] = entry.Hash;
            }
            else if (entry.Type == ObjectType.Tree)
            {
                Tree sub = new();
                sub.Load(entry.Hash);
                sub.Flatten(path, entries);
            }
        }
    }


 

}