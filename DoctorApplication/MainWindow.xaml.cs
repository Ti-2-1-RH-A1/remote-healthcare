using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DoctorApplication
{

    

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ClientManager clientManager;
        public MainWindow()
        {
            clientManager = new ClientManager();
            Task start = clientManager.start();
            InitializeComponent();
            start.Wait();

        }

        private void btnBroadcast_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnMessage_Click(object sender, RoutedEventArgs e)
        {
            clientManager.sendMessageToAll("test");

        }
    }
}
