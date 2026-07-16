namespace gitko.CLI;

public static class Parser
{
     
    public static void ParseAndExecute(string[] args)
    {
        if (args.Length == 0)
            return;
        
        string command = args[0];
        string[] arguments = args[1..];

        switch (command)
        {
            case "help":
                break;
            case "version":
                Console.WriteLine("Trenutna verzija gitka je "+ Helper.LoadConfig()["Version"]?.ToString());
                break;
            case "init":
                new InitCommand(arguments).Execute();
                break;
            case "add":
                new AddCommand(arguments).Execute();
                break;
            case "commit":
                new CommitCommand(arguments).Execute();
                break;
            case "log":
                new LogCommand(arguments).Execute();
                break;
            case "checkout":
                new CheckoutCommand(arguments).Execute();
                break;
            default:
                Console.WriteLine("Korišćenje: gitko <komanda> [argumenti]");
                break;
        }
    }
}