
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Input;

namespace DoctorApplication
{
    class ClientManager
    {
        private List<Client> clients = new List<Client>();

        public ClientManager()
        {

            ServerClient.Client client = new ServerClient.Client("localhost", "EchteDokter", true);

            

            while (!client.loggedIn)
            {
                Thread.Sleep(10);
            }

            client.SendPacket(new Dictionary<string, string>() {
                { "Method", "GetClients" }
            }, new Dictionary<string, string>(), (e1, e2) =>
            {
                Console.WriteLine(e1);
                Console.WriteLine(e2);
                AddClientsFromString(e1["Data"]);
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
