
using System;
using System.Collections.Generic;
using System.Threading;

namespace DoctorApplication
{
    internal class ClientManager
    {
        private readonly List<Client> clients = new List<Client>();

        public ClientManager()
        {

            ServerClient.Client client = new ServerClient.Client("localhost", "EchteDokter", true);

            while (!client.loggedIn)
            {
                Thread.Sleep(10);
            }

            client.SendPacket(new Dictionary<string, string>() {
                { "Method", "GetClients" }
            }, new Dictionary<string, string>(), (header, data) =>
            {
                Console.WriteLine(header);
                Console.WriteLine(data);
                AddClientsFromString(header["Data"]);
            });

        }

        public void AddClientsFromString(string clientsString)
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

    }
}
