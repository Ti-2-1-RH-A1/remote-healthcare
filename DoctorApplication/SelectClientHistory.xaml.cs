using System;
using System.Collections;
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
        // DoctorActions doctorActions;
        private DoctorActions DoctorActions;
        public SelectClientHistory(MainWindow main)
        {
            // doctorActions = new DoctorActions(main);
            DoctorActions = main.DoctorActions;
            this.DataContext = this;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("Select pressed");
            ListView list = UserGrid;

            if (list.SelectedItems.Count < 1)
            {
                MessageBox.Show("You need to have at least one client selected.", "Selection error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (list.SelectedItems.Count > 1)
            {
                MessageBox.Show("You can't select more than one client.", "Selection error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            else
            {
                DoctorActions.OpenHistoryWindow(list.SelectedItems[0] as Client);
            }
        }
    }
}