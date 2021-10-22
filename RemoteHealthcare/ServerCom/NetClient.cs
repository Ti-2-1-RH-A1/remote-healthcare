﻿using System.Collections.Generic;
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
            client = new Client("localhost", false, "Henk");
            while (!client.loggedIn)
            {
                Thread.Sleep(10);
            }

        }

        public void SendRealtime(string name, float data)
        {
            client.SendPacket(new Dictionary<string, string>()
            {
                { "Method", "PostRT" },
                { "Id", client.UUID },
            }, new Dictionary<string, string>() {
                { name, data.ToString() },
            });
        }

        public void SendPost(string name, float data)
        {
            SendRealtime(name, data);
            client.SendPacket(new Dictionary<string, string>()
            {
                { "Method", "Post" },
                { "Id", client.UUID },
            }, new Dictionary<string, string>() {
                { name, data.ToString() },
            });
        }

    }
}
