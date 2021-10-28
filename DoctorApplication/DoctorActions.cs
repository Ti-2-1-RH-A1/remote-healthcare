using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

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
        public void SendMessage(IList clients, string message)
        {
            List<string> clientIDs = new List<string>();
            foreach (Client client in clients)
            {
                clientIDs.Add(client.clientSerial);
            }

            clientManager.SendToClients(clientIDs, "Message", new Dictionary<string, string>()
            {
                {"Message", message}
            });
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
            clientHistoryWindow = new ClientHistoryWindow();
            clientManager.RequestHistoryData(clientID);
            clientHistoryWindow.ShowDialog();
        }

        public void UpdateSelectWindow(Dictionary<string, string> data)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate {
                foreach (KeyValuePair<string, string> entry in data)
                {
                    Client row = new Client(){ clientSerial = entry.Key, clientName = entry.Value };
                    selectClientHistory.UserGrid.Items.Add(row);
                }
            });

        }

        public void UpdateHistoryWindow(JObject data)
        {
            JArray dataArray = data["data"] as JArray;
            //JObject firstItem = (JObject)dataArray[0];
            clientHistoryWindow.Dispatcher.BeginInvoke((Action)(() =>
            {
                clientHistoryWindow.labelClientID.Content = data["id"].ToString();
                clientHistoryWindow.labelClientName.Content = data["name"].ToString();
            }));

            List<ClientData> clientDatas = new List<ClientData>();

            foreach (JObject item in dataArray)
            {
                string speed = item["speed"].ToString();
                string time = item["time"].ToString();
                string distance_traveled = item["distance_traveled"].ToString();
                string rpm = item["rpm"].ToString();
                //string heartrate = item["heartrate"].ToString();
                string heartrate = "not found";
                clientDatas.Add(new ClientData(speed,time,distance_traveled,rpm,heartrate));
            }
            clientHistoryWindow.AddClientDatas(clientDatas);
        }
    }
}
