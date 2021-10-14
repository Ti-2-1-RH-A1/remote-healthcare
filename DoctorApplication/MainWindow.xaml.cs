using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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

        public void addToList(Client client)
        {
            Dispatcher.BeginInvoke(new Action(delegate ()
            {
                ListView list = UserGrid;
                list.Items.Add(client);
            }));
        }

        public void removefromList(Client client)
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

        private void btnHistory_Click(object sender, RoutedEventArgs e)
        {
            DoctorActions.HistoryWindow();
        }

        private void BtnMessage_Click(object sender, RoutedEventArgs e)
        {
            doctorActions.SendToAll("test");
        }
    }
}
