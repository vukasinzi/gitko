using System.Security.Cryptography;
using System.Text;

namespace gitko.CLI;

public static class RepoKey
{
    private const int Iterations = 600_000;

    public static byte[]? NameKey { get; private set; }
    public static byte[]? EncKey { get; private set; }

    public static string AskNewPassword()
    {
        string password = ReadPassword("Lozinka: ");
        if (password == "")
            throw new GitkoException("Lozinka ne može biti prazna.");

        string repeated = ReadPassword("Ponovi lozinku: ");
        if (password != repeated)
            throw new GitkoException("Lozinke se ne poklapaju.");

        return password;
    }

    public static void Create(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        File.WriteAllBytes(Repository.SaltPath, salt);

        byte[] key = Derive(password, salt);
        File.WriteAllBytes(Repository.KeyCheckPath, KeyCheck(key));
    }

    public static void Unlock()
    {
        if (!File.Exists(Repository.SaltPath) || !File.Exists(Repository.KeyCheckPath))
            throw new GitkoException("Repozitorijum nema lozinku. Pokreni gitko init.");

        string password = ReadPassword("Lozinka: ");
        byte[] salt = File.ReadAllBytes(Repository.SaltPath);
        byte[] key = Derive(password, salt);

        byte[] expected = File.ReadAllBytes(Repository.KeyCheckPath);
        if (!CryptographicOperations.FixedTimeEquals(KeyCheck(key), expected))
            throw new GitkoException("Pogrešna lozinka.");

        NameKey = HKDF.Expand(HashAlgorithmName.SHA256, key, 32, Encoding.UTF8.GetBytes("gitko-name"));
        EncKey = HKDF.Expand(HashAlgorithmName.SHA256, key, 32, Encoding.UTF8.GetBytes("gitko-enc"));
    }

    private static byte[] Derive(string password, byte[] salt)
    {
        return Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, 32);
    }

    private static byte[] KeyCheck(byte[] key)
    {
        return HMACSHA256.HashData(key, Encoding.UTF8.GetBytes("gitko"));
    }

    private static string ReadPassword(string prompt)
    {
        Console.Error.Write(prompt);

        if (Console.IsInputRedirected)
            return Console.ReadLine() ?? "";

        StringBuilder password = new();
        while (true)
        {
            ConsoleKeyInfo key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.Enter)
                break;

            if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password.Length--;
                    Console.Error.Write("\b \b");
                }
                continue;
            }

            if (char.IsControl(key.KeyChar))
                continue;

            password.Append(key.KeyChar);
            Console.Error.Write('*');
        }

        Console.Error.WriteLine();
        return password.ToString();
    }
}
