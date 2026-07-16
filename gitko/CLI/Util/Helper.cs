using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using gitko.CLI.Objects;
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
}
