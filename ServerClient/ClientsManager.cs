using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;

namespace ServerClient
{
    public class ClientsManager
    {
        private Dictionary<string, ClientHandler> clients;

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
            string json = header["Clients"];
            List<string> clients;
            //clients = JsonConvert.DeserializeObject<List<string>>(json);
            clients = new List<string>();

        }

        private void SendToClients(List<string> authKeys, string action, Dictionary<string, string> dict)
        {
            foreach (string authKey in authKeys)
            {
                ClientHandler clientHandler = clients[authKey];
                clientHandler.SendPacket(new Dictionary<string, string>()
                {
                    {"Method", "Post"},
                    {"Action",action}
                },dict);

            }
        }



    }
}
