using gitko.CLI.Objects;
using gitko.Models;
using Index = gitko.CLI.Objects.Index;

namespace gitko.CLI;

public class AddCommand : Command
{
    public AddCommand(string[] args) : base(args) { }

    public override bool ValidateArguments()
    {
        if (args.Length != 1)
            throw new ArgumentException("Dodaj zahteva tačno 1 argument");
        return true;
    }

    public override void Run()
    {
        ObjectStore objectStore = new();
        string path = Helper.LocatePath(args[0]);
        byte[] content = File.ReadAllBytes(path);

        Response resp = objectStore.Store(content);
        if (resp.Success == false)
            throw new ArgumentException(resp.Message);

        string rootPath = Helper.LocateRootDirectory();
        string relativePath = Path.GetRelativePath(rootPath, path);
        relativePath = relativePath.Replace("\\", "/");

        Index index = new Index();
        index.Load();
        string d = (string)resp.Data;
        index.Add(relativePath, d);
        index.Save();
    }
}