using gitko.CLI.Objects;

namespace gitko.CLI;

public class LogCommand :Command
{
    public LogCommand(string[] args) : base(args)
    {
    }

    public override bool ValidateArguments()
    {
        if (args.Length == 0)
            return true;
        return false;
    }

    public override void Run()
    {
        string lastHash = Helper.LoadLastCommit();
        if (string.IsNullOrEmpty(lastHash))
        {
            Console.WriteLine("Root direktorijum nema komitove.");
            return;
        }
        do
        {
            Commit c = new Commit();
            c.Load(lastHash);
            Console.WriteLine(c.ToString());
            lastHash = c.Parent;
            
        }while(!string.IsNullOrEmpty(lastHash));
    }
}