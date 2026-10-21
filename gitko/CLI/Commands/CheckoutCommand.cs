using gitko.CLI.Objects;

namespace gitko.CLI;

public class CheckoutCommand : Command
{
    public override bool NeedsKey => true;

    public CheckoutCommand(string[] args) : base(args)
    {
    }

    public override bool ValidateArguments()
    {
        if (args.Length == 1 && args[0] != "-b")
            return true;
        if (args.Length == 2 && args[0] == "-b")
            return true;

        Console.Error.WriteLine("Korišćenje: gitko checkout [-b] <grana>");
        return false;
    }

    public override void Run()
    {
        string name = args[^1];
        bool createNew = args[0] == "-b";

        Refs.ValidateName(name);

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

        File.WriteAllText(refPath, Refs.CurrentCommit());
        Console.WriteLine("Uspešno kreirana grana.");

        PointHeadAt(name);
    }

    private void SwitchToBranch(string name)
    {
        string refPath = BranchRefPath(name);
        if (!File.Exists(refPath))
            throw new GitkoException("Grana ne postoji.");

        bool alreadyOnBranch = name == Refs.CurrentBranchName();
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

        Commit commit = Commit.Load(commitHash);
        WorkingDirectory.Restore(commit.Tree);
    }

    private void PointHeadAt(string name)
    {
        string headPath = Repository.HeadPath;
        File.WriteAllText(headPath, "ref: refs/heads/" + name);
        Console.WriteLine("Trenutna aktivna grana -> " + name);
    }

    private string BranchRefPath(string name)
    {
        return Path.Combine(Repository.HeadsDir, name);
    }
}
