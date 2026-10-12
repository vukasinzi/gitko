using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using gitko.CLI.Objects;
using Index = gitko.CLI.Objects.Index;
using Microsoft.Extensions.Configuration;

namespace gitko.CLI;

public static class Helper
{
    public static IConfiguration LoadConfig()
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();
        return config;

    }

    public static string Hash(byte[] input)
    {
        using var sha = SHA256.Create();
        byte[] hashBytes = sha.ComputeHash(input);
        return ToHexString(hashBytes);
    }

    public static string Hash(string input)
    {
        return Hash(Encoding.UTF8.GetBytes(input));
    }
    private static string ToHexString(byte[] bytes)
    {
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (byte b in bytes)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }



    public static string LocateRootDirectory()
    {
        string? currentDir = Directory.GetCurrentDirectory();
    
        while (currentDir != null)
        {
            if (Directory.Exists(Path.Combine(currentDir, ".gitko")))
                return currentDir;
        
            currentDir = Path.GetDirectoryName(currentDir); 
        }

        throw new Exception("Ne mogu naći root direktorijum!");
    }

    public static string LocatePath(string arg)
    {
        string currentDir = Directory.GetCurrentDirectory();
        string combined = Path.Combine(currentDir, arg);
        return Path.GetFullPath(combined);
        
    }

    public static bool IsInsideRootDirectory(string path)
    {
        string rootPath = Path.GetFullPath(LocateRootDirectory())
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        string fullPath = Path.GetFullPath(path)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        StringComparison comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        return string.Equals(fullPath, rootPath, comparison) ||
               fullPath.StartsWith(rootPath + Path.DirectorySeparatorChar, comparison) ||
               fullPath.StartsWith(rootPath + Path.AltDirectorySeparatorChar, comparison);
    }
    public static string GetBranchPath()
    {
        var root = Helper.LocateRootDirectory();
        string headPath = Path.Combine(root, ".gitko", "HEAD");
        string data = File.ReadAllText(headPath).Trim();
        data = data[5..];

        return Path.Combine(root, ".gitko", data);
    }
    public static void ValidateBranchName(string name)
    {
        if (!Regex.IsMatch(name, @"^[\p{L}\p{N}_][\p{L}\p{N}._-]*$"))
            throw new ApplicationException("Neispravno ime grane: " + name);
    }
    public static string GetAuthorPath()
    {
        var root = LocateRootDirectory();
        return Path.Combine(root, ".gitko", "author");
    }
    public static string? LoadAuthor()
    {
        string path = GetAuthorPath();
        if (!File.Exists(path))
            return null;

        string author = File.ReadAllText(path).Trim();
        return string.IsNullOrWhiteSpace(author) ? null : author;
    }
    public static string LoadLastCommit()
    {
        string path = GetBranchPath();
        if (!File.Exists(path))
            return null;
        return File.ReadAllText(path).Trim();
    }

    public static Commit LoadThatCommit(string lastHash)
    {
        string part1 = lastHash[0..2];
        string part2 = lastHash[2..];

        var root = Helper.LocateRootDirectory();
        var file = Path.Combine(root, ".gitko", "objects", part1, part2);

        byte[] fullBytes = File.ReadAllBytes(file);
        int nullIndex = Array.IndexOf(fullBytes, (byte)0);
        byte[] jsonBytes = fullBytes[(nullIndex + 1)..];//micemo header commit /0
        string json = Encoding.UTF8.GetString(jsonBytes);

        Commit commit = JsonSerializer.Deserialize<Commit>(json);
        if(commit == null)
            throw new NullReferenceException("Poslednji komit nije pronađen");
        commit.Hash = lastHash;
        return commit;
    }
    public static void RestoreTree(string treeHash)
    {
        string root = LocateRootDirectory();

        Tree tree = new();
        tree.Load(treeHash);

        Index index = new Index();
        index.Load();
        foreach (string path in index.Entries.Keys)
            DeleteFile(Path.Combine(root, path));

        tree.Rebuild(root);

        index.LoadFromTree(treeHash);
        index.Save();
    }
    public static void DeleteFile(string fullPath)
    {
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        string root = LocateRootDirectory();
        string? dir = Path.GetDirectoryName(fullPath);
        while (dir != null && dir != root && Directory.Exists(dir)
               && !Directory.EnumerateFileSystemEntries(dir).Any())
        {
            Directory.Delete(dir);
            dir = Path.GetDirectoryName(dir);
        }
    }
}
