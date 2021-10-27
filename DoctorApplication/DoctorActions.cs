using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public void OpenSelectClientWindow()
        {
            selectClientHistory = new SelectClientHistory(mainWindow);
            clientManager.RequestHistoryClients();
            selectClientHistory.ShowDialog();
        }

        public void OpenHistoryWindow(Client client)
        {
            string clientID = client.clientSerial;
            clientHistoryWindow = new ClientHistoryWindow();
            clientManager.RequestHistoryData(clientID);
            clientHistoryWindow.ShowDialog();
        }

        public void UpdateSelectWindow(Dictionary<string, string> data)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate {
                foreach (KeyValuePair<string, string> entry in data)
                {
                    Client client = new Client();
                    string[] row = { entry.Key, entry.Value };
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
