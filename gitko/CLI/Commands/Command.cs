using gitko.CLI;

public abstract class Command
{
    public string[] args { get; }

    protected Command(string[] args)
    {
        this.args = args;
    }

    public void Execute()
    {
        try
        {
            if (!ValidateArguments())
            {
                Environment.ExitCode = 1;
                return;
            }
            if (NeedsKey)
                RepoKey.Unlock();
            Run();
        }
        catch (GitkoException x)      // očekivano: korisnik je pogrešio
        {
            Console.Error.WriteLine(x.Message);
            Environment.ExitCode = 1;
        }
        catch (Exception x)           // neočekivano: bag ili problem sa diskom
        {
            Console.Error.WriteLine("Neočekivana greška: " + x.Message);
            Environment.ExitCode = 1;
        }
    }

    public abstract void Run();

    public virtual bool NeedsKey => false;

    public virtual bool ValidateArguments()
    {
        return true;
    }
}