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

namespace MessangerApp2._0.Controls
{
    /// <summary>
    /// Interaction logic for CreateGroup.xaml
    /// </summary>
    public partial class CreateGroup : UserControl
    { 
        public delegate void CloseHandler();
        public event CloseHandler OnClose;
      
        public CreateGroup()
        {
            InitializeComponent();
            Grid.SetColumn(this, 0);
            Grid.SetRow(this, 0);
            Grid.SetColumnSpan(this, 3);
            Grid.SetRowSpan(this, 2);
        }
        bool isUIBlocked = false;
       public bool IsUIBlocked
        {
            get { return isUIBlocked; }
            set { isUIBlocked = value;
                mainButton.IsEnabled = !value;
                DescrBox.IsEnabled = !value;
                IdBox.IsEnabled = !value;
                NameBox.IsEnabled = !value;
            }
        }
        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                OnClose();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(MainBorder);
            if (pt.X < 0 || pt.X > MainBorder.ActualWidth || pt.Y < 0 || pt.Y > MainBorder.ActualHeight)
                OnClose();
        }
    }
}
