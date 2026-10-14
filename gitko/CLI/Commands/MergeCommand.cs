using gitko.CLI.Objects;
using Index = gitko.CLI.Objects.Index;

namespace gitko.CLI;

public class MergeCommand : Command
{
    public MergeCommand(string[] args) : base(args)
    {
    }

    public override bool ValidateArguments()
    {
        if (args.Length == 1)
            return true;

        Console.Error.WriteLine("Korišćenje: gitko merge <grana>");
        return false;
    }

    public override void Run()
    {
        string otherBranchName = args[0];
        Refs.ValidateName(otherBranchName);

        string currentHash = Refs.CurrentCommit();
        if (string.IsNullOrEmpty(currentHash))
            throw new GitkoException("Trenutna grana nema komitove.");
        string otherHash = LoadBranchHash(otherBranchName);
        
        if (IsCurrentBranchUpToDateWith(currentHash, otherHash))
            throw new GitkoException($"Grana je već ažurna: sadrži sve komitove sa grane {otherBranchName}.");

        if (IsOtherBranchUpToDateWith(currentHash, otherHash))
        {
            FastForward(otherHash);
            return;
        }

        ThreeWayMerge(currentHash,otherHash);

    }

    private void ThreeWayMerge(string currentHash, string otherHash)
    {
        string commonAncestorHash = FindCommonAncestor(currentHash, otherHash);

        Dictionary<string, string> commonFiles = FilesOf(commonAncestorHash);
        Dictionary<string, string> currentFiles = FilesOf(currentHash);
        Dictionary<string, string> otherFiles = FilesOf(otherHash);

        List<string> conflicts = new();
        Dictionary<string, string> merged = MergeFiles(commonFiles, currentFiles, otherFiles, conflicts);

        if (conflicts.Count > 0)
            throw new GitkoException("Konflikt u fajlovima: " + string.Join(", ", conflicts));

        UpdateWorkingDirectory(currentFiles, merged);

        Index index = new Index();
        index.Entries = merged;
        index.Save();

        new CommitCommand(new string[] { "-m", "Merge grane " + args[0] }).Run();
        MoveBranchToCurrent(args[0]);
    }

    private string FindCommonAncestor(string currentHash, string otherHash)
    {
        HashSet<string> currentHistory = new();
        string hash = currentHash;
        while (!string.IsNullOrEmpty(hash))
        {
            currentHistory.Add(hash);
            hash = Commit.Load(hash).Parent;
        }

        hash = otherHash;
        while (!string.IsNullOrEmpty(hash))
        {
            if (currentHistory.Contains(hash))
                return hash;
            hash = Commit.Load(hash).Parent;
        }

        throw new GitkoException("Grane nemaju zajedničkog pretka.");
    }

    private Dictionary<string, string> FilesOf(string commitHash)
    {
        Commit commit = Commit.Load(commitHash);
        Tree tree = new();
        tree.Load(commit.Tree);

        Dictionary<string, string> files = new();
        tree.Flatten("", files);
        return files;
    }

    private Dictionary<string, string> MergeFiles(Dictionary<string, string> commonFiles, Dictionary<string, string> currentFiles,
        Dictionary<string, string> otherFiles, List<string> conflicts)
    {
        SortedSet<string> allPaths = new(StringComparer.Ordinal);
        allPaths.UnionWith(commonFiles.Keys);
        allPaths.UnionWith(currentFiles.Keys);
        allPaths.UnionWith(otherFiles.Keys);

        Dictionary<string, string> merged = new();
        foreach (string path in allPaths)
        {
            string? commonFile = commonFiles.GetValueOrDefault(path);
            string? currentFile = currentFiles.GetValueOrDefault(path);
            string? otherFile = otherFiles.GetValueOrDefault(path);

            string? result;
            //ako je current isti kao other ili ako je other zapravo common file, tada pobedjuje current
            if (currentFile == otherFile || otherFile == commonFile)
                result = currentFile;
            else if (currentFile == commonFile)//ako je trenutni common file, tada pobedjuje other
                result = otherFile;
            else //u suprotnom imamo honflikt!
            {
                conflicts.Add(path);
                continue;
            }

            if (result != null)
                merged[path] = result;
        }
        return merged;
    }

    private void UpdateWorkingDirectory(Dictionary<string, string> currentFiles, Dictionary<string, string> merged)
    {
        foreach (string path in currentFiles.Keys)
        {
            if (!merged.ContainsKey(path))
                WorkingDirectory.DeleteFile(Path.Combine(Repository.Root, path));
        }

        foreach (KeyValuePair<string, string> file in merged)
        {
            bool unchanged = currentFiles.GetValueOrDefault(file.Key) == file.Value;
            if (!unchanged)
                WorkingDirectory.WriteFile(file.Key, file.Value);
        }
    }

    private void MoveBranchToCurrent(string name)
    {
        File.WriteAllText(Path.Combine(Repository.HeadsDir, name), Refs.CurrentCommit());
        Console.WriteLine($"Grana {name} sada pokazuje na isti komit kao trenutna grana.");
    }

    private void FastForward(string otherHash)
    {
        Commit commit = Commit.Load(otherHash);
        WorkingDirectory.Restore(commit.Tree);
        File.WriteAllText(Refs.CurrentBranchPath(), otherHash);
        Console.WriteLine($"Fast-forward: Grana {args[0]} je mergeovana u trenutnu granu");

    }

    // Fast forward moze kad main nije menjan, a features je menjan, i features mergeujem u main.
    // Primer: main: A, features: A <- B
    private bool IsOtherBranchUpToDateWith(string currentHash, string otherHash)
    {
        string hash = otherHash;
        while (!string.IsNullOrEmpty(hash))
        {
            if (hash == currentHash)
                return true;
            hash = Commit.Load(hash).Parent;
        }
        return false;
    }

    private bool IsCurrentBranchUpToDateWith(string currentHash, string otherHash)
    {
        string hash = currentHash;
        while (!string.IsNullOrEmpty(hash))
        {
            if (hash == otherHash)
                return true;
            hash = Commit.Load(hash).Parent;
        }
        return false;
    }

    private string LoadBranchHash(string name)
    {
        string path = Path.Combine(Repository.HeadsDir, name);
        if (!File.Exists(path))
            throw new GitkoException("Grana ne postoji.");

        string hash = File.ReadAllText(path).Trim();
        if (string.IsNullOrEmpty(hash))
            throw new GitkoException($"Grana {name} nema komitove.");
        return hash;
    }
}