using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ServerClient
{
    class AuthHandler
    {
        private Dictionary<string, bool> keys;

        public AuthHandler()
        {
            keys = new Dictionary<string, bool>() {
                { "EchteDokter", true },
                { "Fiets", false },
                { "Tinus", true },
                { "Henk", false }
            };
        }

        public AuthHandler(Dictionary<string, bool> keys)
        {
            this.keys = keys;
        }

        /// <summary>
        /// Checks if key is valid
        /// </summary>
        /// <param name="keyToCheck"></param>
        /// <returns>(Key exists, Key is docter)</returns>
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
