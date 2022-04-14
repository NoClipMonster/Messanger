using System;
using System.Collections.Generic;
using System.Threading;
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
        Dictionary<string, ContactUC> contacts;

        FindUser findUser;
        Controls.CreateGroup createGroup;

        BlurEffect myBlur = new BlurEffect() { Radius = 6, KernelType = KernelType.Gaussian, RenderingBias = RenderingBias.Quality };
        Timer timer;
        public MainWindow(ClientSideModule clientSideModule)
        {
            InitializeComponent();

            this.clientSideModule = clientSideModule;
            contacts = new();
            chatGrids = new();

            SetColorTheme();

            InitializeCustomComponents();
            this.clientSideModule.OnMessagesReceived += ClientSideModule_OnMessageReceived;
            this.clientSideModule.OnAnswerReceived += ClientSideModule_OnAnswerReceived;
            Title = this.clientSideModule.UserName;

            foreach (var item in this.clientSideModule.contacts.GetAll())
            {
                if (item.IsGroup)
                    AddGroup(new Answer.Group() { Id = item.Id, Name = item.Name, Description = item.Description }, false, item.HasNewMessage);
                else
                    AddUser(new Answer.User() { Id = item.Id, Name = item.Name, Description = item.Description }, false, item.HasNewMessage);
            }

            foreach (var item in this.clientSideModule.messages.GetAll())
            {
                ShowMessage(item, false);
            }

            // timer = new Timer(new TimerCallback(clientSideModule.CheckForMessage), null, 0, 2000);
        }

        private void ClientSideModule_OnAnswerReceived(dynamic message)
        {

            if (message is StatusType status)
            {
                switch (status)
                {
                    case StatusType.GroupCreated:
                        dynamic answer = clientSideModule.FindGroup(createGroup.IdBox.Text);
                        if (answer is Answer.Group group)
                            AddGroup(group);
                        CreateGroup_OnClose();
                        break;
                    case StatusType.GroupExists:
                        MessageBox.Show("Такая группа уже существует");
                        break;
                }
            }
        }

        void SetColorTheme()
        {
            Background = clientSideModule.settings.MainColor;
            MainGrid.Background = clientSideModule.settings.MainColor;
            UsersField.Background = clientSideModule.settings.AdditionalColor;
            TextEnterField.Background = clientSideModule.settings.AdditionalColor;
            MenuGrid.Background = clientSideModule.settings.AdditionalColor;
        }
        void InitializeCustomComponents()
        {
            findUser = new();

            findUser.Width = Width;
            findUser.Height = Height;
            findUser.mainButton.Click += findUserButton_Click;
            findUser.OnClose += FindUser_OnClose;
            findUser.Visibility = Visibility.Hidden;
            HeadGrid.Children.Add(findUser);


            createGroup = new();

            createGroup.Width = Width;
            createGroup.Height = Height;
            createGroup.mainButton.Click += createGroupButton_Click;
            createGroup.OnClose += CreateGroup_OnClose;
            createGroup.Visibility = Visibility.Hidden;
            HeadGrid.Children.Add(createGroup);
        }

        private void findUserButton_Click(object sender, RoutedEventArgs e)
        {
            findUser.IsUIBlocked = true;
            if ((bool)findUser.IsGroup.IsChecked)
            {
                dynamic answer = clientSideModule.FindGroup(findUser.userNameBox.Text);

                if (answer is Answer.Group group && group.Id != "")
                {
                    AddGroup(group);

                    FindUser_OnClose();
                }
                else MessageBox.Show("Группа не найдена");
            }
            else
            {
                dynamic answer = clientSideModule.FindUser(findUser.userNameBox.Text);

                if (answer is Answer.User user && user.Id != "")
                {
                    AddUser(user);
                    FindUser_OnClose();
                }
                else MessageBox.Show("Человек не найдена");
            }

            findUser.IsUIBlocked = false;
        }

        private void createGroupButton_Click(object sender, RoutedEventArgs e)
        {
            createGroup.IsUIBlocked = true;
            clientSideModule.CreateGroup(createGroup.IdBox.Text, createGroup.NameBox.Text, createGroup.DescrBox.Text);
        }

        private void CreateGroup_OnClose()
        {
            createGroup.Visibility = Visibility.Hidden;
            createGroup.IsUIBlocked = false;
            MainGrid.Effect = null;
        }

        private void FindUser_OnClose()
        {
            findUser.Visibility = Visibility.Hidden;

            MainGrid.Effect = null;
        }

        private void ClientSideModule_OnMessageReceived(dynamic message)
        {
            if (message is Answer.Message[] messages)
            {
                clientSideModule.messages.Add(messages);
                foreach (var item in messages)
                {
                    ShowMessage(item);
                }
            }
        }

        void ContactClicked(string Login)
        {
            SelectedChat = Login;
            MessageScroller.Content = chatGrids[Login];
            clientSideModule.contacts.SetNewMessage(Login, false);
            contacts[Login].newMessageIndicator.Visibility = Visibility.Hidden;
        }

        void PlaceUserOnTop(string userId)
        {
            int contactplace = contacts[userId].Position;

            foreach (ContactUC contact in contacts.Values)
            {
                if (contact.Position <= contactplace)
                {
                    if (contact.user != null)
                    {
                        if (contact.user.Id != userId)
                            contact.Position++;
                        else { contact.Position = 0; }
                    }
                    else
                    {
                        if (contact.group.Id != userId)
                            contact.Position++;
                        else { contact.Position = 0; }
                    }

                    contact.Margin = new(10, 10 + contact.Height * contact.Position, 10, 0);
                }

            }
        }
        void AddGroup(Answer.Group group, bool IsNew = true, bool HasNewMessage = false)
        {
            if (IsNew)
                clientSideModule.contacts.Add(group);
            ContactUC contactUC = new(group, DateTime.Now);
            if (!contacts.TryAdd(group.Id, contactUC))
            {
                contactUC = null;
                return;
            }
            contactUC.OnMouseClick += ContactClicked;
            contactUC.newMessageIndicator.Visibility = HasNewMessage ? Visibility.Visible : Visibility.Hidden;
            ContactsGrid.Children.Add(contactUC);
            chatGrids.Add(group.Id, new Grid());
            chatGrids[group.Id].SizeChanged += MessageGrid_SizeChanged;
            chatGrids[group.Id].ColumnDefinitions.Add(new ColumnDefinition());
            chatGrids[group.Id].ColumnDefinitions.Add(new ColumnDefinition());
            contacts[group.Id].Position = int.MaxValue;
            PlaceUserOnTop(group.Id);

        }
        void AddUser(Answer.User user, bool IsNew = true, bool HasNewMessage = false)
        {
            if (IsNew)
                clientSideModule.contacts.Add(user);
            ContactUC contactUC = new(user, DateTime.Now);
            if (!contacts.TryAdd(user.Id, contactUC))
            {
                return;
            }
            contactUC.OnMouseClick += ContactClicked;
            contactUC.newMessageIndicator.Visibility = HasNewMessage ? Visibility.Visible : Visibility.Hidden;
            ContactsGrid.Children.Add(contactUC);
            chatGrids.Add(user.Id, new Grid());
            chatGrids[user.Id].SizeChanged += MessageGrid_SizeChanged;
            chatGrids[user.Id].ColumnDefinitions.Add(new ColumnDefinition());
            chatGrids[user.Id].ColumnDefinitions.Add(new ColumnDefinition());
            contacts[user.Id].Position = int.MaxValue;
            PlaceUserOnTop(user.Id);
        }
        void ShowMessage(Answer.Message ms, bool IsNew = true)
        {
            ShowMessage(ms.SenderID, ms.RecipientID, ms.Text, ms.Date, ms.IsGroup, IsNew);
        }
        void ShowMessage(string SenderId, string RecipientId, string text, DateTime dateTime, bool isGroup, bool IsNew = true)
        {
            if (isGroup)
            {
                if (!contacts.ContainsKey(RecipientId))
                {
                    dynamic answer = clientSideModule.FindGroup(RecipientId);
                    if (answer is Answer.Group group)
                        AddGroup(group);

                }
                if (RecipientId != SelectedChat)
                    contacts[RecipientId].newMessageIndicator.Visibility = Visibility.Visible;

                MessageControl message = new(text, dateTime, SenderId, SenderId == clientSideModule.UserId);
                double height = (chatGrids[RecipientId].Children.Count != 0) ? (chatGrids[RecipientId].Children[^1] as MessageControl).Margin.Top + (chatGrids[RecipientId].Children[^1] as MessageControl).ActualHeight : 0;
                Thickness thickness = message.Margin;
                thickness.Top = 20 + height;
                message.Margin = thickness;

                int index = chatGrids[RecipientId].Children.Add(message);
                for (int i = index - 1; i >= 0; i--)
                {
                    if ((chatGrids[RecipientId].Children[i] as MessageControl).data.DateTime > (chatGrids[RecipientId].Children[index] as MessageControl).data.DateTime)
                    {
                        MessageControl newMS = new(chatGrids[RecipientId].Children[index] as MessageControl);
                        MessageControl MS = new(chatGrids[RecipientId].Children[i] as MessageControl);
                        chatGrids[RecipientId].Children.RemoveAt(i);
                        chatGrids[RecipientId].Children.Insert(i, newMS);
                        chatGrids[RecipientId].Children.RemoveAt(index);
                        chatGrids[RecipientId].Children.Insert(index, MS);
                        index = i;
                    }
                }
                Visibility = contacts[RecipientId].newMessageIndicator.Visibility;
                contacts[RecipientId].ChangeMessage(text, dateTime);
                if (!IsNew)
                    contacts[RecipientId].newMessageIndicator.Visibility = clientSideModule.contacts.GetNewMessage(RecipientId) ? Visibility.Visible : Visibility.Hidden;
                else
                    clientSideModule.contacts.SetNewMessage(RecipientId);
                PlaceUserOnTop(contacts[RecipientId].group.Id);

            }
            else
            {
                string notCurentUser;
                if (SenderId == clientSideModule.UserId)
                    notCurentUser = RecipientId;
                else
                    notCurentUser = SenderId;
                if (!contacts.ContainsKey(notCurentUser))
                {
                    dynamic answer = clientSideModule.FindUser(notCurentUser);
                    if (answer is Answer.User user)
                        AddUser(user);
                }

                if (notCurentUser != SelectedChat)
                    contacts[notCurentUser].newMessageIndicator.Visibility = Visibility.Visible;
                bool isCurentUserSender = SenderId == clientSideModule.UserId;
                MessageControl message = new(text, dateTime, SenderId, isCurentUserSender);
                double height = (chatGrids[notCurentUser].Children.Count != 0) ? (chatGrids[notCurentUser].Children[^1] as MessageControl).Margin.Top + (chatGrids[notCurentUser].Children[^1] as MessageControl).ActualHeight : 0;
                Thickness thickness = message.Margin;
                thickness.Top = 20 + height;
                message.Margin = thickness;

                int index = chatGrids[notCurentUser].Children.Add(message);
                for (int i = index - 1; i >= 0; i--)
                {
                    if ((chatGrids[notCurentUser].Children[i] as MessageControl).data.DateTime > (chatGrids[notCurentUser].Children[index] as MessageControl).data.DateTime)
                    {
                        MessageControl newMS = new(chatGrids[notCurentUser].Children[index] as MessageControl);
                        MessageControl MS = new(chatGrids[notCurentUser].Children[i] as MessageControl);
                        chatGrids[notCurentUser].Children.RemoveAt(i);
                        chatGrids[notCurentUser].Children.Insert(i, newMS);
                        chatGrids[notCurentUser].Children.RemoveAt(index);
                        chatGrids[notCurentUser].Children.Insert(index, MS);
                        index = i;
                    }
                }

                if (isCurentUserSender)
                {
                    Visibility = contacts[RecipientId].newMessageIndicator.Visibility;
                    contacts[RecipientId].ChangeMessage(text, dateTime);
                    if (!IsNew)
                        contacts[RecipientId].newMessageIndicator.Visibility = clientSideModule.contacts.GetNewMessage(RecipientId) ? Visibility.Visible : Visibility.Hidden;
                    else
                        clientSideModule.contacts.SetNewMessage(RecipientId, true);

                    PlaceUserOnTop(RecipientId);
                }
                else
                {
                    Visibility = contacts[SenderId].newMessageIndicator.Visibility;
                    contacts[SenderId].ChangeMessage(text, dateTime);
                    if (!IsNew)
                        contacts[SenderId].newMessageIndicator.Visibility = clientSideModule.contacts.GetNewMessage(RecipientId) ? Visibility.Visible : Visibility.Hidden;
                    else
                        clientSideModule.contacts.SetNewMessage(SenderId, true);
                    PlaceUserOnTop(SenderId);
                }
            }

            MessageScroller.ScrollToBottom();
        }

        private void SendButtonClick(object sender, RoutedEventArgs e)
        {
            if (SelectedChat == "")
                return;
            //  ShowMessage(clientSideModule.UserId, SelectedChat, textInputBox.Text, DateTime.Now);
            clientSideModule.SendMessage(SelectedChat, textInputBox.Text, contacts[SelectedChat].isGroup);
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

        private void FindSomeOne_Click(object sender, RoutedEventArgs e)
        {
            findUser.Visibility = Visibility.Visible;
            MainGrid.Effect = myBlur;
            findUser.userNameBox.Focus();
        }

        bool isExpandedUsersColumn = true;
        void UsersColumnState(bool isExpanded)
        {
            isExpandedUsersColumn = isExpanded;
            if (isExpanded)
            {
                FirstColumnDef.Width = new(330);
                UsersButton.Content = "ᐊ";
            }
            else
            {
                FirstColumnDef.Width = new(30);
                UsersButton.Content = "ᐅ";
            }
        }

        bool isExpandedMenuColumn = false;
        void MenuColumnState(bool isExpanded)
        {
            isExpandedMenuColumn = isExpanded;
            if (isExpanded)
            {
                Thickness thic = MenuGrid.Margin;
                thic.Left = 0;
                MenuGrid.Margin = thic;
                MainGrid.Effect = myBlur;
                HeadGrid.PreviewMouseDown += HeadGrid_PreviewMouseDown;
                HeadGrid.KeyDown += HeadGrid_KeyDown;
            }
            else
            {
                Thickness thic = MenuGrid.Margin;
                thic.Left = -MenuGrid.Width;
                MenuGrid.Margin = thic;
                MainGrid.Effect = null;
                HeadGrid.PreviewMouseDown -= HeadGrid_PreviewMouseDown;
                HeadGrid.KeyDown -= HeadGrid_KeyDown;
            }
        }

        private void HeadGrid_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                MenuColumnState(false);
            }
        }

        private void UsersButton_Click(object sender, RoutedEventArgs e)
        {
            UsersColumnState(!isExpandedUsersColumn);
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            MenuColumnState(!isExpandedMenuColumn);
        }

        private void HeadGrid_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(MenuGrid);
            if (pt.X < 0 || pt.X > MenuGrid.ActualWidth || pt.Y < 0 || pt.Y > MenuGrid.ActualHeight)
            {
                MenuColumnState(false);
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            clientSideModule.Disconection();

            new RegistrationWindow().Show();
            Close();
        }

        private void CheFMesButton_Click(object sender, RoutedEventArgs e)
        {
            clientSideModule.CheckForMessage(null);
        }

        private void ClearAllMsgButton_Click(object sender, RoutedEventArgs e)
        {
            clientSideModule.contacts.DeleteAll();
            clientSideModule.messages.DeleteAll();
            clientSideModule.settings.LastMessageCheck = DateTime.MinValue;
            ContactsGrid.Children.Clear();
            MessageScroller.Content = null;
            contacts.Clear();
            chatGrids.Clear();
        }

        private void CreateGroup_Click(object sender, RoutedEventArgs e)
        {
            createGroup.Visibility = Visibility.Visible;
            MainGrid.Effect = myBlur;
            createGroup.IdBox.Focus();
        }
    }
}
