using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using static Protocol.Data;

namespace MessangerApp2._0
{
    

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ClientSideModule clientSideModule;
        string SelectedChat = "";
        Dictionary<string, Grid> chatGrids;
        Dictionary<string, Contact> contacts;
        public MainWindow(ClientSideModule clientSideModule)
        {
            InitializeComponent();
            this.clientSideModule = clientSideModule;
            contacts = new();
            chatGrids = new();
           // clientSideModule.OnMessageReceived += ClientSideModule_OnMessageReceived;
          //  Title = clientSideModule.login;
            if (ContactsGrid.Children.Count == 0)
            {
                AddUser(Title);
            }
        }

        private void ClientSideModule_OnMessageReceived(dynamic message)
        {
            if(message is DirectMessage)
            {
                DirectMessage directMessage = (DirectMessage)message;
                ShowMessage(directMessage.senderLogin,directMessage.message,directMessage.sendingTime,false,chatGrids[directMessage.senderLogin]);
                if(directMessage.senderLogin!=SelectedChat)
                  contacts[directMessage.senderLogin].contactUC.newMessageIndicator.Visibility = Visibility.Visible;
            }
        }

        void ContactClicked(string name)
        {
            SelectedChat = name;
            MessageScroller.Content = chatGrids[name];
            contacts[name].contactUC.newMessageIndicator.Visibility = Visibility.Hidden;
        }

        void AddUser(string Name)
        {
            ContactUC contactUC = new(Name, DateTime.Now);
            if (!contacts.TryAdd(Name, new Contact(Name, contactUC)))
            {
                contactUC = null;
                return;
            }
            contactUC.OnMouseClick += ContactClicked;
            contactUC.Margin = new(10, 10 + ((ContactsGrid.Children.Count != 0) ? (ContactsGrid.Children[^1] as ContactUC).Margin.Top + contactUC.Height : 0), 10, 0);
            ContactsGrid.Children.Add(contactUC);
            chatGrids.Add(Name, new Grid());
            chatGrids[Name].SizeChanged += MessageGrid_SizeChanged;
            chatGrids[Name].ColumnDefinitions.Add(new ColumnDefinition());
            chatGrids[Name].ColumnDefinitions.Add(new ColumnDefinition());
        }
        void ShowMessage(string sender,string text, DateTime dateTime, bool isRightSide,Grid grid)
        {
            MessageControl message = new(text, dateTime, isRightSide);
            double height = (grid.Children.Count != 0) ? (grid.Children[^1] as MessageControl).Margin.Top + (grid.Children[^1] as MessageControl).ActualHeight : 0;
            Thickness thickness = message.Margin;
            thickness.Top = 20 + height;
            message.Margin = thickness;

            if (isRightSide)
            {
                Grid.SetColumn(message, 1);
            }
            else
            {
                Grid.SetColumn(message, 0);
            }

            grid.Children.Add(message);
            if(isRightSide)
            contacts[sender].AddMessage(new("me",sender, text, dateTime));
            else
                contacts[sender].AddMessage(new( sender, "me", text, dateTime));


            MessageScroller.ScrollToBottom();
        }

        private void SendButtonClick(object sender, RoutedEventArgs e)
        {
        /*    if (clientSideModule.SendMessage(new DirectMessage() { targetLogin = SelectedChat, message = textInputBox.Text, senderLogin = clientSideModule.login }))
                ShowMessage(SelectedChat,textInputBox.Text, DateTime.Now, true,chatGrids[SelectedChat]);

            else
                MessageBox.Show("Нет подключения");*/
        }


        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            FirstColumnDef.Width = new(325);
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            FirstColumnDef.Width = new(25);
        }

        private void MessageGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (chatGrids[SelectedChat].Children.Count > 1)
                for (int i = 1; i < chatGrids[SelectedChat].Children.Count; i++)
                {
                    Grid gr = (e.Source as Grid);
                    double height = (gr.Children[i - 1] as MessageControl).Margin.Top + (gr.Children[i - 1] as MessageControl).ActualHeight;
                    Thickness thickness = ((MessageControl)gr.Children[i]).Margin;
                    thickness.Top = 20 + height;
                    (gr.Children[i] as MessageControl).Margin = thickness;
                }

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            FindUser findUser = new FindUser();
            Grid.SetColumn(findUser, 0);
            Grid.SetRow(findUser, 0);
            Grid.SetColumnSpan(findUser, 3);
            Grid.SetRowSpan(findUser, 2);
            findUser.Width = ActualWidth;
            findUser.Height = ActualHeight;
            for (int i = 0; i < MainGrid.Children.Count; i++)
            {
                MainGrid.Children[i].Effect = new BlurEffect() { Radius = 5, KernelType = KernelType.Gaussian, RenderingBias = RenderingBias.Performance };
            }
            findUser.mainButton.Click += MainButton_Click;
            MainGrid.Children.Add(findUser);
        }

        private void MainButton_Click(object sender, RoutedEventArgs e)
        {
            AddUser((MainGrid.Children[^1] as FindUser).userNameBox.Text);

            MainGrid.Children.RemoveAt(MainGrid.Children.Count - 1);
            for (int i = 0; i < MainGrid.Children.Count; i++)
            {
                MainGrid.Children[i].Effect = null;
            }

        }
    }
}
