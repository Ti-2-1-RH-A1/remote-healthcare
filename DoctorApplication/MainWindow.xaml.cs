using System.Threading.Tasks;
using System.Windows;

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
            doctorActions = new DoctorActions();
            Task start = doctorActions.Start();
            start.Wait();
            InitializeComponent();
            
        }

        private void BtnBroadcast_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            DoctorActions.OpenSelectClientWindow();
        }

        private void btnMessage_Click(object sender, RoutedEventArgs e)
        {
            doctorActions.SendToAll("test");

        }
    }
}
