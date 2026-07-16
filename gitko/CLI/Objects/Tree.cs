using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using gitko.Models;

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
        Response resp = os.Store(data, ObjectType.Tree);

        if (!resp.Success)
            throw new Exception(resp.Message);

        Hash = (string)resp.Data;
    }

    public void Rebuild(string targetDirectory)
    {
        foreach (TreeEntry treeEntry in TreeEntries)
        {
            if (treeEntry.Type == ObjectType.Blob)
            {
                byte[] fullBytes = File.ReadAllBytes(Path.Combine(Helper.LocateRootDirectory(), ".gitko", "objects",
                    treeEntry.Hash[0..2], treeEntry.Hash[2..]));
                
                int nullIndex = Array.IndexOf(fullBytes, (byte)0);
                byte[] content = fullBytes[(nullIndex + 1)..];

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
        string part1 = hash[0..2];
        string part2 = hash[2..];

        string root = Helper.LocateRootDirectory();
        string file = Path.Combine(root, ".gitko", "objects", part1, part2);

        byte[] fullBytes = File.ReadAllBytes(file);
        int nullIndex = Array.IndexOf(fullBytes, (byte)0);
        byte[] jsonBytes = fullBytes[(nullIndex + 1)..];
        string json = Encoding.UTF8.GetString(jsonBytes);

        List<TreeEntry> entries = JsonSerializer.Deserialize<List<TreeEntry>>(json);
        if (entries == null)
            throw new NullReferenceException("Tree nije pronađen");

        TreeEntries = entries;
        Hash = hash;
    }


 

}