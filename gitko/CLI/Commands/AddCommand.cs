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

        if (!Helper.IsInsideRootDirectory(path))
            throw new ArgumentException("Fajl mora biti unutar gitko repozitorijuma");

        if (!File.Exists(path))
            throw new FileNotFoundException("Fajl koji želite dodati nepostoji");

        string rootPath = Helper.LocateRootDirectory();
        string relativePath = Path.GetRelativePath(rootPath, path);
        relativePath = relativePath.Replace("\\", "/");

        Index index = new Index();
        index.Load();
        byte[] content = File.ReadAllBytes(path);

        Response resp = objectStore.Store(content);
        if (resp.Success == false)
            throw new ArgumentException(resp.Message);

        if(resp.Data == null)
            throw new FileNotFoundException("Fajl koji želite dodati nepostoji");
        string d = (string)resp.Data;
        index.Add(relativePath, d);
        index.Save();
    }
}
