using gitko.CLI.Objects;

namespace gitko.CLI;

public class LogCommand : Command
{
    public override bool NeedsKey => true;

    public LogCommand(string[] args) : base(args)
    {
    }

    public override bool ValidateArguments()
    {
        if (args.Length == 0)
            return true;

        Console.Error.WriteLine("Korišćenje: gitko log");
        return false;
    }

    public override void Run()
    {
        string? lastHash = Refs.CurrentCommit();
        if (string.IsNullOrEmpty(lastHash))
        {
            Console.WriteLine("Root direktorijum nema komitove.");
            return;
        }

        PrintHistory(lastHash);
    }

    private void PrintHistory(string? hash)
    {
        while (!string.IsNullOrEmpty(hash))
        {
            Commit commit = Commit.Load(hash);
            Console.WriteLine(commit);
            hash = commit.Parent;
        }
    }
}
