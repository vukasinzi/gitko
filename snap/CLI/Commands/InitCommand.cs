namespace snap.CLI;

public class InitCommand : Command
{
    private string snapDir;

    public InitCommand(string[] args) : base(args)
    {
        string currentDir = Directory.GetCurrentDirectory();
        snapDir = Path.Combine(currentDir, ".snap");

    }


    public override bool ValidateArguments()
    {
        if (args.Length == 0)
            return true;

        Console.Error.WriteLine("Korišćenje: snap init");
        return false;
    }
    

    public void Initialize()
    {
        Directory.CreateDirectory(snapDir);
        Directory.CreateDirectory(Path.Combine(snapDir, "objects"));
        Directory.CreateDirectory(Path.Combine(snapDir, "refs", "heads"));

        File.WriteAllText(Path.Combine(snapDir, "HEAD"), "ref: refs/heads/main");
        File.WriteAllText(Path.Combine(snapDir, "index"), "{}");
        File.WriteAllText(Path.Combine(snapDir, "refs", "heads", "main"), "");

    }
    public override void Run()
    {
        string password = RepoKey.AskNewPassword();

        if (Directory.Exists(snapDir))
        {
            Directory.Delete(snapDir, true);
            Initialize();
            RepoKey.Create(password);
            Console.WriteLine("Ponovo inicijalizovan .snap direktorijum u: " + snapDir);
            return;
        }

        Initialize();
        RepoKey.Create(password);
        Console.WriteLine($"Inicijalizovan .snap direktorijum u: {snapDir}");
    }
}