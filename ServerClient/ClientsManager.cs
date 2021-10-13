using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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
            if (client.authKey != null && clients.ContainsKey(client.authKey))
            {
                clients.Remove(client.authKey);
            }
            else
            {
                Console.WriteLine(client.ToString() + " not found");
            }

            Console.WriteLine("Client disconnected");
        }

        public List<ClientHandler> GetClients()
        {
            return new List<ClientHandler>(clients.Values);
        }
        /// <summary>
        /// send a header and data to a list of clients
        /// </summary>
        /// <param name="header"></param>
        /// <param name="data"></param>
        public void SendToClients(Dictionary<string, string> header, Dictionary<string, string> data)
        {
            string json = data["Clients"];
            List<string> clientAuths;
            clientAuths = JsonConvert.DeserializeObject<List<string>>(json);
            SendToClients(clientAuths, header["Action"], data);
        }

        /// <summary>
        /// send a 
        /// </summary>
        /// <param name="authKeys"></param>
        /// <param name="action"></param>
        /// <param name="dict"></param>
        private void SendToClients(List<string> authKeys, string action, Dictionary<string, string> dict)
        {
            foreach (string authKey in authKeys)
            {
                ClientHandler clientHandler = clients[authKey];
                clientHandler.SendPacket(new Dictionary<string, string>()
                {
                    {"Method", action}
                }, dict);

            }
        }



    }
}
