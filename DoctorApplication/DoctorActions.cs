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

        public static void OpenSelectClientWindow()
        {
            SelectClientHistory selectClientHistory = new SelectClientHistory();
            selectClientHistory.ShowDialog();
        }

        public static void OpenHistoryWindow(string clientID)
        {
            ClientHistoryWindow clientHistoryWindow = new ClientHistoryWindow(clientID);
            clientHistoryWindow.ShowDialog();
        }

    }
}
