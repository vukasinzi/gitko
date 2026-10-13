using gitko.CLI.Objects;

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

        Console.WriteLine("Korišćenje: gitko merge <grana>");
        return false;
    }

    public override void Run()
    {
        string otherBranchName = args[0];
        Helper.ValidateBranchName(otherBranchName);

        string currentHash = Helper.LoadLastCommit();
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
        
    }

    private void FastForward(string otherHash)
    {
        Commit commit = Helper.LoadThatCommit(otherHash);
        Helper.RestoreTree(commit.Tree);
        File.WriteAllText(Helper.GetBranchPath(), otherHash);
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
            hash = Helper.LoadThatCommit(hash).Parent;
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
            hash = Helper.LoadThatCommit(hash).Parent;
        }
        return false;
    }

    private string LoadBranchHash(string name)
    {
        string path = Path.Combine(Helper.LocateRootDirectory(), ".gitko", "refs", "heads", name);
        if (!File.Exists(path))
            throw new GitkoException("Grana ne postoji.");

        string hash = File.ReadAllText(path).Trim();
        if (string.IsNullOrEmpty(hash))
            throw new GitkoException($"Grana {name} nema komitove.");
        return hash;
    }
}