using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace ServerClient.Data
{
    public class DataHandler
    {
        public Dictionary<string, ClientData> ClientData;
        private readonly string storageLocation = Directory.GetCurrentDirectory() + "/clients/storage";

        public DataHandler()
        {
            ClientData = new Dictionary<string, ClientData>();
            if (!Directory.Exists(storageLocation))
            {
                Directory.CreateDirectory(storageLocation);
            }
        }

        private string FilePath(string id) => storageLocation + "/" + id + ".json";

        public void LoadAllData()
        {
            string[] storageFiles = Directory.GetFiles(storageLocation);
            foreach (string file in storageFiles)
            {
                if (file.IndexOf(".json") != -1)
                {
                    string fileName = file.Substring(0, file.IndexOf(".json"));
                    JObject jo = JObject.Parse(File.ReadAllText(file));
                    
                    ClientData.Add(fileName, new ClientData(){ client_id=fileName,client_name=jo["name"].ToString()});
                }
            }
        }

        public void AddFile(string id, string name)
        {
            
            if (File.Exists(FilePath(id)))
            {
                
                JObject jo = JObject.Parse(File.ReadAllText(FilePath(id)));
                if (!ClientData.ContainsKey(id)) ClientData.Add(id, new ClientData() { client_id = id, client_name = jo["name"].ToString()});
            }
            else
            {
                File.Create(FilePath(id));

                JObject idJson = new()
                {
                    { "id", id }
                };

                JObject nameJson = new()
                {
                    { "name", name }
                };

                JObject dataJson = new();

                JObject mainJson = new()
                {
                    idJson,
                    nameJson,
                    dataJson
                };
                File.WriteAllText(FilePath(id), mainJson.ToString());
                ClientData.Add(id, new ClientData() { client_id = id, client_name =name });
            }
        }

        public bool StoreData(string id, Dictionary<string, string> healthData)
        {
            if (!ClientData.ContainsKey(id)) return false;
            JObject jo = JObject.Parse(File.ReadAllText(FilePath(id)));
            JArray data = jo["data"] as JArray;
            JObject main = new();
            foreach (var (key, value) in healthData)
            {
                main.Add(key, value);
            }
            data.Add(main);
            jo["data"] = data;
            File.WriteAllText(FilePath(id),jo.ToString());
            return true;
        }
    }
}
