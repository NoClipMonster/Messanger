using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using static Protocol.Data;

namespace MessangerApp2._0
{

    //TODO: Memberships,Файлы, групповые сообщения

    public partial class MainWindow : Window
    {
        ClientSideModule clientSideModule;

        string SelectedChat = "";
        Dictionary<string, Grid> chatGrids;
        Dictionary<string, ContactUC> contacts;

        FindUser findUser;
        Controls.CreateGroup createGroup;

        BlurEffect myBlur = new() { Radius = 0, KernelType = KernelType.Gaussian, RenderingBias = RenderingBias.Quality };
        Timer timer;
        public MainWindow(ClientSideModule clientSideModule)
        {
            InitializeComponent();
            MainGrid.Effect = myBlur;
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

            timer = new Timer(new TimerCallback(clientSideModule.CheckForMessage), null, 0, 250);
        }

        private void ClientSideModule_OnAnswerReceived(dynamic message)
        {
            switch (message)
            {
                case StatusType status:
                    switch (status)
                    {
                        case StatusType.InvalidSessionId:
                            clientSideModule.Disconection();

                            new RegistrationWindow().Show();
                            Close();
                            break;
                        case StatusType.GroupCreated:
                            MessageBox.Show("Группа успешно созданна");
                            clientSideModule.FindGroup(createGroup.IdBox.Text);
                            CreateGroup_OnClose();
                            status.ToString();
                            break;
                        case StatusType.GroupExists:
                            MessageBox.Show("Такая группа уже существует");
                            break;
                        case StatusType.GroupNOTExists:
                            MessageBox.Show("Такая группа не существует");
                            findUser.IsUIBlocked = false;
                            break;
                        case StatusType.UserNOTExists:
                            MessageBox.Show("Такой человек не существует");
                            findUser.IsUIBlocked = false;
                            break;
                    }
                    break;

                case Answer.Group group:
                    findUser.IsUIBlocked = false;
                    FindUser_OnClose();
                    AddGroup(group);
                    break;
                case Answer.User user:
                    findUser.IsUIBlocked = false;
                    FindUser_OnClose();
                    AddUser(user);
                    break;

                default:
                    MessageBox.Show("Произошло что то странное: " + (message).ToString()); break;
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
                clientSideModule.FindGroup(findUser.userNameBox.Text);
            else
                clientSideModule.FindUser(findUser.userNameBox.Text);


        }

        private void createGroupButton_Click(object sender, RoutedEventArgs e)
        {
            clientSideModule.CreateGroup(createGroup.IdBox.Text, createGroup.NameBox.Text, createGroup.DescrBox.Text);
        }

        private void CreateGroup_OnClose()
        {
            createGroup.Visibility = Visibility.Hidden;
            createGroup.IsUIBlocked = false;
            DoubleAnimation an = new() { From = 10, To = 0, Duration = TimeSpan.FromSeconds(0.25) };
            (MainGrid.Effect as BlurEffect).BeginAnimation(BlurEffect.RadiusProperty, an);
        }

        private void FindUser_OnClose()
        {
            findUser.Visibility = Visibility.Hidden;
            DoubleAnimation an = new() { From = 10, To = 0, Duration = TimeSpan.FromSeconds(0.25) };
            (MainGrid.Effect as BlurEffect).BeginAnimation(BlurEffect.RadiusProperty, an);
        }
        private void ClientSideModule_OnMessageReceived(dynamic message)
        {
            if (message is Answer.Message[] messages)
            {
                clientSideModule.messages.Add(messages);

                foreach (var item in messages)
                {
                    if (item.FileId != "")
                    {
                        if (!Directory.Exists(clientSideModule.settings.FilesPath))
                            Directory.CreateDirectory(clientSideModule.settings.FilesPath);
                        Answer.File file = clientSideModule.GetFileNow(item.FileId);
                        File.WriteAllBytes(clientSideModule.settings.FilesPath + "\\" + file.FileName, file.FileData);
                    }
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
            ShowMessage(ms.SenderID, ms.RecipientID, ms.Text, ms.Date, ms.IsGroup, IsNew,ms.FileId);
        }
        void ShowMessage(string SenderId, string RecipientId, string text, DateTime dateTime, bool isGroup, bool IsNew = true,string FileId = "")
        {
            if (isGroup)
            {
                if (!contacts.ContainsKey(RecipientId))
                {
                    AddGroup(clientSideModule.FindGroupNow(RecipientId));             
                }
                if (RecipientId != SelectedChat)
                    contacts[RecipientId].newMessageIndicator.Visibility = Visibility.Visible;
                if (clientSideModule.settings.LastMessageRecieve < dateTime)
                    clientSideModule.settings.LastMessageRecieve = dateTime;
                MessageControl message = new(text, dateTime, SenderId, SenderId == clientSideModule.UserId,FileId);
                if (FileId != null)
                {
                    message.OnFileOpenClick += Message_OnFileOpenClick;
                }
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
               // Visibility = contacts[RecipientId].newMessageIndicator.Visibility;
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
                    AddUser(clientSideModule.FindUserNow(notCurentUser));
                }

                if (notCurentUser != SelectedChat)
                    contacts[notCurentUser].newMessageIndicator.Visibility = Visibility.Visible;
                bool isCurentUserSender = SenderId == clientSideModule.UserId;
                MessageControl message = new(text, dateTime, SenderId, isCurentUserSender, FileId);
                if (FileId != null)
                {
                    message.OnFileOpenClick += Message_OnFileOpenClick;
                }
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

                    contacts[RecipientId].ChangeMessage(text, dateTime);
                    if (!IsNew)
                        contacts[RecipientId].newMessageIndicator.Visibility = clientSideModule.contacts.GetNewMessage(RecipientId) ? Visibility.Visible : Visibility.Hidden;
                    else
                        clientSideModule.contacts.SetNewMessage(RecipientId, true);

                    PlaceUserOnTop(RecipientId);
                }
                else
                {

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

        private void Message_OnFileOpenClick(string FileId)
        {
            if (FileId == "")
                return;
            string path = clientSideModule.settings.FilesPath;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
           
            string FileName = clientSideModule.GetFileNameNow(FileId);
            if (!File.Exists(path + "\\" + FileName))
            {
                Answer.File file = clientSideModule.GetFileNow(FileId);
                File.WriteAllBytes(path + "\\" + file.FileName, file.FileData);
            }
            Process.Start(new ProcessStartInfo("explorer.exe", " /select, " + path + "\\" + FileName));
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
            DoubleAnimation an = new() { From = 0, To = 10, Duration = TimeSpan.FromSeconds(0.25) };
            (MainGrid.Effect as BlurEffect).BeginAnimation(BlurEffect.RadiusProperty, an);
            findUser.userNameBox.Focus();
        }

        bool isExpandedUsersColumn = true;
        void UsersColumnState(bool isExpanded)
        {
            isExpandedUsersColumn = isExpanded;
            if (isExpanded)
            {
                GridLengthAnimation col = new() { From = FirstColumnDef.Width, To = new GridLength(330), Duration = TimeSpan.FromSeconds(0.25) };
                FirstColumnDef.BeginAnimation(ColumnDefinition.WidthProperty, col);

                UsersButton.Content = "ᐊ";
            }
            else
            {
                GridLengthAnimation col = new() { From = FirstColumnDef.Width, To = new GridLength(30), Duration = TimeSpan.FromSeconds(0.25) };

                FirstColumnDef.BeginAnimation(ColumnDefinition.WidthProperty, col);

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

                ThicknessAnimation col = new() { From = MenuGrid.Margin, To = thic, Duration = TimeSpan.FromSeconds(0.25) };
                MenuGrid.BeginAnimation(Grid.MarginProperty, col);


                DoubleAnimation an = new() { From = 0, To = 10, Duration = TimeSpan.FromSeconds(0.25) };
                (MainGrid.Effect as BlurEffect).BeginAnimation(BlurEffect.RadiusProperty, an);

                HeadGrid.PreviewMouseDown += HeadGrid_PreviewMouseDown;
                HeadGrid.KeyDown += HeadGrid_KeyDown;
            }
            else
            {
                Thickness thic = MenuGrid.Margin;
                thic.Left = -MenuGrid.Width;

                ThicknessAnimation col = new() { From = MenuGrid.Margin, To = thic, Duration = TimeSpan.FromSeconds(0.25) };
                MenuGrid.BeginAnimation(Grid.MarginProperty, col);

                DoubleAnimation an = new() { From = 10, To = 0, Duration = TimeSpan.FromSeconds(0.25) };
                (MainGrid.Effect as BlurEffect).BeginAnimation(BlurEffect.RadiusProperty, an);

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
            clientSideModule.settings.LastMessageRecieve = DateTime.MinValue;
            ContactsGrid.Children.Clear();
            MessageScroller.Content = null;
            contacts.Clear();
            chatGrids.Clear();
        }

        private void CreateGroup_Click(object sender, RoutedEventArgs e)
        {
            createGroup.Visibility = Visibility.Visible;
            DoubleAnimation an = new() { From = 0, To = 10, Duration = TimeSpan.FromSeconds(0.25) };
            (MainGrid.Effect as BlurEffect).BeginAnimation(BlurEffect.RadiusProperty, an);
            createGroup.IdBox.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new();
           if((bool)ofd.ShowDialog())
            {
                string FileId= clientSideModule.SendFileNow(ofd.FileName);
                clientSideModule.SendMessage(SelectedChat, ofd.SafeFileName, contacts[SelectedChat].isGroup, FileId);
            }
        }

        private void GetFirstFile_Click(object sender, RoutedEventArgs e)
        {
           /* string path = clientSideModule.settings.FilesPath;
            Answer.File file = clientSideModule.GetFileNow(0);
            if(!Directory.Exists(path))
                Directory.CreateDirectory(path);
            File.WriteAllBytes(path + "\\" + file.FileName, file.FileData);
            Process.Start(new ProcessStartInfo("explorer.exe", " /select, " + path + "\\" + file.FileName));*/
            /* if (!File.Exists(path + "\\" + FileName))
             {
                 Answer.File file = clientSideModule.GetFileNow(FileId);
                 File.WriteAllBytes(path + "\\" + file.FileName, file.FileData);
             }
             */
        }
    }
    public class GridLengthAnimation : AnimationTimeline
    {
        public GridLengthAnimation()
        {
            // no-op
        }

        public GridLength From
        {
            get { return (GridLength)GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }

        public static readonly DependencyProperty FromProperty =
          DependencyProperty.Register("From", typeof(GridLength), typeof(GridLengthAnimation));

        public GridLength To
        {
            get { return (GridLength)GetValue(ToProperty); }
            set { SetValue(ToProperty, value); }
        }

        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register("To", typeof(GridLength), typeof(GridLengthAnimation));

        public override Type TargetPropertyType
        {
            get { return typeof(GridLength); }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new GridLengthAnimation();
        }

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            double fromValue = this.From.Value;
            double toValue = this.To.Value;

            if (fromValue > toValue)
            {
                return new GridLength((1 - animationClock.CurrentProgress.Value) * (fromValue - toValue) + toValue, this.To.IsStar ? GridUnitType.Star : GridUnitType.Pixel);
            }
            else
            {
                return new GridLength((animationClock.CurrentProgress.Value) * (toValue - fromValue) + fromValue, this.To.IsStar ? GridUnitType.Star : GridUnitType.Pixel);
            }
        }
    }
}
