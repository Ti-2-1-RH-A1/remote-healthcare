using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ServerClient.Data
{
    public class DataHandler
    {
        public Dictionary<string, ClientData> ClientData;
        private Dictionary<string, JObject> loadedData;
        private Dictionary<string, bool> changed;
        private bool changes = false;
        private readonly Timer Savetimer;
        private readonly string storageLocation = Directory.GetCurrentDirectory() + "/clients/storage";

        public DataHandler()
        {
            ClientData = new Dictionary<string, ClientData>();
            loadedData = new Dictionary<string, JObject>();
            changed = new Dictionary<string, bool>();
            if (!Directory.Exists(storageLocation))
            {
                Directory.CreateDirectory(storageLocation);
            }
            Savetimer = new(_ =>
            {
                if (changes)
                {
                    Update();
                }
            }, null, 0, 5000);
        }

        private string FilePath(string id) => storageLocation + "/" + id + ".json";

        public void LoadAllData()
        {
            ClientData.Clear();
            string[] storageFiles = Directory.GetFiles(storageLocation);
            foreach (string file in storageFiles)
            {
                if (file.IndexOf(".json") != -1)
                {
                    JObject jo = JObject.Parse(File.ReadAllText(file));
                    var client = new ClientData() { client_id = jo["id"].ToString(), client_name = jo["name"].ToString() };
                    ClientData.Add(jo["id"].ToString(), client);
                }
            }
        }

        public string LoadClientHistory()
        {
            string[] storageFiles = Directory.GetFiles(storageLocation);
            Dictionary<string, string> client = new Dictionary<string, string>();
            foreach (string file in storageFiles)
            {
                if (file.IndexOf(".json") != -1)
                {
                    JObject jo = JObject.Parse(File.ReadAllText(file));
                    client.Add(jo["id"].ToString(), jo["name"].ToString());
                }
            }
            return JsonConvert.SerializeObject(client); ;
        }

        public JObject LoadClientHistoryData(string clientID)
        {
            if (File.Exists(FilePath(clientID)))
            {
                return JObject.Parse(File.ReadAllText(FilePath(clientID)));
                
            } 
                return new JObject();
        }

        public void AddFile(string id, string name)
        {
            if (File.Exists(FilePath(id)))
            {
                JObject jo = JObject.Parse(File.ReadAllText(FilePath(id)));
                if (!ClientData.ContainsKey(id)) ClientData.Add(id, new ClientData() { client_id = id, client_name = jo["name"].ToString() });
            }
            else
            {
                JObject mainJson = new()
                {
                    { "id", id },
                    { "name", name },
                    { "data", new JArray() }
                };

                Console.WriteLine(mainJson);
                File.WriteAllText(FilePath(id), mainJson.ToString());
                ClientData.Add(id, new ClientData() { client_id = id, client_name = name });
            }
        }


        public bool StoreData(string id, Dictionary<string, string> healthData)
        {
            if (!ClientData.ContainsKey(id)) return false;

            if (loadedData.ContainsKey(id))
            {
                if (loadedData.TryGetValue(id, out JObject jo))
                {
                    JArray data = jo["data"] as JArray;
                    JObject main = new();
                    foreach ((string key, string value) in healthData)
                    {
                        main.Add(key, value);
                    }
                    data.Add(main);
                    jo["data"] = data;
                    loadedData[id] = jo;
                    changed[id] = true;
                    changes = true;
                }
            }
            else
            {
                JObject jo = JObject.Parse(File.ReadAllText(FilePath(id)));
                JArray data = jo["data"] as JArray;
                JObject main = new();
                foreach ((string key, string value) in healthData)
                {
                    main.Add(key, value);
                }
                data.Add(main);
                jo["data"] = data;
                loadedData.Add(id, jo);
                changed.Add(id, false);
                try
                {
                    File.WriteAllText(FilePath(id), jo.ToString());
                }
                catch (Exception)
                {
                    Console.WriteLine("Data not saved exception found. But still going strong");
                }
            }
            return true;
        }

        private void Update()
        {
            foreach (KeyValuePair<string, bool> item in changed)
            {
                if (item.Value)
                {
                    if (loadedData.TryGetValue(item.Key, out JObject jo))
                    {
                        Console.WriteLine($"Saving {item.Key}");
                        try
                        {
                            File.WriteAllText(FilePath(item.Key), jo.ToString());
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Data not saved exception found.");
                        }
                        changed[item.Key] = false;
                    }
                }
            }
            changes = false;
        }
    }
}
