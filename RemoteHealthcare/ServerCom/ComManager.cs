using System;

namespace RemoteHealthcare.ServerCom
{
    class ComManager
    {
        private NetClient netClient;
        private readonly IServiceProvider services;

        public ComManager(IServiceProvider services)
        {
            this.services = services;
            netClient = new NetClient();
        }

        public void Start()
        {
            netClient.Start();
        }
    }
}
