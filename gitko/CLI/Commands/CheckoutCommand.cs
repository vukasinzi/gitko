using System.Reflection.Metadata;
using gitko.CLI.Objects;

namespace gitko.CLI;

public class CheckoutCommand : Command
{
    public CheckoutCommand(string[] args) : base(args)
    {
    }

    public override bool ValidateArguments()
    {
        //gitko checkout -b name
        if (args.Length == 1)
            return true;
        if (args.Length == 2 && args[0] == "-b")
            return true;
        return false;

    }

    private void HandleArgs(string name,string branch)
    {
        if (!File.Exists(branch) && args[0] == "-b")
        {
            string currentHash = Helper.LoadLastCommit();
            File.WriteAllText(branch, currentHash);
            Console.WriteLine("Uspešno kreirana grana.");
        }
        else if (!File.Exists(branch) && args[0] != "-b")
        {
            throw new ApplicationException("Grana ne postoji.");
        }
        else if(File.Exists(branch) && args[0] == "-b")
        {
            throw new ApplicationException("Grana već postoji. Nemoguće je kreirati je");
        }
        
        string head = Path.Combine(Helper.LocateRootDirectory(), ".gitko", "HEAD");
        File.WriteAllText(head,"ref: refs/heads/"+name);
       
        Console.WriteLine("Trenutna aktivna grana -> " + name);
    }
    public override void Run()
    {
         string name = args.Length == 2 ? args[1] : args[0];
        string branch = Path.Combine(Helper.LocateRootDirectory(), ".gitko", "refs","heads",name);
       HandleArgs(name,branch);
       string lastHash = File.ReadAllText(branch);
       if (string.IsNullOrEmpty(lastHash))
           return;
       
       //ovde krece checkout
       Tree mainTree = new();
        Commit lastCommit = Helper.LoadThatCommit(lastHash);
        mainTree.Load(lastCommit.Tree);
        mainTree.Rebuild(Helper.LocateRootDirectory());
        
    }
}