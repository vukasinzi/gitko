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
        return false;
    }

    public override void Run()
    {
        string root = Helper.LocateRootDirectory();
        string headsDir = Path.Combine(root, ".gitko", "refs", "heads");

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

        string current = Path.GetFileName(Helper.GetBranchPath());

        foreach (string file in Directory.GetFiles(headsDir))
        {
            string name = Path.GetFileName(file);
            string marker = name == current ? "* " : "  ";
            Console.WriteLine(marker + name);
        }
    }

    private void DeleteBranch(string name, string headsDir)
    {
        string path = Path.Combine(headsDir, name);

        if (!File.Exists(path))
            throw new ApplicationException("Grana ne postoji.");

        if (name == Path.GetFileName(Helper.GetBranchPath()))
            throw new ApplicationException("Nemoguće je obrisati trenutno aktivnu granu.");

        File.Delete(path);
        Console.WriteLine("Obrisana grana -> " + name);
    }
}