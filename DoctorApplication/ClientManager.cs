using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace DoctorApplication
{
    public class ClientManager
    {
        private readonly List<Client> clients = new();
        private ServerClient.Client client;

        public ClientManager()
        {
        }

        public async Task Start()
        {
            client = new ServerClient.Client("localhost", "EchteDokter", true);
            while (!client.loggedIn)
            {
                Thread.Sleep(10);
            }

            client.SendPacket(new Dictionary<string, string>()
            {
                { "Method", "GetClients" }
            }, new Dictionary<string, string>(), (header, data) =>
            {
                Console.WriteLine(header);
                Console.WriteLine(data);
                AddClientsFromString(data["Data"]);
            });
        }

        /// <summary>
        /// generate a list of active clients based on a string
        /// </summary>
        /// <param name="clientsString"></param>
        private void AddClientsFromString(string clientsString)
        {
            string[] strings = clientsString[0..^1].Split(";");

            foreach (string clientString in strings)
            {
                string[] split = clientString.Split("|");

                Client client = new()
                {
                    clientAuthKey = split[0]
                };
                clients.Add(client);
            }
        }

        /// <summary>
        /// sends a message to the server with all clients and message
        /// </summary>
        /// <param name="message"></param>
        public void SendMessageToAll(string message)
        {
            List<string> clientsId = new List<string>();
            clients.ForEach((s1) => clientsId.Add(s1.clientAuthKey));

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
