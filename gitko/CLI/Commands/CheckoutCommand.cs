using System.Reflection.Metadata;
using gitko.CLI.Objects;

namespace gitko.CLI;

public class CheckoutCommand : Command
{
    public CheckoutCommand(string[] args) : base(args)
    {
    }

    public override bool ValidateArguments()
    {
        //gitko checkout -b name
        if (args.Length == 1)
            return true;
        if (args.Length == 2 && args[0] == "-b")
            return true;
        return false;

    }

    public override void Run()
    {
        string name = args.Length == 2 ? args[1] : args[0];
        Helper.ValidateBranchName(name);
        string branch = Path.Combine(Helper.LocateRootDirectory(), ".gitko", "refs", "heads", name);

        if (args[0] == "-b")
        {
            CreateBranch(branch);
            WriteHead(name);
            return;
        }

        if (!File.Exists(branch))
            throw new ApplicationException("Grana ne postoji.");

        if (name == Path.GetFileName(Helper.GetBranchPath()))
        {
            Console.WriteLine("Grana je već aktivna -> " + name);
            return;
        }

        //ovde krece checkout
        string lastHash = File.ReadAllText(branch);
        if (!string.IsNullOrEmpty(lastHash))
        {
            Commit lastCommit = Helper.LoadThatCommit(lastHash);
            Helper.RestoreTree(lastCommit.Tree);
        }

        WriteHead(name);
    }

    private void CreateBranch(string branch)
    {
        if (File.Exists(branch))
            throw new ApplicationException("Grana već postoji. Nemoguće je kreirati je");

        File.WriteAllText(branch, Helper.LoadLastCommit());
        Console.WriteLine("Uspešno kreirana grana.");
    }

    private void WriteHead(string name)
    {
        string head = Path.Combine(Helper.LocateRootDirectory(), ".gitko", "HEAD");
        File.WriteAllText(head, "ref: refs/heads/" + name);
        Console.WriteLine("Trenutna aktivna grana -> " + name);
    }
}