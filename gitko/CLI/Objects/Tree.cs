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
}