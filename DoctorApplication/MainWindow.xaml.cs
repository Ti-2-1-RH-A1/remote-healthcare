using System.Windows;

namespace DoctorApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnBroadcast_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnHistory_Click(object sender, RoutedEventArgs e)
        {
            DoctorActions.HistoryWindow();
        }
    }
}
