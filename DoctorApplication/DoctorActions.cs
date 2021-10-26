using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System;
using System.Windows;
using System.Windows.Controls;

namespace DoctorApplication
{
    public class DoctorActions
    {
        private ClientManager clientManager;
        private MainWindow mainWindow;
        private SelectClientHistory selectClientHistory;
        private ClientHistoryWindow clientHistoryWindow;

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
        public void SendSetResistance(IList clients, string resistance)
        {
            List<string> clientIDs = new List<string>();
            foreach (Client client in clients)
            {
                clientIDs.Add(client.clientSerial);
            }

            clientManager.SendToClients(clientIDs, "SetResistance", new Dictionary<string, string>()
            {
                { "Resistance", resistance },
            });
        }

        public void OpenSelectClientWindow()
        {
            selectClientHistory = new SelectClientHistory(mainWindow);
            clientManager.RequestHistoryClients();
            selectClientHistory.ShowDialog();
        }

        public void OpenHistoryWindow(Client client)
        {
            string clientID = client.clientSerial;
            clientHistoryWindow = new ClientHistoryWindow(client);
            clientManager.RequestHistoryData(clientID);
            clientHistoryWindow.ShowDialog();
        }

        public void UpdateSelectWindow(Dictionary<string, string> data)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate {
                foreach (KeyValuePair<string, string> entry in data)
                {
                    Client row = new Client(){ clientSerial = entry.Key, clientName = entry.Value };
                    ListViewItem listViewItem = new ListViewItem();
                    listViewItem.Content = row;
                    selectClientHistory.UserGrid.Items.Add(listViewItem);
                }
            });

        }

        public void UpdateHistoryWindow(JObject data)
        {
            JArray dataArray = data["data"] as JArray;
            JObject firstItem = (JObject)dataArray[0];
            clientHistoryWindow.labelClientID.Content = firstItem["client_id"].ToString();
            clientHistoryWindow.labelClientName.Content = firstItem["client_name"].ToString();
            foreach (JObject item in dataArray)
            {
                string speed = item["speed"].ToString();
                string time = item["time"].ToString();
                string distance_traveled = item["distance_traveled"].ToString();
                string rpm = item["rpm"].ToString();
                string heartrate = item["heartrate"].ToString();

                string[] row = { time, speed, distance_traveled, rpm, heartrate };
                var listViewItem = new ListViewItem();
                listViewItem.Content = row;
                clientHistoryWindow.UserGrid.Items.Add(listViewItem);
            }
        }
    }
}
