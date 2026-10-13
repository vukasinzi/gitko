namespace gitko.CLI;

public class MergeCommand : Command
{
    public MergeCommand(string[] args) : base(args)
    {
    }

    public override bool ValidateArguments()
    {
        if (args.Length == 1)
            return true;

        Console.WriteLine("Korišćenje: gitko merge <grana>");
        return false;
    }

    public override void Run()
    {
        string otherBranchName = args[0];
        Helper.ValidateBranchName(otherBranchName);

        string currentHash = Helper.LoadLastCommit();
        if (string.IsNullOrEmpty(currentHash))
            throw new GitkoException("Trenutna grana nema komitove.");
        string otherHash = LoadBranchHash(otherBranchName);
        
        if (IsBranchUpToDate(currentHash, otherHash))
            throw new GitkoException($"Grana je već ažurna: sadrži sve komitove sa grane {otherBranchName}.");
        
    }

    // Da li se poslednji komit naše grane nalazi u istoriji druge grane (npr. features),
    // tj. da li je main grana menjana ili ne, dok je features menjana.
    // Primer: main: A, features: A <- B

    private bool IsFastForward(string currentHash, string otherHash)
    {
        string hash = currentHash;
        
        while (!string.IsNullOrEmpty(hash))
        {
            if (hash == otherHash)
                return true;
            hash = Helper.LoadThatCommit(hash).Parent;
        }

        return false;
    }

    private bool IsBranchUpToDate(string currentHash, string otherHash)
    {
        string hash = currentHash;
        while (!string.IsNullOrEmpty(hash))
        {
            if (hash == otherHash)
                return true;
            hash = Helper.LoadThatCommit(hash).Parent;
        }
        return false;
    }

    private string LoadBranchHash(string name)
    {
        string path = Path.Combine(Helper.LocateRootDirectory(), ".gitko", "refs", "heads", name);
        if (!File.Exists(path))
            throw new GitkoException("Grana ne postoji.");

        string hash = File.ReadAllText(path).Trim();
        if (string.IsNullOrEmpty(hash))
            throw new GitkoException($"Grana {name} nema komitove.");
        return hash;
    }
}