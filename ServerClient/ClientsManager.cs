using System;
using System.Collections.Generic;

namespace ServerClient
{
    class ClientsManager
    {
        private readonly List<ClientHandler> clients;

        public ClientsManager()
        {
            clients = new List<ClientHandler>();
        }

        public void Add(ClientHandler clientHandler) => clients.Add(clientHandler);

        public void Disconnect(ClientHandler client)
        {
            clients.Remove(client);
            Console.WriteLine("Client disconnected");
        }
    }
}
