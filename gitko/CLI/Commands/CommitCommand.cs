using System.Data;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using gitko.CLI.Objects;
using gitko.Models;

namespace gitko.CLI;
using Index = gitko.CLI.Objects.Index;


public class CommitCommand : Command
{
    public CommitCommand(string[] args) : base(args)
    {
        
    }

    public override bool ValidateArguments()
    {
        if (args.Length != 2)
            return false;
        if (args[0] != "-m")
            return false;
        return true;
    }

    private string RecursiveTree(string dir, Dictionary<string,string> entries)
    {
        Tree tree = new Tree();
        foreach (var entry in entries)
        {
            if (Path.GetDirectoryName(entry.Key) == dir)
                tree.TreeEntries.Add(new TreeEntry(ObjectType.Blob, Path.GetFileName(entry.Key), entry.Value));
        }

        foreach (var entry in entries)
        {
            string prefix = dir;
            if(dir != "")
                prefix = dir + '/';
    
            if(!entry.Key.StartsWith(prefix))
                continue;
    
            var editedEntry = entry.Key.Substring(prefix.Length);
            if(!editedEntry.Contains('/'))
                continue;

            var folderName = editedEntry.Split('/')[0];
            if (tree.TreeEntries.Any(e => e.Name == folderName && e.Type == ObjectType.Tree))
                continue;
            
            var newDir = dir;
            if (dir != "")
                newDir = dir + "/" + folderName;
            else
                newDir = folderName;

            var hash = RecursiveTree(newDir, entries);
            tree.TreeEntries.Add(new TreeEntry(ObjectType.Tree, folderName, hash));
        }

        Response resp = tree.Save();
        if (!resp.Success)
            throw new DataException("Nemoguće sačuvati tree");
        return (string)resp.Data;
    }
    private string GetBranchPath()
    {
        var root = Helper.LocateRootDirectory();
        string headPath = Path.Combine(root, ".gitko", "HEAD");
        string data = File.ReadAllText(headPath).Trim();
        data = data[5..];

        return Path.Combine(root, ".gitko", data);
    }

    private Commit LoadThatCommit(string lastCommitHash)
    {
        string part1 = lastCommitHash[0..2];
        string part2 = lastCommitHash[2..];

        var root = Helper.LocateRootDirectory();
        var file = Path.Combine(root, ".gitko", "objects", part1, part2);

        byte[] fullBytes = File.ReadAllBytes(file);
        int nullIndex = Array.IndexOf(fullBytes, (byte)0);
        byte[] jsonBytes = fullBytes[(nullIndex + 1)..];//micemo header commit /0
        string json = Encoding.UTF8.GetString(jsonBytes);

        Commit commit = JsonSerializer.Deserialize<Commit>(json);
        return commit;
    }
    public override void Run()
    {
        Index index = new Index();
        index.Load();
        
        string treeHash = RecursiveTree("", index.Entries);
        Commit commit = new();
        var (isFirst, lastCommitHash) = isItFirstCommit();
        if (isFirst)
        {
            commit.Parent = null;
        }
        else
        {
            Commit last = LoadThatCommit(lastCommitHash);
            if (last.Tree == treeHash)
            {
                Console.WriteLine("Ne postoje promene za commitovanje");
                return;
            }
            commit.Parent = lastCommitHash;
        }
        commit.Tree = treeHash;
        commit.Message = args[1].Replace("\"", "");
        commit.Timestamp = DateTime.Now;

        Response resp = commit.Save();
        if (!resp.Success)
            throw new Exception("Neuspešno komitovanje promena");

        if(WriteToBranch((string)resp.Data))
            Console.WriteLine($"Uspešno komitovanje. Hash: {resp.Data}");
        throw new Exception("Commit sačuvan, ali HEAD nije ažuriran");
    }

    public bool WriteToBranch(string hash)
    {
        string branchPath = GetBranchPath();
        File.WriteAllText(branchPath, hash);
        return File.ReadAllText(branchPath) == hash;
    }

    private Tuple<bool,string> isItFirstCommit()
    {
        string branchPath = GetBranchPath();
        if (!Path.Exists(branchPath))
            return new Tuple<bool, string>(true, "");

        var lastCommit = File.ReadAllText(branchPath);
        return new Tuple<bool, string>(false, lastCommit);
    }
}
