using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NetProtocol;

namespace RemoteHealthcare.ServerCom
{

    class NetClient
    {
        private Client client;
        public Dictionary<string, Client.Callback> actions;

        public NetClient()
        {
            actions = actions = new Dictionary<string, Client.Callback>() {
                { "Stop", StartClient() },
                { "Start", StopClient() },
            };
        }

        private Client.Callback StartClient()
        {
            return delegate (Dictionary<string, string> header, Dictionary<string, string> data)
            {
                
            };
        }

        private Client.Callback StopClient()
        {
            return delegate (Dictionary<string, string> header, Dictionary<string, string> data)
            {

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
            client = new Client("localhost", "Henk", false);
            while (!client.loggedIn)
            {
                Thread.Sleep(10);
            }
            client.DataReceived += HandleData;

        }

        public void SendData(string name, float data)
        {
            client.SendPacket(new Dictionary<string, string>()
            {
                { "Method", "Post" },
                { "Id", this.client.UUID },
            }, new Dictionary<string, string>() {
                { name, data.ToString() },
            });
        }

    }
}
