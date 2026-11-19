using snap.CLI.Objects;

namespace snap.CLI;

public class ResetCommand : Command
{
    public override bool NeedsKey => true;

    public ResetCommand(string[] args) : base(args)
    {
    }

    public override bool ValidateArguments()
    {
        if (args.Length == 1)
            return true;

        Console.Error.WriteLine("Korišćenje: snap reset <hash>");
        return false;
    }

    public override void Run()
    {
        Commit commit = Commit.Load(args[0]);
        
        if (string.IsNullOrWhiteSpace(commit.Tree))
            throw new SnapException("Komit nije pronađen");
        
        WorkingDirectory.Restore(commit.Tree);
        string branchPath = Refs.CurrentBranchPath();
        File.WriteAllText(branchPath, commit.Hash);//promenjen hash u branchu
     

    }
}