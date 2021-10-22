using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DoctorApplication
{
    /// <summary>
    /// Interaction logic for SelectClientHistory.xaml
    /// </summary>
    public partial class SelectClientHistory : Window
    {
        public SelectClientHistory()
        {
            InitializeComponent();
            Client client = new Client();
            client.clientAuthKey = "aweq1312";
            client.clientName = "Naam van client";
            client.clientSerial = "12are";
            UserGrid.Items.Add(client);

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("Select pressed");
            if(UserGrid.SelectedItems.Count > 0)
            {
                Client client = (Client)UserGrid.SelectedItem;
                DoctorActions.OpenHistoryWindow(client.clientSerial);
                Close();
            }
        }
    }
}
