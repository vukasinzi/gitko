using gitko.CLI.Objects;
using Index = gitko.CLI.Objects.Index;

namespace gitko.CLI;

public class AddCommand : Command
{
    public AddCommand(string[] args) : base(args) { }

    public override bool ValidateArguments()
    {
        if (args.Length != 1)
            throw new GitkoException("Add zahteva tačno 1 argument");
        return true;
    }

    public override void Run()
    {
        string path = Helper.LocatePath(args[0]);

        if (!Helper.IsInsideRootDirectory(path))
            throw new GitkoException("Fajl mora biti unutar gitko repozitorijuma");

        Index index = new Index();
        index.Load();

        int removed = RemoveDeleted(path, index);

        if (Directory.Exists(path))
        {
            foreach (string file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                AddFile(file, index);
        }
        else if (File.Exists(path))
            AddFile(path, index);
        else if (removed == 0)
            throw new GitkoException("Direktorijum/Fajl koji želite dodati ne postoji");

        index.Save();
    }

    private int RemoveDeleted(string path, Index index)
    {
        string rootPath = Helper.LocateRootDirectory();
        string relativePath = Path.GetRelativePath(rootPath, path).Replace("\\", "/");

        List<string> missing = index.PathsUnder(relativePath)
            .Where(key => !File.Exists(Path.Combine(rootPath, key)))
            .ToList();

        foreach (string key in missing)
        {
            index.Remove(key);
            Console.WriteLine($"Obrisan {key}");
        }
        return missing.Count;
    }

    private void AddFile(string path, Index index)
    {
        
        ObjectStore objectStore = new();
        string rootPath = Helper.LocateRootDirectory();
        string relativePath = Path.GetRelativePath(rootPath, path).Replace("\\", "/");

        if (relativePath == ".gitko" || relativePath.StartsWith(".gitko/"))
            return;
      
        byte[] content = File.ReadAllBytes(path);
        string hash = objectStore.Store(content,ObjectType.Blob);
        if (index.Entries.TryGetValue(relativePath, out var existingHash) && existingHash == hash)
            return;
        index.Add(relativePath, hash);
        Console.WriteLine($"Dodat {relativePath}");
    }
    
}
