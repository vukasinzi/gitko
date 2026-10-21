namespace gitko.CLI;

public static class Parser
{
     
    public static void ParseAndExecute(string[] args)
    {
        if (args.Length == 0)
        {
            PrintHelp();
            return;
        }

        string command = args[0];
        string[] arguments = args[1..];

        switch (command)
        {
            case "help":
                PrintHelp();
                break;
            case "version":
                Console.WriteLine("Trenutna verzija gitka je "+ Config.Version());
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
            case "reset":
                new ResetCommand(arguments).Execute();
                break;
            case "branch":
                new BranchCommand(arguments).Execute();
                break;
            case "author":
                new AuthorCommand(arguments).Execute();
                break;
            case "rm":
                new RmCommand(arguments).Execute();
                break;
            case "merge":
                new MergeCommand(arguments).Execute();
                break;
            default:
                Console.Error.WriteLine($"Nepoznata komanda: {command}. Pokreni gitko help.");
                Environment.ExitCode = 1;
                break;
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine("""
            Korišćenje: gitko <komanda> [argumenti]

              init                     napravi novi repozitorijum
              add <putanja>            dodaj fajl ili folder u index
              rm [-r] [--cached] <p>   ukloni fajl iz indexa
              commit -m <poruka>       commituj index
              log                      prikaži istoriju
              checkout [-b] <grana>    pređi na granu (-b: napravi novu)
              branch [-d <grana>]      prikaži grane (-d: obriši)
              merge <grana>            spoji granu u trenutnu
              reset <hash>             vrati granu na commit
              author [-set <ime>]      prikaži ili postavi autora
              version                  prikaži verziju

            init traži novu lozinku dva puta.
            add, commit, merge, log, checkout i reset traže lozinku pri svakom pokretanju.
            """);
    }
}