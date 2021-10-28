using System.Windows;

namespace DoctorApplication
{
    /// <summary>
    /// Interaction logic for MessageAll.xaml
    /// </summary>
    public partial class MessageAll : Window
    {
        private DoctorActions doctorActions;

        public MessageAll(MainWindow main)
        {
            doctorActions = main.doctorActions;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            doctorActions.SendToAll(TBMessageToSend.Text);
            TBMessageToSend.Clear();
        }
    }
}
