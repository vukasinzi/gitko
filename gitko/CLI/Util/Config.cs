using Microsoft.Extensions.Configuration;

namespace gitko.CLI;

public static class Config
{
    public static string AuthorPath
    {
        get { return Path.Combine(Repository.GitkoDir, "author"); }
    }

    public static string? Version()
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();
        return config["Version"];
    }

    public static string? LoadAuthor()
    {
        string path = AuthorPath;
        if (!File.Exists(path))
            return null;

        string author = File.ReadAllText(path).Trim();
        return string.IsNullOrWhiteSpace(author) ? null : author;
    }
}
