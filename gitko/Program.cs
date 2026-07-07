using gitko.CLI;

namespace gitko;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: gitko <command> [arguments]");
            return;
        }

        Parser.ParseAndExecute(args);
    }
}