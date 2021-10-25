using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace DoctorApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DoctorActions doctorActions;

        public ObservableCollection<Client> Clients { get; } = new ObservableCollection<Client>();


        public MainWindow()
        {
            doctorActions = new DoctorActions(this);
            Task start = doctorActions.Start();
            start.Wait();
            InitializeComponent();
            this.DataContext = this;
        }



        /// <summary>
        /// Adds a client to the listview in de mainwindow
        /// </summary>
        /// <param name="client"></param>
        public void AddToList(Client client)
        {
            Dispatcher.BeginInvoke(new Action(delegate ()
            {
                if (!Clients.Contains(client))
                {
                    Clients.Add(client);
                }
            }));
        }

        /// <summary>
        /// Removes a client from the listview in de mainwindow
        /// </summary>
        /// <param name="client"></param>
        public void RemovefromList(Client client)
        {
            Dispatcher.BeginInvoke(new Action(delegate ()
            {
                if (Clients.Contains(client))
                {
                    Clients.Remove(client);
                }
            }));
        }

        private void BtnBroadcast_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            doctorActions.OpenSelectClientWindow();
        }

        private void BtnMessage_Click(object sender, RoutedEventArgs e)
        {
            MessageAll message = new(this);
            message.ShowDialog();
        }

        private void btnStartSession_Click(object sender, RoutedEventArgs e)
        {
            ListView list = ClientListView;
            if (list.SelectedItems.Count < 1) return;

            IList lstClients = list.SelectedItems;

            doctorActions.SendStartSession(lstClients);
        }

        private void btnStopSession_Click(object sender, RoutedEventArgs e)
        {
            ListView list = ClientListView;
            if (list.SelectedItems.Count < 1) return;
            doctorActions.SendStopSession(list.SelectedItems);
        }
    }
}
