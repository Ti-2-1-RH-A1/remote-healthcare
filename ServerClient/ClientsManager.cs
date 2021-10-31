using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using ServerClient.Data;

namespace ServerClient
{
    public class ClientsManager
    {
        private readonly Dictionary<string, ClientHandler> clients;
        public readonly DataHandler dataHandler;

        public enum ClientType
        {
            PATIENT,
            DOCTOR,
            ALL,
        }

        public ClientsManager()
        {
            clients = new Dictionary<string, ClientHandler>();
            dataHandler = new DataHandler();
            dataHandler.LoadAllData();
        }

        public void Add(ClientHandler clientHandler)
        {
            if (!clients.ContainsKey(clientHandler.UUID))
                clients.Add(clientHandler.UUID, clientHandler);
            if (!clientHandler.UUID.Contains("DOCTOR"))
            {
                Console.WriteLine("No doctor client: " + clientHandler.UUID);
                SendToClients(ClientType.DOCTOR,
                    "NewClient",
                    new Dictionary<string, string>(){
                    { "Data", Util.StringifyClients(new List<ClientHandler>(){clientHandler})},
                });
            }
        }

        /// <summary>
        /// Sends the disconnect message to all doctors
        /// </summary>
        /// <param name="client"></param>
        public void Disconnect(ClientHandler client)
        {
            if (!client.UUID.Contains("DOCTOR-"))
            {
                SendToClients(ClientType.DOCTOR,
                    "RemoveClient",
                    new Dictionary<string, string>()
                    {
                        {"Data", client.UUID},
                    });
                if (client.UUID != null && clients.ContainsKey(client.UUID))
                {
                    clients.Remove(client.UUID);
                }
                else
                {
                    Console.WriteLine(client.ToString() + " not found");
                }
            }

            Console.WriteLine("Client disconnected");
        }

        /// <summary>
        /// Gets all logged in patient clients
        /// </summary>
        /// <returns>List<ClientHandler></returns>
        public List<ClientHandler> GetClients()
        {
            Console.WriteLine(clients);
            Dictionary<string, ClientHandler> dic = clients
                .Where(p => !p.Key.Contains("DOCTOR"))
                .ToDictionary(p => p.Key, p => p.Value);
            Console.WriteLine(dic);

            return new List<ClientHandler>(dic.Values);
        }

        /// <summary>
        /// send a header and data to a list of clients
        /// </summary>
        /// <param name="header"></param>
        /// <param name="data"></param>
        public void SendToClients(Dictionary<string, string> header, Dictionary<string, string> data)
        {
            string json = data["Clients"];
            List<string> clientUUIDs;
            clientUUIDs = JsonConvert.DeserializeObject<List<string>>(json);
            SendToUUID(clientUUIDs, header["Action"], data);
        }

        /// <summary>
        /// Send message to clients specified
        /// </summary>
        /// <param name="clientType"></param>
        /// <param name="action"></param>
        /// <param name="data"></param>
        public void SendToClients(ClientType clientType, string action, Dictionary<string, string> data)
        {
            List<string> clientUUIDs;
            clientUUIDs = GetClientUUID(clientType);
            SendToUUID(clientUUIDs, action, data);
        }

        /// <summary>
        /// Returns a list of clients specified to the enum
        /// </summary>
        /// <param name="clientType"></param>
        /// <returns>List with client UUID's</returns>
        private List<string> GetClientUUID(ClientType clientType)
        {
            switch (clientType)
            {
                case ClientType.DOCTOR:
                    return clients.Keys
                        .Where(p => p.Contains("DOCTOR"))
                        .ToList();
                case ClientType.PATIENT:
                    return clients.Keys
                        .Where(p => !p.Contains("DOCTOR"))
                        .ToList();
                case ClientType.ALL:
                default:
                    return clients.Keys
                        .ToList();

            }
        }

        /// <summary>
        /// send a 
        /// </summary>
        /// <param name="authKeys"></param>
        /// <param name="action"></param>
        /// <param name="dict"></param>
        private void SendToUUID(List<string> uuidList, string action, Dictionary<string, string> dict)
        {
            foreach (string uuid in uuidList)
            {
                ClientHandler clientHandler = clients[uuid];
                clientHandler.SendPacket(new Dictionary<string, string>()
                {
                    { "Method", action },
                }, dict);

            }
        }
    }
}
