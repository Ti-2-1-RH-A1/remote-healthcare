using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DoctorApplication.Data
{
    class DataHandler
    {
        public Dictionary<string, ClientData> clientData { get; }
        string storageLocation = Directory.GetCurrentDirectory() + "/clients/storage";

        public DataHandler()
        {
            this.clientData = new Dictionary<string, ClientData>();
            if (!Directory.Exists(storageLocation))
            {
                Directory.CreateDirectory(storageLocation);
            }
        }

        public string filePath(string id) => storageLocation + "/" + id + ".json";

        public void LoadAllData()
        {
            string[] storageFiles = Directory.GetFiles(storageLocation);
            foreach (string file in storageFiles)
            {
                if (file.IndexOf(".json") != -1)
                {
                    string fileName = file.Substring(0, file.IndexOf(".json"));
                    JObject jo = JObject.Parse(File.ReadAllText(file));
                    
                    clientData.Add(fileName, new ClientData(){ client_id=fileName,client_name=jo["name"].ToString()});
                }
            }
        }

        public void addFile(string id, string name)
        {
            
            if (File.Exists(filePath(id)))
            {
                
                JObject jo = JObject.Parse(File.ReadAllText(filePath(id)));
                if (!clientData.ContainsKey(id)) clientData.Add(id, new ClientData() { client_id = id, client_name = jo["name"].ToString()});
            }
            else
            {
                File.Create(filePath(id));

                JObject idJson = new JObject();
                idJson.Add("id", id);

                JObject nameJson = new JObject();
                nameJson.Add("name", name);

                JObject dataJson = new JObject();

                JObject mainJson = new JObject();
                mainJson.Add(idJson);
                mainJson.Add(nameJson);
                mainJson.Add(dataJson);
                File.WriteAllText(filePath(id), mainJson.ToString());
                clientData.Add(id, new ClientData() { client_id = id, client_name =name });
            }
        }

        public async Task<bool> storeData(string id, Dictionary<string, string> healthData)
        {
            if (!clientData.ContainsKey(id)) return false;
            JObject jo = JObject.Parse(File.ReadAllText(filePath(id)));
            JArray data = jo["data"] as JArray;
            JObject main = new JObject();
            foreach (var (key, value) in healthData)
            {
                main.Add(key, value);
            }
            data.Add(main);
            jo["data"] = data;
            File.WriteAllText(filePath(id),jo.ToString());
            return true;
        }
    }
}
