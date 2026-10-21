using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using gitko.CLI.Objects;

namespace gitko.CLI;
using Index = gitko.CLI.Objects.Index;


public class CommitCommand : Command
{
    public override bool NeedsKey => true;

    public CommitCommand(string[] args) : base(args)
    {
        
    }

    public override bool ValidateArguments()
    {
        if (args.Length == 2 && args[0] == "-m")
            return true;

        Console.Error.WriteLine("Korišćenje: gitko commit -m <poruka>");
        return false;
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

        tree.TreeEntries = tree.TreeEntries
            .OrderBy(e => e.Name, StringComparer.Ordinal)
            .ToList();

        tree.Save();
        return tree.Hash;
    }
 

  
    public override void Run()
    {
        Index index = new Index();
        index.Load();

        string lastHash = Refs.CurrentCommit();
        if (string.IsNullOrEmpty(lastHash) && index.IsEmpty())
        {
            Console.WriteLine("Ne postoje promene za commitovanje");
            return;
        }

        string treeHash = RecursiveTree("", index.Entries);
        Commit commit = new();

        if (string.IsNullOrEmpty(lastHash))
        {
            commit.Parent = null;                   
        }
        else
        {
            Commit last = Commit.Load(lastHash);

            if (last.Tree == treeHash)
            {
                Console.WriteLine("Ne postoje promene za commitovanje");
                return;
            }

            commit.Parent = last.Hash;                 
        }

        commit.Tree = treeHash;
        commit.Message = args[1].Replace("\"", "");
        commit.Timestamp = DateTime.Now;

        string? author = Config.LoadAuthor();
        if (author != null)
            commit.Author = author;

        commit.Save();
  
        if (WriteToBranch(commit.Hash))
        {
            Console.WriteLine($"Uspešno komitovanje. Hash: {commit.Hash}");
        }
        else
        {
            throw new Exception("Commit sačuvan, ali HEAD nije ažuriran");
        }
    }


    public bool WriteToBranch(string hash)
    {
        string branchPath = Refs.CurrentBranchPath();
        File.WriteAllText(branchPath, hash);
        return File.ReadAllText(branchPath) == hash;
    }

   
}
