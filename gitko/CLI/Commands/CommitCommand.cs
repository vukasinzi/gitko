using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using gitko.CLI.Objects;

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

        tree.Save();
        return tree.Hash;
    }
 

  
    public override void Run()
    {
        Index index = new Index();
        index.Load();

        string treeHash = RecursiveTree("", index.Entries);
        Commit commit = new();

        string lastHash = Helper.LoadLastCommit();
  
        if (string.IsNullOrEmpty(lastHash))
        {
            commit.Parent = null;                   
        }
        else
        {
            Commit last = Helper.LoadThatCommit(lastHash);

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
        string branchPath = Helper.GetBranchPath();
        File.WriteAllText(branchPath, hash);
        return File.ReadAllText(branchPath) == hash;
    }

   
}
