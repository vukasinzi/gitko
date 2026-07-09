using System.Text;
using System.Text.Json;
using gitko.Models;

namespace gitko.CLI.Objects;

public class Commit
{
    public string Tree { get; set; }
    public string Parent { get; set; }
    public string Author { get; set; } = "Nepoznat";
    public DateTime Timestamp { get; set; }
    public string Message { get; set; }

    public Response Save()
    {
        string json = JsonSerializer.Serialize(this);
        byte[] data = Encoding.UTF8.GetBytes(json);

        ObjectStore os = new();
        return os.Store(data, ObjectType.Commit);
    }
}