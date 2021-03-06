using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using NetProtocol;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace DoctorApplication
{
    public class ClientManager
    {
        private readonly Dictionary<string, Client> clients = new();
        private NetProtocol.Client client;
        

        public delegate void DataReceivedHandler(object Client, DataReceivedArgs PacketInformation);

        public Dictionary<string, NetProtocol.Client.Callback> actions;
        public MainWindow MainWindow;
        public DoctorActions doctorActions;

        public ClientManager(MainWindow mainWindow)
        {
            actions = new Dictionary<string, NetProtocol.Client.Callback>() {
                { "GetClients", AddClientsFromString() },
                { "NewClient", AddConnectedClient() },
                { "RemoveClient", RemoveDisconnectedClient() },
                { "Realtime", ReceiveRealtime() },
            };

            this.MainWindow = mainWindow;
        }

        public async Task Start()
        {
            client = new NetProtocol.Client("remotehealthcare.local", false);

            while (!client.loggedIn)
            {
                Thread.Sleep(10);
            }

            client.DataReceived += HandleData;

            client.SendPacket(new Dictionary<string, string>()
            {
                { "Method", "GetClients" },
            }, new Dictionary<string, string>() {
                { "Empty", "Empty" },
            });
        }

        /// <summary>
        /// Subscription for messages from the server
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="e">DataReceivedArgs</param>
        private void HandleData(object Client, DataReceivedArgs e)
        {
            e.headers.TryGetValue("Method", out string item);
        
            if (actions.TryGetValue(item, out NetProtocol.Client.Callback action))
            {
                action(e.headers, e.data);
                return;
            }
        }

        /// <summary>
        /// receives realtime data from client
        /// </summary>
        private NetProtocol.Client.Callback ReceiveRealtime()
        {
            return delegate (Dictionary<string, string> header, Dictionary<string, string> data)
            {
                if (data.TryGetValue("Id", out string id))
                {
                    if (clients.TryGetValue(id, out Client editClient))
                    {
                        if (data.TryGetValue("speed", out string speed)) editClient.speed = speed;
                        if (data.TryGetValue("time", out string time)) editClient.time = time;
                        if (data.TryGetValue("distance_traveled", out string distance_traveled)) editClient.distanceTraveled = distance_traveled;
                        if (data.TryGetValue("rpm", out string rpm)) editClient.rpm = rpm;
                        if (data.TryGetValue("heartrate", out string heartrate)) editClient.heartRate = heartrate;
                        clients[id] = editClient;
                    }
                }
            };
        }

        /// <summary>
        /// generate a list of active clients based on a string
        /// </summary>
        /// <param name="clientsString"></param>
        private NetProtocol.Client.Callback AddClientsFromString()
        {
            return delegate (Dictionary<string, string> header, Dictionary<string, string> data)
            {
                if (data["Data"] == "Data")
                {
                    return;
                }
                string[] strings = data["Data"][0..^1].Split(";");

                foreach (string clientString in strings)
                {
                    string[] split = clientString.Split("|");

                    Client client = new()
                    {
                        clientSerial = split[0],
                        clientName = split[1]
                    };
                    clients.Add(split[0], client);
                    MainWindow.AddToList(client);
                }
            };

        }

        /// <summary>
        /// Handles the callback when a new client connects to the server
        /// </summary>
        /// <returns></returns>
        private NetProtocol.Client.Callback AddConnectedClient()
        {
            return delegate (Dictionary<string, string> header, Dictionary<string, string> data)
            {
                string[] split = data["Data"][0..^1].Split("|");

                Client client = new()
                {
                    clientSerial = split[0],
                    clientName = split[1]
                };
                clients.Add(split[0], client);
                MainWindow.AddToList(client);
            };
        }

        /// <summary>
        /// Handles the disconnect callback
        /// </summary>
        private NetProtocol.Client.Callback RemoveDisconnectedClient()
        {
            return delegate (Dictionary<string, string> header, Dictionary<string, string> data)
            {
                data.TryGetValue("Data", out string uuid);
                clients.TryGetValue(uuid, out Client client);
                MainWindow.RemovefromList(client);
                clients.Remove(uuid);
            };
        }

        public void RequestHistoryClients()
        {
            client.SendPacket(new Dictionary<string, string>()
                {
                    {"Method", "GetHistoryClients"},
                }, new Dictionary<string, string>() {
                    { "Empty", "Empty" },
                },
                (Dictionary<string, string> header, Dictionary<string, string> data) =>
                {
                    data.TryGetValue("data", out string Jdata);

                    Dictionary<string, string> jo = JsonConvert.DeserializeObject<Dictionary<string, string>>(Jdata);

                    MainWindow.DoctorActions.UpdateSelectWindow(jo);
                });
        }

        public void RequestHistoryData(string clientID)
        {
            client.SendPacket(new Dictionary<string, string>()
            {
                { "Method", "GetHistory" },
            }, new Dictionary<string, string>()
            {
                { "client_id", clientID },
            }, (Dictionary<string, string> header, Dictionary<string, string> data) =>
            {
                data.TryGetValue("data", out string Jdata);

                JObject jo = JObject.Parse(Jdata);
    
                MainWindow.DoctorActions.UpdateHistoryWindow(jo);
            });
        }

        /// <summary>
        /// sends a message to the server with all clients and message
        /// </summary>
        /// <param name="message"></param>
        public void SendMessageToAll(string message)
        {
            SendToClients(clients.Values.Select(v => v.clientSerial).ToList(), "Message", new Dictionary<string, string>()
            {
                { "Message", message },
            });
        }

        public void SendToClient(string action, Dictionary<string, string> data, string id)
        {
            SendToClients(clients.Keys
                .Where(p => p.Equals(id))
                .ToList(), action, data);
        }

        /// <summary>
        /// sends to a list of clients to the server with a action and a dictionary
        /// </summary>
        /// <param name="clientList"></param>
        /// <param name="action"></param>
        /// <param name="data"></param>
        public void SendToClients(List<string> clientList, string action, Dictionary<string, string> data = null)
        {
            string clientsString = JsonConvert.SerializeObject(clientList);

            if (data != null)
            {
                data.Add("Clients", clientsString);
            }
            else
            {
                data = new Dictionary<string, string>()
                {
                    { "Clients", clientsString },
                };
            }

            client.SendPacket(new Dictionary<string, string>()
            {
                { "Method", "SendToClients" },
                { "Action", action },
            }, data);
        }


    }
}
