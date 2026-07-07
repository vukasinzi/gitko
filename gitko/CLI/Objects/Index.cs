using System.Text.Json;

namespace gitko.CLI.Objects;
using System.Linq;
public class Index
{
    public Dictionary<string,string> Entries { get; set; }

    public void Save()
    {
        var root = Helper.LocateRootDirectory();
        var json = JsonSerializer.Serialize(Entries);
        File.WriteAllText(Path.Combine(root, ".gitko", "index"), json);
    }

    public void Load()
    {
        var root = Helper.LocateRootDirectory();
        var path = Path.Combine(root, ".gitko", "index");
        if (!File.Exists(path))
        {
            Entries = new();
        }
        var json = File.ReadAllText(path);
        Entries = JsonSerializer.Deserialize<Dictionary<string,string>>(json) ?? new();
    }

    public void Add(string path, string hash)
    {
        Entries[path] = hash;
    }

  
}