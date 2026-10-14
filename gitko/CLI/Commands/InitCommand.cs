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

    }
    public override void Run()
    {
       
        if (Directory.Exists(gitkoDir))
        {
            Directory.Delete(gitkoDir, true);
            Initialize();
            Console.WriteLine("Ponovo inicijalizovan .gitko direktorijum u: " + gitkoDir);
            return;
        }

        Initialize();
        Console.WriteLine($"Inicijalizovan .gitko direktorijum u: {gitkoDir}");
    }
}