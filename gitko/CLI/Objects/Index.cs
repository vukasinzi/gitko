namespace gitko.CLI.Objects;
using System.Linq;
public class Index
{
    public Dictionary<string,string> Entries { get; set; }

    public void Load()
    {
        Entries = new Dictionary<string,string>();
        string root =  Helper.LocateRootDirectory();
        string[] lines = File.ReadAllLines(Path.Combine(root, ".gitko", "index"));
        foreach (string line in lines)
        {
            string[] parts = line.Split(' ', 2);
            if (parts.Length == 2)
                Entries[parts[0]] = parts[1];
        }
    }

    public void Add(string path, string hash)
    {
        Entries[path] = hash;
    }

    public void Save()
    {
        string root = Helper.LocateRootDirectory();
        var lines = Entries.OrderBy(kv => kv.Key)
            .Select(kv => $"{kv.Key} {kv.Value}");
        File.WriteAllLines(Path.Combine(root, ".gitko", "index"), lines);
    }
}