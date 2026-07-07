using gitko.Models;

namespace gitko.CLI.Objects;

public class ObjectStore
{
   
   public Response Store(byte[] data)
   {
      Response r = new Response();
      
      
      string hashed = Helper.Hash(data);
      string folderName = hashed[0..2];
      string fileName = hashed[2..];
      string root = "";
      try
      {
         root = Helper.LocateRootDirectory();
         
         string objectsDir = Path.Combine(root, ".gitko", "objects");
         string folderPath = Path.Combine(objectsDir, folderName);
         Directory.CreateDirectory(folderPath); //pravi folder aa
         
         string filePath = Path.Combine(folderPath, fileName);
         File.WriteAllBytes(filePath, data);

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