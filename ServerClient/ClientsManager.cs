using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using Newtonsoft.Json;

namespace ServerClient
{
    public class ClientsManager
    {
        private readonly Dictionary<string, ClientHandler> clients;

        public ClientsManager()
        {
            clients = new Dictionary<string, ClientHandler>();
        }

        public void Add(ClientHandler clientHandler) => clients.Add(clientHandler.authKey, clientHandler);

        public void Disconnect(ClientHandler client)
        {
            clients.Remove(client.authKey);
            Console.WriteLine("Client disconnected");
        }

        public List<ClientHandler> GetClients()
        {
            return new List<ClientHandler>(clients.Values);
        }

        public void SendToClients(Dictionary<string, string> header, Dictionary<string, string> data)
        {
            string json = data["Clients"];
            List<string> clientAuths;
            clientAuths = JsonConvert.DeserializeObject<List<string>>(json);
            SendToClients(clientAuths, header["Action"],data);
        }

        private void SendToClients(List<string> authKeys, string action, Dictionary<string, string> dict)
        {
            foreach (string authKey in authKeys)
            {
                ClientHandler clientHandler = clients[authKey];
                clientHandler.SendPacket(new Dictionary<string, string>()
                {
                    {"Method", action}
                },dict);

            }
        }



    }
}
