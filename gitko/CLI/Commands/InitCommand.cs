namespace gitko.CLI;

public class InitCommand : Command
{
    private string gitkoDir;

    public InitCommand(string[] args) : base(args)
    {
        string currentDir = Directory.GetCurrentDirectory();
        gitkoDir = Path.Combine(currentDir, ".gitko");

    }


    public override bool ValidateArguments()
    {
        if (args.Length == 0)
            return true;

        Console.Error.WriteLine("Korišćenje: gitko init");
        return false;
    }
    

    public void Initialize()
    {
        Directory.CreateDirectory(gitkoDir);
        Directory.CreateDirectory(Path.Combine(gitkoDir, "objects"));
        Directory.CreateDirectory(Path.Combine(gitkoDir, "refs", "heads"));

        File.WriteAllText(Path.Combine(gitkoDir, "HEAD"), "ref: refs/heads/main");
        File.WriteAllText(Path.Combine(gitkoDir, "index"), "{}");
        File.WriteAllText(Path.Combine(gitkoDir, "refs", "heads", "main"), "");

    }
    public override void Run()
    {
        string password = RepoKey.AskNewPassword();

        if (Directory.Exists(gitkoDir))
        {
            Directory.Delete(gitkoDir, true);
            Initialize();
            RepoKey.Create(password);
            Console.WriteLine("Ponovo inicijalizovan .gitko direktorijum u: " + gitkoDir);
            return;
        }

        Initialize();
        RepoKey.Create(password);
        Console.WriteLine($"Inicijalizovan .gitko direktorijum u: {gitkoDir}");
    }
}