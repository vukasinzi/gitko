using gitko.CLI.Objects;

namespace gitko.CLI;

public class CheckoutCommand : Command
{
    public CheckoutCommand(string[] args) : base(args)
    {
    }

    public override bool ValidateArguments()
    {
        return args.Length == 1 || (args.Length == 2 && args[0] == "-b");
    }

    public override void Run()
    {
        string name = args[^1];
        bool createNew = args[0] == "-b";

        Helper.ValidateBranchName(name);

        if (createNew)
            CreateBranch(name);
        else
            SwitchToBranch(name);
    }

    private void CreateBranch(string name)
    {
        string refPath = BranchRefPath(name);
        if (File.Exists(refPath))
            throw new GitkoException("Grana već postoji. Nemoguće je kreirati je");

        File.WriteAllText(refPath, Helper.LoadLastCommit());
        Console.WriteLine("Uspešno kreirana grana.");

        PointHeadAt(name);
    }

    private void SwitchToBranch(string name)
    {
        string refPath = BranchRefPath(name);
        if (!File.Exists(refPath))
            throw new GitkoException("Grana ne postoji.");

        bool alreadyOnBranch = name == Path.GetFileName(Helper.GetBranchPath());
        if (alreadyOnBranch)
        {
            Console.WriteLine("Grana je već aktivna -> " + name);
            return;
        }

        RestoreWorkingDirectory(File.ReadAllText(refPath));
        PointHeadAt(name);
    }

    private void RestoreWorkingDirectory(string commitHash)
    {
        if (string.IsNullOrEmpty(commitHash))
            return; // grana jos nema komitova

        Commit commit = Helper.LoadThatCommit(commitHash);
        Helper.RestoreTree(commit.Tree);
    }

    private void PointHeadAt(string name)
    {
        string headPath = Path.Combine(Helper.LocateRootDirectory(), ".gitko", "HEAD");
        File.WriteAllText(headPath, "ref: refs/heads/" + name);
        Console.WriteLine("Trenutna aktivna grana -> " + name);
    }

    private string BranchRefPath(string name)
    {
        return Path.Combine(Helper.LocateRootDirectory(), ".gitko", "refs", "heads", name);
    }
}
