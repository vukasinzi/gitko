using System.Text;
using System.Text.Json;
using gitko.Models;

namespace gitko.CLI.Objects;

public class Tree
{
    public List<TreeEntry> TreeEntries { get; set; } = new();

    public Response Save()
    {
        string json = JsonSerializer.Serialize(TreeEntries);
        byte[] data = Encoding.UTF8.GetBytes(json);

        ObjectStore os = new();
        return os.Store(data, ObjectType.Tree);
    }
}