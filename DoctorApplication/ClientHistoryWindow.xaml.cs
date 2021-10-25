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
using System.Windows.Shapes;

namespace DoctorApplication
{
    /// <summary>
    /// Interaction logic for ClientHistoryWindow.xaml
    /// </summary>
    public partial class ClientHistoryWindow : Window
    {
        public ClientHistoryWindow(Client client)
        {
            InitializeComponent();
            
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
