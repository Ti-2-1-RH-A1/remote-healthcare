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
            actions = new Dictionary<string, Client.Callback>() {
                { "Stop", StopClient() },
                { "Start", StartClient() },
                { "Message", HandleMessage() },
                { "SetResistance", SetResistance() }
            };
        }

        private Client.Callback SetResistance()
        {
            return delegate (Dictionary<string, string> header, Dictionary<string, string> data)
            {
                data.TryGetValue("Resistance", out string resistance);
                int resist = int.Parse(resistance);

                iServiceProvider.GetService<IBikeManager>().SetResistance(resist);

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
        private void HandleDataFromServer(object Client, DataReceivedArgs e)
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
            Console.WriteLine("Wat is je naam? Deze sturen we naar de dokter zodat hij weet wie je bent.");
            client = new Client("remotehealthcare.local", false, Console.ReadLine());
            while (!client.loggedIn)
            {
                Thread.Sleep(10);
            }
            client.DataReceived += HandleDataFromServer;
        }

        public void SendPost(Dictionary<string, string> data)
        {
            client.SendPacket(new Dictionary<string, string>()
            {
                { "Method", "Post" },
                { "Id", client.UUID },
            }, data);
        }

        private Client.Callback HandleMessage() => delegate (Dictionary<string, string> header, Dictionary<string, string> data)
            {
                if (data.TryGetValue("Message", out string message))
                {
                    //                  BgGreen   [CHAT]Reset     FgGreen   {message}Reset    
                    Console.WriteLine($"\u001b[42m[CHAT]\u001b[0m \u001b[32m{message}\u001b[0m");
                    IVRManager vrManager = iServiceProvider.GetService<IVRManager>();
                    vrManager.HandleDoctorMessage(message);
                }
            };
    }
}
