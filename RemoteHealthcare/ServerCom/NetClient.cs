using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NetProtocol;

namespace RemoteHealthcare.ServerCom
{

    class NetClient
    {
        private Client client;
        public delegate void Callback(Dictionary<string, string> header, Dictionary<string, string> data);
        public Dictionary<string, Callback> actions;

        public NetClient()
        {
            actions = new Dictionary<string, Callback>()
            {
                { "Message", HandleMessage() },
            };
        }

        public async Task Start()
        {
            client = new Client("localhost", false, "Henk");
            while (!client.loggedIn)
            {
                Thread.Sleep(10);
            }

            client.DataReceived += HandleDataFromServer;
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

        private void HandleDataFromServer(object Client, DataReceivedArgs e)
        {
            e.headers.TryGetValue("Method", out string item);

            if (actions.TryGetValue(item, out Callback action))
            {
                action(e.headers, e.data);
                return;
            }
        }

        private Callback HandleMessage() => delegate (Dictionary<string, string> header, Dictionary<string, string> data)
            {
                if (data.TryGetValue("Message", out string message))
                {
                    //                  BgGreen   [CHAT]Reset     FgGreen   {message}Reset    
                    Console.WriteLine($"\u001b[42m[CHAT]\u001b[0m \u001b[32m{message}\u001b[0m");
                }
            };
    }
}
