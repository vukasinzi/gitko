using System.Text.Json;

namespace gitko.CLI.Objects;
using System.Linq;
public class Index
{
    public Dictionary<string,string> Entries { get; set; }

    public void Save()
    {
        var json = JsonSerializer.Serialize(Entries);
        File.WriteAllText(Repository.IndexPath, json);
    }

    public void Load()
    {
        var path = Repository.IndexPath;
        if (!File.Exists(path))
        {
            Entries = new();
            return;
        }
        var json = File.ReadAllText(path);
        Entries = JsonSerializer.Deserialize<Dictionary<string,string>>(json) ?? new();
    }

    public void LoadFromTree(string treeHash)
    {
        Tree tree = new();
        tree.Load(treeHash);
        Entries = new();
        tree.Flatten("", Entries);
    }

    public void Add(string path, string hash)
    {
        Entries[path] = hash;
    }

    public void Remove(string path)
    {
        Entries.Remove(path);
    }

    public bool IsEmpty()
    {
        return Entries.Count == 0;
    }

    public List<string> PathsUnder(string relativePath)
    {
        return Entries.Keys
            .Where(key => relativePath == "." || key == relativePath || key.StartsWith(relativePath + "/"))
            .ToList();
    }

  
}