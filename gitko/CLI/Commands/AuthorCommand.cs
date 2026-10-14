namespace gitko.CLI;

public class AuthorCommand : Command
{
    public AuthorCommand(string[] args) : base(args)
    {
    }

    public override bool ValidateArguments()
    {
        if (args.Length == 0)
            return true;
        if (args.Length >= 2 && args[0] == "-set")
            return true;

        Console.Error.WriteLine("Korišćenje: gitko author [-set <ime>]");
        return false;
    }

    public override void Run()
    {
        if (args.Length == 0)
        {
            ShowAuthor();
            return;
        }

        SetAuthor(string.Join(" ", args[1..]));
    }

    private void ShowAuthor()
    {
        string? author = Config.LoadAuthor();
        Console.WriteLine(author ?? "Autor nije podešen (Nepoznat)");
    }

    private void SetAuthor(string name)
    {
        name = name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new GitkoException("Ime autora ne može biti prazno.");

        File.WriteAllText(Config.AuthorPath, name);
        Console.WriteLine("Autor postavljen -> " + name);
    }
}
