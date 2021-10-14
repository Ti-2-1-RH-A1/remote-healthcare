using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using ServerClient;

namespace DoctorApplication
{
    public class ClientManager
    {
        private readonly List<Client> clients = new();
        private ServerClient.Client client;

        public delegate void DataReceivedHandler(object Client, DataReceivedArgs PacketInformation);

        public delegate void Callback(Dictionary<string, string> header, Dictionary<string, string> data);
        public Dictionary<string, Callback> actions;
        public MainWindow MainWindow;
        public ClientManager(MainWindow mainWindow)
        {
            actions = new Dictionary<string, Callback>() {
                {"GetClients", AddClientsFromString()},
                {"NewClient", AddConnectedClient()}
            };

            this.MainWindow = mainWindow;
        }

        public async Task Start()
        {
            client = new ServerClient.Client("localhost", "EchteDokter", false);
            while (!client.loggedIn)
            {
                Thread.Sleep(10);
            }

            client.DataReceived += HandleData;

            client.SendPacket(new Dictionary<string, string>()
            {
                { "Method", "GetClients" }
            }, new Dictionary<string, string>());
        }

        

        private void HandleData(object Client, DataReceivedArgs e)
        {
            e.headers.TryGetValue("Method", out string item);

            if (actions.TryGetValue(item, out Callback action))
            {
                action(e.headers, e.data);
                return;
            }
        }

        /// <summary>
        /// generate a list of active clients based on a string
        /// </summary>
        /// <param name="clientsString"></param>
        private Callback AddClientsFromString()
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
                    clients.Add(client);
                    MainWindow.addToList(client);
                }
            };
            
        }

        private Callback AddConnectedClient()
        {
            return delegate(Dictionary<string, string> header, Dictionary<string, string> data)
            {
                string[] split = data["Data"][0..^1].Split("|");

                Client client = new()
                {
                    clientSerial = split[0],
                    clientName = split[1]
                };
                clients.Add(client);
                MainWindow.addToList(client);
            };
        }

        /// <summary>
        /// sends a message to the server with all clients and message
        /// </summary>
        /// <param name="message"></param>
        public void SendMessageToAll(string message)
        {
            List<string> clientsId = new List<string>();
            clients.ForEach((s1) => clientsId.Add(s1.clientSerial));

            SendToClients(clientsId, "Message", new Dictionary<string, string>()
            {
                { "Message", message }
            });
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
                    { "Clients", clientsString }
                };
            }

            client.SendPacket(new Dictionary<string, string>()
            {
                { "Method", "SendToClients" },
                { "Action", action }
            }, data);
        }
    }
}
