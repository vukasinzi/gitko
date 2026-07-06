using System.ComponentModel.DataAnnotations;

namespace gitko.CLI;

public abstract class Command
{
    [Required]
    public string[] args;
    
    public void Execute()
    {
        try
        {
            if (!ValidateArguments(args))
                return;
            Run(args);

        }
        catch (Exception x)
        {
            Console.WriteLine(x.Message);
        }
    }

    public abstract void Run(string [] arguments);
  

    public virtual bool ValidateArguments(string[] arguments)
    {
        return true;
    }
}