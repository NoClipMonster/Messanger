using System.Windows;
using static Protocol.Data;

namespace MessangerApp2._0
{
    /// <summary>
    /// Interaction logic for RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {

        MainWindow? mainWindow;
        ClientSideModule clientSideModule;
        Protocol.Protocol protocol = new(4);
        public RegistrationWindow()
        {
            InitializeComponent();
            clientSideModule = new ClientSideModule();
            clientSideModule.OnAnswerReceived += ClientSideModule_OnAnswerReceived;
        }

        private void ClientSideModule_OnAnswerReceived(dynamic message)
        {
            
            switch (message)
            {
                case byte[]:

                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RegBT.IsEnabled = false;
            LoginBT.IsEnabled = false;
            // clientSideModule.TryConnect(new Command(Command.CommandType.Connection, LoginTB.Text, PasswordTB.Text));
            clientSideModule.Login(LoginTB.Text, PasswordTB.Text);
            RegBT.IsEnabled = true;
            LoginBT.IsEnabled = true;
        }

    /*    private void ClientSideModule_OnConnected(int result)
        {

            switch (result)
            {
                case 0:
                    MessageBox.Show("Пароль не верный или пользователь не существует");
                    break;

                case 1:
                    mainWindow = new MainWindow(clientSideModule);
                    mainWindow.Show();
                    Close();
                    break;

                case 2:
                    MessageBox.Show("Не удалось подключиться к серверу");
                    break;

                case 3:
                    MessageBox.Show("Такой пользователь уже существует");
                    break;

                case 4:
                default:
                    MessageBox.Show("Какая-то ошибка");
                    break;
            }
            RegBT.IsEnabled = true;
            LoginBT.IsEnabled = true;
        }*/

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (regRepPasswordTB.Text != regPasswordTB.Text)
            {
                MessageBox.Show("Пароли не совпадают");
                return;
            }
           // clientSideModule.TryConnect(new Command(Command.CommandType.Registration, regLoginTB.Text, regPasswordTB.Text));
            RegBT.IsEnabled = false;
            LoginBT.IsEnabled = false;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            mainWindow = new MainWindow(clientSideModule);
            mainWindow.Show();
            Close();
        }
    }
}
