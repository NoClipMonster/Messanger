using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MessangerApp2._0
{
    /// <summary>
    /// Interaction logic for ContactUC.xaml
    /// </summary>
    public partial class ContactUC : UserControl
    {
        public ContactUC(string Name, string Message, DateTime date)
        {
            InitializeComponent();
            name.Content = Name;
            ChangeMessage(Message, date);
            VerticalAlignment = VerticalAlignment.Top;
        }
        public void ChangeMessage(string message, DateTime date)
        {
            lastmsg.Content = message;
            lasttime.Content = date.ToShortTimeString();
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            contactBorder.Background.Opacity = 100;
        }

        private void contactBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            contactBorder.Background.Opacity = 0;
        }
    }
}
