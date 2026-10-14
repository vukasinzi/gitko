using System.Text.RegularExpressions;

namespace gitko.CLI;

public static class Refs
{
    public static string CurrentBranchPath()
    {
        string head = File.ReadAllText(Repository.HeadPath).Trim();
        string refPath = head[5..];
        return Path.Combine(Repository.GitkoDir, refPath);
    }

    public static string CurrentBranchName()
    {
        return Path.GetFileName(CurrentBranchPath());
    }

    public static string CurrentCommit()
    {
        string path = CurrentBranchPath();
        if (!File.Exists(path))
            return null;
        return File.ReadAllText(path).Trim();
    }

    public static void ValidateName(string name)
    {
        if (!Regex.IsMatch(name, @"^[\p{L}\p{N}_][\p{L}\p{N}._-]*$"))
            throw new GitkoException("Neispravno ime grane: " + name);
    }
}
