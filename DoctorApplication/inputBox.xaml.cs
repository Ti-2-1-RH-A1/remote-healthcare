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
    /// Interaction logic for inputBox.xaml
    /// </summary>
    public partial class inputBox : Window
    {
        private string message;

        public inputBox(string message ="Enter value below")
        {
            this.message = message;
            InitializeComponent();
            lblMessage.Content = this.message;
             
        }

        public string ResponseText
        {
            get { return ValueResponseBox.Text; }
            set { ValueResponseBox.Text = value; }
        }

        private void SendBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }
        
    }
}
