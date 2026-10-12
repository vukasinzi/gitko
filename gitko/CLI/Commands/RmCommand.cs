using Index = gitko.CLI.Objects.Index;

namespace gitko.CLI;

public class RmCommand : Command
{
    private static readonly string[] Flags = { "-r", "--cached" };

    public RmCommand(string[] args) : base(args) { }

    public override bool ValidateArguments()
    {
        string[] flags = args.Where(a => a.StartsWith("-")).ToArray();
        string[] paths = args.Where(a => !a.StartsWith("-")).ToArray();

        if (paths.Length == 1 && flags.All(f => Flags.Contains(f)))
            return true;

        Console.WriteLine("Korišćenje: gitko rm [-r] [--cached] <putanja>");
        return false;
    }

    public override void Run()
    {
        bool recursive = args.Contains("-r");
        bool cached = args.Contains("--cached");
        string path = Helper.LocatePath(args.First(a => !a.StartsWith("-")));

        if (!Helper.IsInsideRootDirectory(path))
            throw new ArgumentException("Fajl mora biti unutar gitko repozitorijuma");

        string rootPath = Helper.LocateRootDirectory();
        string relativePath = Path.GetRelativePath(rootPath, path).Replace("\\", "/");

        Index index = new Index();
        index.Load();

        List<string> matches = index.PathsUnder(relativePath);
        if (matches.Count == 0)
            throw new ApplicationException("Fajl nije praćen: " + relativePath);

        if (!recursive && !index.Entries.ContainsKey(relativePath))
            throw new ApplicationException($"{relativePath} je folder. Za brisanje foldera koristi -r.");

        foreach (string entry in matches)
        {
            index.Remove(entry);

            if (cached)
            {
                Console.WriteLine($"Uklonjen {entry} (ostaje na disku)");
                continue;
            }

            Helper.DeleteFile(Path.Combine(rootPath, entry));
            Console.WriteLine($"Uklonjen {entry}");
        }

        index.Save();
    }
}
