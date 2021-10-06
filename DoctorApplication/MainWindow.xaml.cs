using System.Windows;

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
