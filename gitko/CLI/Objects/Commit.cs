using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using gitko.Models;

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
        Response resp = os.Store(data, ObjectType.Commit);

        if (!resp.Success)
            throw new Exception(resp.Message);

        Hash = (string)resp.Data;
    }

    public void Load()
    {
        string lastHash = Helper.LoadLastCommit();
        Commit lastCommit = Helper.LoadThatCommit(lastHash);
        this.Tree = lastCommit.Tree;
        this.Parent = lastCommit.Parent;
        this.Author = lastCommit.Author;
        this.Timestamp = lastCommit.Timestamp;
        this.Message = lastCommit.Message;
        
    }
}