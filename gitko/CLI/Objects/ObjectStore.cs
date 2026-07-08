using System.Text;
using gitko.Models;

namespace gitko.CLI.Objects;

public class ObjectStore
{
   
   public Response Store(byte[] data,ObjectType type)
   {
      Response r = new Response();
      
      byte[] header = Encoding.UTF8.GetBytes($"{type.ToString().ToLower()}\0");
      byte[] full = header.Concat(data).ToArray();
      
      string hashed = Helper.Hash(full);
      string folderName = hashed[0..2];
      string fileName = hashed[2..];
      string root = "";
      
    

      try
      {
         root = Helper.LocateRootDirectory();
         
         string objectsDir = Path.Combine(root, ".gitko", "objects");
         string folderPath = Path.Combine(objectsDir, folderName);
         Directory.CreateDirectory(folderPath);
         
         string filePath = Path.Combine(folderPath, fileName);
         
         File.WriteAllBytes(filePath, full);

         r.Data = hashed;
         r.Success =  true;
      }
      catch (Exception e)
      {
         r.Success = false;
         r.Message =  e.Message;
      }

      return r;
   }
}