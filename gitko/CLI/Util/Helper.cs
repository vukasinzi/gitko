using System.Security.Cryptography;
using System.Text;
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

        throw new Exception("Nemoće naći root direktorijum!");
    }

    public static string LocatePath(string arg)
    {
        string currentDir = Directory.GetCurrentDirectory();
        string combined = Path.Combine(currentDir, arg);
        return Path.GetFullPath(combined);
        
    }
}