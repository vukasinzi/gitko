namespace gitko.CLI;

public class InitCommand : Command
{
    public InitCommand(string[] args)
    {
        this.args = args;
    }
    public override bool ValidateArguments(string[] arguments)
    {
        if(args.Length != 0)
            throw new ArgumentException("Init zahteva 0 argumenata");
        return true;
    }

    public override void Run(string[] arguments)
    {
        string currentDir = Directory.GetCurrentDirectory();
        string minigitDir = Path.Combine(currentDir, ".gitko");

        if (Directory.Exists(minigitDir))
        {
            Console.WriteLine("Ponovo inicijalizovan .gitko direktorijum u: " + minigitDir);
            return;
        }

        Directory.CreateDirectory(minigitDir);
        Directory.CreateDirectory(Path.Combine(minigitDir, "objects"));
        Directory.CreateDirectory(Path.Combine(minigitDir, "refs", "heads"));

        File.WriteAllText(Path.Combine(minigitDir, "HEAD"), "ref: refs/heads/main");
        File.WriteAllText(Path.Combine(minigitDir, "index"), "");

        Console.WriteLine($"Inicijalizovan .gitko direktorijum u: {minigitDir}");
    }
}