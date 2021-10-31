using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public ObservableCollection<ClientData> clientDatas { get; set; }

        public void AddClientDatas(List<ClientData> clientData)
        {
            Dispatcher.BeginInvoke((Action) (() =>
            {
                clientData.ForEach(clientDatas.Add);
            }));
        }


        public ClientHistoryWindow()
        {
            clientDatas = new ObservableCollection<ClientData>();
            this.DataContext = this;
            InitializeComponent();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
