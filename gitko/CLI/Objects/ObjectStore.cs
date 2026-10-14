using System.Security.Cryptography;
using System.Text;

namespace gitko.CLI.Objects;

public class ObjectStore
{
   
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
      
      File.WriteAllBytes(filePath, full);

      return hashed;
   }

   public byte[] Read(string hash, ObjectType type)
   {
      if (hash.Length != 64)
         throw new GitkoException("Neispravan hash: " + hash);

      string filePath = Path.Combine(Repository.ObjectsDir, hash[0..2], hash[2..]);
      if (!File.Exists(filePath))
         throw new GitkoException("Objekat nije pronađen: " + hash);

      byte[] full = File.ReadAllBytes(filePath);
      int nullIndex = Array.IndexOf(full, (byte)0);
      string header = Encoding.UTF8.GetString(full, 0, nullIndex);
      string expected = type.ToString().ToLower();
      if (header != expected)
         throw new GitkoException($"{hash} nije {expected}, nego {header}.");

      return full[(nullIndex + 1)..];
   }

   private static string Hash(byte[] input)
   {
      using var sha = SHA256.Create();
      byte[] hashBytes = sha.ComputeHash(input);
      return ToHexString(hashBytes);
   }

   private static string ToHexString(byte[] bytes)
   {
      var sb = new StringBuilder(bytes.Length * 2);
      foreach (byte b in bytes)
         sb.Append(b.ToString("x2"));
      return sb.ToString();
   }
}
