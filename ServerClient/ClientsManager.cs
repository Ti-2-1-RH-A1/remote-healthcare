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
            clients.Remove(client.authKey);
            Console.WriteLine("Client disconnected");
        }

        public List<ClientHandler> GetClients()
        {
            return new List<ClientHandler>(clients.Values);
        }
    }
}
