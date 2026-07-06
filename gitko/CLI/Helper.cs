using Microsoft.Extensions.Configuration;

namespace gitko.CLI;

public static class Helper
{
    public static IConfiguration UcitajConfig()
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();
        return config;

    }
}