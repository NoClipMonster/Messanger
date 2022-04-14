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
        public delegate void ContactHandler(string name);
        public event ContactHandler OnMouseClick;
        public Protocol.Data.Answer.User user;
        public Protocol.Data.Answer.Group group;
        public bool isGroup { get { return (group != null && user == null); } }
        string Id;
        int positionIndex;
        public int Position
        {
            get { return positionIndex; }
            set { positionIndex = value; position.Content = value; }
        }
        public ContactUC(Protocol.Data.Answer.Group Group, DateTime date)
        {
            InitializeComponent();
            group = Group;
            name.Content = group.Name;
            VerticalAlignment = VerticalAlignment.Top;
            Id = Group.Id;
        }
        public ContactUC(Protocol.Data.Answer.User User, DateTime date)
        {
            InitializeComponent();
            user= User;
            name.Content = user.Name;
            VerticalAlignment = VerticalAlignment.Top;
            Id = User.Id;
        }

        public void ChangeMessage(string message, DateTime date)
        {
            position.Content = 0;
            lastmsg.Content = message;
            lasttime.Content = date.ToShortTimeString();
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            contactBorder.Background.Opacity += 0.5;
        }

        private void contactBorder_MouseLeave(object sender, MouseEventArgs e)
        {
           contactBorder.Background.Opacity= 0;
        }

        private void contactBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            contactBorder.Background.Opacity += 0.5;
        }

        private void contactBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            contactBorder.Background.Opacity -= 0.5;
            OnMouseClick(Id);
        }

    }
}
