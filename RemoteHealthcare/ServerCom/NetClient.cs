using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetProtocol;

namespace RemoteHealthcare.ServerCom
{

    class NetClient
    {
        private Client client;

        public NetClient()
        {
        }

        public async Task Start()
        {
            client = new Client("localhost", "Henk", false);
            while (!client.loggedIn)
            {
                Thread.Sleep(10);
            }

            // Test request.
            client.SendPacket(new Dictionary<string, string>()
            {
                { "Method", "GetClients" },
            }, new Dictionary<string, string>(), (header, data) =>
            {
                Console.WriteLine(header);
                Console.WriteLine(data);
            });
        }
    }
}
