using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using NetProtocol;

namespace DoctorApplication
{
    public class ClientManager
    {
        private readonly Dictionary<string, Client> clients = new();
        private NetProtocol.Client client;


        public delegate void DataReceivedHandler(object Client, DataReceivedArgs PacketInformation);

        public delegate void Callback(Dictionary<string, string> header, Dictionary<string, string> data);
        public Dictionary<string, Callback> actions;
        public MainWindow MainWindow;
        public ClientManager(MainWindow mainWindow)
        {
            actions = new Dictionary<string, Callback>() {
                { "GetClients", AddClientsFromString() },
                { "NewClient", AddConnectedClient() },
                { "RemoveClient", RemoveDisconnectedClient() },
                { "Realtime", ReceiveRealtime() },
            };

            this.MainWindow = mainWindow;
        }

        public async Task Start()
        {
            client = new NetProtocol.Client("localhost", "EchteDokter", false);

            while (!client.loggedIn)
            {
                Thread.Sleep(10);
            }

            client.DataReceived += HandleData;

            client.SendPacket(new Dictionary<string, string>()
            {
                { "Method", "GetClients" },
            }, new Dictionary<string, string>());
        }

        /// <summary>
        /// Subscription for messages from the server
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="e">DataReceivedArgs</param>
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
        /// receives realtime data from client
        /// </summary>
        private Callback ReceiveRealtime()
        {
            return delegate (Dictionary<string, string> header, Dictionary<string, string> data)
            {
                Console.WriteLine(data.Values);
            };
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
                    clients.Add(split[0], client);
                    MainWindow.AddToList(client);
                }
            };

        }

        /// <summary>
        /// Handles the callback when a new client connects to the server
        /// </summary>
        /// <returns></returns>
        private Callback AddConnectedClient()
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
        private Callback RemoveDisconnectedClient()
        {
            return delegate (Dictionary<string, string> header, Dictionary<string, string> data)
            {
                data.TryGetValue("Data", out string uuid);
                clients.TryGetValue(uuid, out Client client);
                MainWindow.RemovefromList(client);
                clients.Remove(uuid);

            };
        }

        /// <summary>
        /// sends a message to the server with all clients and message
        /// </summary>
        /// <param name="message"></param>
        public void SendMessageToAll(string message)
        {
            SendToClients(clients.Keys.ToList(), "Message", new Dictionary<string, string>()
            {
                { "Message", message },
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
