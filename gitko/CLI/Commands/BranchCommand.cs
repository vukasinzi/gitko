namespace gitko.CLI;

public class BranchCommand : Command
{
    public BranchCommand(string[] args) : base(args)
    {
    }

    public override bool ValidateArguments()
    {
        if (args.Length == 0)
            return true;
        if (args.Length == 2 && args[0] == "-d")
            return true;

        Console.Error.WriteLine("Korišćenje: gitko branch [-d <grana>]");
        return false;
    }

    public override void Run()
    {
        string headsDir = Repository.HeadsDir;

        if (args.Length == 2 && args[0] == "-d")
        {
            DeleteBranch(args[1], headsDir);
            return;
        }

        ListBranches(headsDir);
    }

    private void ListBranches(string headsDir)
    {
        if (!Directory.Exists(headsDir))
            return;

        string current = Refs.CurrentBranchName();

        foreach (string file in Directory.GetFiles(headsDir))
        {
            string name = Path.GetFileName(file);
            string marker = name == current ? "* " : "  ";
            Console.WriteLine(marker + name);
        }
    }

    private void DeleteBranch(string name, string headsDir)
    {
        Refs.ValidateName(name);
        string path = Path.Combine(headsDir, name);

        if (!File.Exists(path))
            throw new GitkoException("Grana ne postoji.");

        if (name == Refs.CurrentBranchName())
            throw new GitkoException("Nemoguće je obrisati trenutno aktivnu granu.");

        File.Delete(path);
        Console.WriteLine("Obrisana grana -> " + name);
    }
}