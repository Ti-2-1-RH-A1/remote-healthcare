using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DoctorApplication
{
    public class ClientManager
    {
        private List<Client> clients = new List<Client>();
        private ServerClient.Client client;

        public ClientManager()
        {
        }

        public async Task start()
        {
            client = new ServerClient.Client("localhost", "EchteDokter", true);
            while (!client.loggedIn)
            {
                await Task.Delay(1);
            }

            client.SendPacket(new Dictionary<string, string>()
            {
                {"Method", "GetClients"}
            }, new Dictionary<string, string>(), (header, data) =>
            {
                Console.WriteLine(header);
                Console.WriteLine(data);
                AddClientsFromString(header["Data"]);
            });
        }

        private void AddClientsFromString(string clientsString)
        {
            clientsString = clientsString.Substring(0, clientsString.Length - 1);
            string[] strings = clientsString.Split(";");

            foreach (string clientString in strings)
            {
                string[] split = clientString.Split("|");

                Client client = new Client();
                client.clientAuthKey = split[0];
                clients.Add(client);
            }
        }

        public void sendMessageToAll(string message)
        {
            List<string> clientsId = new List<string>();
            clients.ForEach((s1)=>clientsId.Add(s1.clientAuthKey));

            SendToClients(clientsId,"Message",new Dictionary<string, string>()
            {
                {"Message",message}
            });
        }

        private void SendToClients(List<string> clientList, string action, Dictionary<string, string> data = null)
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
                    {"Clients", clientsString}
                };
            }

            client.SendPacket(new Dictionary<string, string>()
                {
                    {"Method", "SendToClients"},
                    {"Action", action}
                }, data);
            
        }
    }
}