using System.Threading.Tasks;

namespace DoctorApplication
{
    internal class DoctorActions
    {
        private ClientManager clientManager;

        public DoctorActions()
        {
            clientManager = new ClientManager();
        }

        public async Task Start()
        {
            await clientManager.Start();
        }

        /// <summary>
        /// send a message to all clients
        /// </summary>
        /// <param name="message"></param>
        public void SendToAll(string message)
        {
            clientManager.SendMessageToAll(message);
        }
    }
}