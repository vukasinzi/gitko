using gitko.CLI.Objects;

namespace gitko.CLI;

public class ResetCommand : Command
{
    public ResetCommand(string[] args) : base(args)
    {
    }

    public override bool ValidateArguments()
    {
        if (args.Length != 1)
            return false;
        return true;
    }

    public override void Run()
    {
   
        
        Tree mainTree = new();
        Commit commit = Helper.LoadThatCommit(args[0]);
        if (string.IsNullOrWhiteSpace(commit.Tree))
            throw new ArgumentException("Komit nije pronađen");
        Helper.WipeOut();
        mainTree.Load(commit.Tree);
        mainTree.Rebuild(Helper.LocateRootDirectory());
        
        Helper.ClearIndex();
        string branchPath = Helper.GetBranchPath();
        File.WriteAllText(branchPath, commit.Hash);//promenjen hash u branchu
     

    }
}