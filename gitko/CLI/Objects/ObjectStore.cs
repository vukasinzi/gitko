using System.Security.Cryptography;
using System.Text;

namespace gitko.CLI.Objects;

public class ObjectStore
{
   private const int NonceSize = 12;
   private const int TagSize = 16;

   public string Store(byte[] data,ObjectType type)
   {
      byte[] header = Encoding.UTF8.GetBytes($"{type.ToString().ToLower()}\0");
      byte[] full = header.Concat(data).ToArray();
      
      string hashed = Hash(full);
      string folderName = hashed[0..2];
      string fileName = hashed[2..];
      
      string folderPath = Path.Combine(Repository.ObjectsDir, folderName);
      Directory.CreateDirectory(folderPath);
      
      string filePath = Path.Combine(folderPath, fileName);
      
      File.WriteAllBytes(filePath, Encrypt(full, hashed));

      return hashed;
   }

   public byte[] Read(string hash, ObjectType type)
   {
      if (hash.Length != 64)
         throw new GitkoException("Neispravan hash: " + hash);

      string filePath = Path.Combine(Repository.ObjectsDir, hash[0..2], hash[2..]);
      if (!File.Exists(filePath))
         throw new GitkoException("Objekat nije pronađen: " + hash);

      byte[] full = Decrypt(File.ReadAllBytes(filePath), hash);
      int nullIndex = Array.IndexOf(full, (byte)0);
      string header = Encoding.UTF8.GetString(full, 0, nullIndex);
      string expected = type.ToString().ToLower();
      if (header != expected)
         throw new GitkoException($"{hash} nije {expected}, nego {header}.");

      return full[(nullIndex + 1)..];
   }

   private static string Hash(byte[] input)
   {
      byte[] hashBytes = HMACSHA256.HashData(RepoKey.NameKey!, input);
      return ToHexString(hashBytes);
   }

   private static byte[] Encrypt(byte[] plain, string hash)
   {
      byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
      byte[] tag = new byte[TagSize];
      byte[] cipher = new byte[plain.Length];

      using var aes = new AesGcm(RepoKey.EncKey!, TagSize);
      aes.Encrypt(nonce, plain, cipher, tag, Encoding.UTF8.GetBytes(hash));

      return nonce.Concat(tag).Concat(cipher).ToArray();
   }

   private static byte[] Decrypt(byte[] stored, string hash)
   {
      if (stored.Length < NonceSize + TagSize)
         throw new GitkoException("Objekat je oštećen: " + hash);

      byte[] nonce = stored[..NonceSize];
      byte[] tag = stored[NonceSize..(NonceSize + TagSize)];
      byte[] cipher = stored[(NonceSize + TagSize)..];
      byte[] plain = new byte[cipher.Length];

      using var aes = new AesGcm(RepoKey.EncKey!, TagSize);
      try
      {
         aes.Decrypt(nonce, cipher, tag, plain, Encoding.UTF8.GetBytes(hash));
      }
      catch (AuthenticationTagMismatchException)
      {
         throw new GitkoException("Objekat je oštećen: " + hash);
      }
      return plain;
   }

   private static string ToHexString(byte[] bytes)
   {
      var sb = new StringBuilder(bytes.Length * 2);
      foreach (byte b in bytes)
         sb.Append(b.ToString("x2"));
      return sb.ToString();
   }
}
