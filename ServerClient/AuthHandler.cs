using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ServerClient
{
    public class AuthHandler
    {
        private readonly Dictionary<string, bool> keys;
        private static readonly string authDir = Directory.GetCurrentDirectory() + "\\clients";

        public AuthHandler(Dictionary<string, bool> keys)
        {
            this.keys = keys;
        }

        public static AuthHandler Init()
        {
            if (!Directory.Exists(authDir)) Directory.CreateDirectory(authDir);
            if (File.Exists(authDir + "\\key.txt"))
            {
                return LoadKeysFromFile(authDir + "\\key.txt");
            }
            AuthHandler auth = new(new Dictionary<string, bool>() {
                { "EchteDokter", true },
                { "Fiets", false },
                { "Tinus", true },
                { "Henk", false },
            });
            auth.SaveKeysToFile(authDir + "\\key.txt");
            return auth;
        }

        /// <summary>
        /// Checks if key is valid
        /// </summary>
        /// <param name="keyToCheck"></param>
        /// <returns>(Key exists, Key is doctor)</returns>
        public (bool, bool) Check(string keyToCheck) =>
            keys.TryGetValue(keyToCheck, out bool value) ? (true, value) : (false, false);

        public void SaveKeysToFile(string filename)
        {
            new XElement("root", keys.Select(kv => new XElement(kv.Key, kv.Value)))
                        .Save(filename, SaveOptions.OmitDuplicateNamespaces);
        }
        public static AuthHandler LoadKeysFromFile(string filename)
        {
            Dictionary<string, bool> keysLoaded = XElement.Parse(File.ReadAllText(filename))
                           .Elements()
                           .ToDictionary(k => k.Name.ToString(), v => bool.Parse(v.Value));
            return new AuthHandler(keysLoaded);
        }
    }
}
