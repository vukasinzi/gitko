using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gitko.CLI.Objects;

public class Commit
{
    public string Tree { get; set; }
    public string Parent { get; set; } = "";
    public string Author { get; set; } = "Nepoznat";
    public DateTime Timestamp { get; set; }
    public string Message { get; set; }

    [JsonIgnore]
    public string Hash { get; set; }

    public void Save()
    {
        string json = JsonSerializer.Serialize(this);
        byte[] data = Encoding.UTF8.GetBytes(json);

        ObjectStore os = new();
        Hash = os.Store(data, ObjectType.Commit);
    }

    public static Commit Load(string hash)
    {
        ObjectStore os = new();
        byte[] content = os.Read(hash, ObjectType.Commit);
        string json = Encoding.UTF8.GetString(content);

        Commit commit = JsonSerializer.Deserialize<Commit>(json);
        if (commit == null)
            throw new GitkoException("Poslednji komit nije pronađen");
        commit.Hash = hash;
        return commit;
    }

    public override string ToString()
    {
        return $"{Hash}, {Message}, {Author}, {Timestamp.ToString()}";
    }
}