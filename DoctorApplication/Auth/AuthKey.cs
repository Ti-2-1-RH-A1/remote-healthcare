using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoctorApplication.Auth
{
    class AuthKey
    {
        private static readonly string storageLocation = Directory.GetCurrentDirectory() + "/auth";

        public static string GetAuthKey()
        {
            if (Directory.Exists(storageLocation))
            {
                if (File.Exists(storageLocation + "key.txt"))
                {
                    return File.ReadAllText(storageLocation + "key.txt");
                }
                File.Create(storageLocation + "key.txt");
            }
            else
            {
                Directory.CreateDirectory(storageLocation);
            }
            throw new FileNotFoundException("Key file not found created at: " + storageLocation + "key.txt");
        
        }
    }
}
