﻿using System;
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
        private DoctorActions doctorActions;
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
            doctorActions.SendToAll("test");
        }

        private void btnStartSession_Click(object sender, RoutedEventArgs e)
        {
            ListView list = UserGrid;
            if (list.SelectedItems.Count < 1) return;

            IList lstClients = list.SelectedItems;

            doctorActions.SendStartSession(lstClients);
        }

        private void btnStopSession_Click(object sender, RoutedEventArgs e)
        {
            ListView list = UserGrid;
            if (list.SelectedItems.Count > 1 || list.SelectedItems.Count < 1) return;
            Client selClient = (Client)list.SelectedItem;
            doctorActions.SendStopSession(selClient.clientSerial);
        }
    }
}
