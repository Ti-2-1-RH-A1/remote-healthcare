using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace DoctorApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DoctorActions doctorActions;

        public MainWindow()
        {
            doctorActions = new DoctorActions(this);
            Task start = doctorActions.Start();
            start.Wait();
            InitializeComponent();      
        }


        /// <summary>
        /// Adds a client to the listview in de mainwindow
        /// </summary>
        /// <param name="client"></param>
        public void AddToList(Client client)
        {
            Dispatcher.BeginInvoke(new Action(delegate ()
            {
                ListView list = UserGrid;
                list.Items.Add(client);
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
                ListView list = UserGrid;
                int index = list.Items.IndexOf(client);
                list.Items.RemoveAt(index);
            }));
        }

        private void BtnBroadcast_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            DoctorActions.OpenSelectClientWindow();
        }

        private void BtnMessage_Click(object sender, RoutedEventArgs e)
        {
            MessageAll message = new(this);
            message.ShowDialog();
        }

        private void btnStartSession_Click(object sender, RoutedEventArgs e)
        {
            ListView list = UserGrid;
            if (list.SelectedItems.Count < 1)
            {
                MessageBox.Show("You need to have at least one client selected.", "Selection error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            IList lstClients = list.SelectedItems;

            doctorActions.SendStartSession(lstClients);
        }

        private void btnStopSession_Click(object sender, RoutedEventArgs e)
        {
            ListView list = UserGrid;
            if (list.SelectedItems.Count < 1)
            {
                MessageBox.Show("You need to have at least one client selected.", "Selection error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            doctorActions.SendStopSession(list.SelectedItems);
        }

        private void btnChangeResistance_Click(object sender, RoutedEventArgs e)
        {
            ListView list = UserGrid;
            if (list.SelectedItems.Count < 1)
            {
                MessageBox.Show("You need to have at least one client selected.", "Selection error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var dialog = new inputBox("Enter resistance %");
            dialog.Owner = this;
            if (dialog.ShowDialog() == true)
            {
                doctorActions.SendSetResistance(list.SelectedItems, dialog.ResponseText);
            }


        }
    }
}
