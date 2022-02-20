using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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


namespace MessangerApp2._0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TcpClient client;
        public MainWindow(TcpClient Client)
        {
            InitializeComponent();
            client = Client;
        }
        int i = 0;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ContactUC contactUC = new ContactUC(i.ToString(), "Привет от " + (i).ToString(), DateTime.Now);

            contactUC.Margin = new Thickness(10, 10 + (10 + contactUC.Height) * i, 10, 0);

            MainGrid.Children.Add(contactUC);
            
            i++;
            //Grid.Column="0" Margin="10,10,10,0" VerticalAlignment="Top" Width="Auto"
        }
    }
}
