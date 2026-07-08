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
        string path = Helper.LocatePath(args[0]);

        if (!Helper.IsInsideRootDirectory(path))
            throw new ArgumentException("Fajl mora biti unutar gitko repozitorijuma");

        if (!Path.Exists(path))
            throw new DirectoryNotFoundException("Direktorijum/Fajl koji želite dodati ne postoji");

        Index index = new Index();
        index.Load();

        if (Directory.Exists(path))
        {
            foreach (string file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                AddFile(file, index);
        }
        else
            AddFile(path, index);

        index.Save();
    }

    private void AddFile(string path, Index index)
    {
        ObjectStore objectStore = new();
        string rootPath = Helper.LocateRootDirectory();
        string relativePath = Path.GetRelativePath(rootPath, path).Replace("\\", "/");

        if (relativePath == ".gitko" || relativePath.StartsWith(".gitko/"))
            return;
        
        byte[] content = File.ReadAllBytes(path);
        Response resp = objectStore.Store(content,ObjectType.Blob);

        if (!resp.Success)
            throw new ArgumentException(resp.Message);

        string hash = (string)resp.Data;
        index.Add(relativePath, hash);
        Console.WriteLine($"Dodat {relativePath}");
    }
}
