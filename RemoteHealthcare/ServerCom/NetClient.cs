using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NetProtocol;
using RemoteHealthcare.Bike;

namespace RemoteHealthcare.ServerCom
{

    class NetClient
    {
        private readonly IServiceProvider iServiceProvider;

        private Client client;
        public Dictionary<string, Client.Callback> actions;
        public NetClient(IServiceProvider iServiceProvider)
        {
            this.iServiceProvider = iServiceProvider;
            actions = actions = new Dictionary<string, Client.Callback>() {
                { "Stop", StartClient() },
                { "Start", StopClient() },
            };
        }

        private Client.Callback StartClient()
        {
            return delegate (Dictionary<string, string> header, Dictionary<string, string> data)
            {
                iServiceProvider.GetService<IDeviceManager>().StartTraining();
            };
        }

        private Client.Callback StopClient()
        {
            return delegate (Dictionary<string, string> header, Dictionary<string, string> data)
            {
                iServiceProvider.GetService<IDeviceManager>().StopTraining();
            };
        }

        /// <summary>
        /// Subscription for messages from the server
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="e">DataReceivedArgs</param>
        private void HandleData(object Client, DataReceivedArgs e)
        {
            e.headers.TryGetValue("Method", out string item);

            if (actions.TryGetValue(item, out Client.Callback action))
            {
                action(e.headers, e.data);
                return;
            }
        }
        
        public async Task Start()
        {
            client = new Client("localhost", false, "Henk");
            while (!client.loggedIn)
            {
                Thread.Sleep(10);
            }
            client.DataReceived += HandleData;

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
