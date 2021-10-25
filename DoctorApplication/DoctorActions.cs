using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DoctorApplication
{
    public class DoctorActions
    {
        private ClientManager clientManager;
        private MainWindow mainWindow;

        public DoctorActions(MainWindow mainWindow)
        {
            clientManager = new ClientManager(mainWindow);
            this.mainWindow = mainWindow;
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

        public void SendStartSession(IList clients)
        {
            List<string> clientIDs = new List<string>();
            foreach (Client client in clients)
            {
                clientIDs.Add(client.clientSerial);
            }

            clientManager.SendToClients(clientIDs,"Start", new Dictionary<string, string>());
        }
        public void SendStopSession(IList clients)
        {
            List<string> clientIDs = new List<string>();
            foreach (Client client in clients)
            {
                clientIDs.Add(client.clientSerial);
            }

            clientManager.SendToClients(clientIDs, "Stop", new Dictionary<string, string>());
        }

        public static void OpenSelectClientWindow()
        {
            SelectClientHistory selectClientHistory = new SelectClientHistory();
            selectClientHistory.ShowDialog();
        }

        public static void OpenHistoryWindow()
        {
            ClientHistoryWindow clientHistoryWindow = new ClientHistoryWindow();
            clientHistoryWindow.ShowDialog();
        }
        public static void OpenHistoryWindow(string clientID)
        {
            ClientHistoryWindow clientHistoryWindow = new ClientHistoryWindow(clientID);
            clientHistoryWindow.ShowDialog();
        }

        //public static void HistoryWindow(Client client)
        //{
        //    ClientHistoryWindow clientHistoryWindow = new ClientHistoryWindow(client.clientSerial);
        //    clientHistoryWindow.ShowDialog();
        //}

    }
}
