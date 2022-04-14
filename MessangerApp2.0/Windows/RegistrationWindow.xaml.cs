using Protocol;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static Protocol.Data;

namespace MessangerApp2._0
{
    /// <summary>
    /// Interaction logic for RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        ClientSideModule clientSideModule;
        public RegistrationWindow()
        {
            clientSideModule = new ClientSideModule();
            clientSideModule.OnAnswerReceived += ClientSideModule_OnAnswerReceived;
            clientSideModule.CheckSession();
            InitializeComponent();
        }

        private void ClientSideModule_OnAnswerReceived(dynamic message)
        {
            
            if (message is Data.StatusType status)
            {
                switch (status)
                {
                    case Data.StatusType.Authorized:
                        MessageBox.Show("Регистрация прошла успешно.");
                        break;
                    case Data.StatusType.Ok:
                       
                        new MainWindow(clientSideModule).Show();
                        Close();
                        break;
                    case Data.StatusType.AuthorizationDenied:
                        MessageBox.Show("Данные введены неверно");
                        break;
                    case Data.StatusType.UserExists:
                        MessageBox.Show("Пользователь с таким логином уже существует");
                        break;
                    case StatusType.Error:
                        MessageBox.Show("Ошибка");
                        break;
                }
               
            }


            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RegBT.IsEnabled = false;
            LoginBT.IsEnabled = false;
            clientSideModule.Login(LoginTB.Text, PasswordTB.Text);
            RegBT.IsEnabled = true;
            LoginBT.IsEnabled = true;
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            RegBT.IsEnabled = false;
            LoginBT.IsEnabled = false;
            clientSideModule.Registration(regLoginTB.Text, regPasswordTB.Text,regNameTB.Text,"");
            RegBT.IsEnabled = true;
            LoginBT.IsEnabled = true;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            new MainWindow(clientSideModule).Show();
            Close();
        }
    }
}
