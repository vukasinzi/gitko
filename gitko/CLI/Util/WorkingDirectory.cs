using gitko.CLI.Objects;
using Index = gitko.CLI.Objects.Index;

namespace gitko.CLI;

public static class WorkingDirectory
{
    public static void Restore(string treeHash)
    {
        string root = Repository.Root;

        Tree tree = new();
        tree.Load(treeHash);

        Index index = new Index();
        index.Load();
        foreach (string path in index.Entries.Keys)
            DeleteFile(Path.Combine(root, path));

        tree.Rebuild(root);

        index.LoadFromTree(treeHash);
        index.Save();
    }

    public static void WriteFile(string relativePath, string blobHash)
    {
        ObjectStore os = new();
        byte[] content = os.Read(blobHash, ObjectType.Blob);

        string fullPath = Path.Combine(Repository.Root, relativePath);
        string? dir = Path.GetDirectoryName(fullPath);
        if (dir != null)
            Directory.CreateDirectory(dir);
        File.WriteAllBytes(fullPath, content);
    }

    public static void DeleteFile(string fullPath)
    {
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        string root = Repository.Root;
        string currentDir = Directory.GetCurrentDirectory();
        string? dir = Path.GetDirectoryName(fullPath);
        while (dir != null && dir != root && dir != currentDir && Directory.Exists(dir)
               && !Directory.EnumerateFileSystemEntries(dir).Any())
        {
            Directory.Delete(dir);
            dir = Path.GetDirectoryName(dir);
        }
    }
}
