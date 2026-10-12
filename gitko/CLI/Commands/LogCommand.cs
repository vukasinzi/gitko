using gitko.CLI.Objects;

namespace gitko.CLI;

public class LogCommand : Command
{
    public LogCommand(string[] args) : base(args)
    {
    }

    public override bool ValidateArguments()
    {
        return args.Length == 0;
    }

    public override void Run()
    {
        string? lastHash = Helper.LoadLastCommit();
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
            Commit commit = Helper.LoadThatCommit(hash);
            Console.WriteLine(commit);
            hash = commit.Parent;
        }
    }
}
