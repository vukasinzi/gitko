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
                return;
            Run();
        }
        catch (Exception x)
        {
            Console.WriteLine(x.Message);
        }
    }

    public abstract void Run();

    public virtual bool ValidateArguments()
    {
        return true;
    }
}