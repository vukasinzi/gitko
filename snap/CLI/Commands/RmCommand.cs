using Index = snap.CLI.Objects.Index;

namespace snap.CLI;

public class RmCommand : Command
{
    private static readonly string[] Flags = { "-r", "--cached" };

    public RmCommand(string[] args) : base(args) { }

    public override bool ValidateArguments()
    {
        int pathCount = 0;
        bool unknownFlag = false;
        foreach (string arg in args)
        {
            if (!arg.StartsWith("-"))
                pathCount++;
            else if (!Flags.Contains(arg))
                unknownFlag = true;
        }

        if (pathCount == 1 && !unknownFlag)
            return true;

        Console.Error.WriteLine("Korišćenje: snap rm [-r] [--cached] <putanja>");
        return false;
    }

    public override void Run()
    {
        bool recursive = args.Contains("-r");
        bool cached = args.Contains("--cached");
        string path = Repository.FullPath(args.First(a => !a.StartsWith("-")));

        if (!Repository.Contains(path))
            throw new SnapException("Fajl mora biti unutar snap repozitorijuma");

        string rootPath = Repository.Root;
        string relativePath = Repository.RelativePath(path);

        Index index = new Index();
        index.Load();

        List<string> matches = index.PathsUnder(relativePath);
        if (matches.Count == 0)
            throw new SnapException("Fajl nije praćen: " + relativePath);

        if (!recursive && !index.Entries.ContainsKey(relativePath))
            throw new SnapException($"{relativePath} je folder. Za brisanje foldera koristi -r.");

        foreach (string entry in matches)
        {
            index.Remove(entry);

            if (cached)
            {
                Console.WriteLine($"Uklonjen {entry} (ostaje na disku)");
                continue;
            }

            WorkingDirectory.DeleteFile(Path.Combine(rootPath, entry));
            Console.WriteLine($"Uklonjen {entry}");
        }

        index.Save();
    }
}
