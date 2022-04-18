using Protocol;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using static Protocol.Data;

namespace MessangerApp2._0
{
    /// <summary>
    /// Interaction logic for RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        ClientSideModule clientSideModule;
        BlurEffect myBlur = new() { Radius = 5, KernelType = KernelType.Gaussian, RenderingBias = RenderingBias.Quality };
        public RegistrationWindow()
        {
            clientSideModule = new ClientSideModule();
            clientSideModule.OnAnswerReceived += ClientSideModule_OnAnswerReceived;
            InitializeComponent();
            MainGrid.Effect = myBlur;
            clientSideModule.CheckSession();
            RegBT.IsEnabled = false;
            LoginBT.IsEnabled = false;
        }

        private void ClientSideModule_OnAnswerReceived(dynamic message)
        {
            

            if (message is Data.StatusType status)
            {
                DoubleAnimation an = new() { From = 5, To = 0, Duration = TimeSpan.FromSeconds(0.25) };
                (MainGrid.Effect as BlurEffect).BeginAnimation(BlurEffect.RadiusProperty, an);
                RegBT.IsEnabled = true;
                LoginBT.IsEnabled = true;
                switch (status)
                {
                    case Data.StatusType.Authorized:
                        MessageBox.Show("Регистрация прошла успешно.");
                        break;
                    case Data.StatusType.Ok:
                        clientSideModule.OnAnswerReceived-= ClientSideModule_OnAnswerReceived;
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
               
            }else
            MessageBox.Show("Произошло что то странное: " + (message).ToString());


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RegBT.IsEnabled = false;
            LoginBT.IsEnabled = false;
            clientSideModule.Login(LoginTB.Text, PasswordTB.Text);
            DoubleAnimation an = new() { From = 0, To = 5, Duration = TimeSpan.FromSeconds(0.25) };
            (MainGrid.Effect as BlurEffect).BeginAnimation(BlurEffect.RadiusProperty, an);
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            RegBT.IsEnabled = false;
            LoginBT.IsEnabled = false;
            clientSideModule.Registration(regLoginTB.Text, regPasswordTB.Text,regNameTB.Text,"");
            DoubleAnimation an = new() { From = 0, To = 5, Duration = TimeSpan.FromSeconds(0.25) };
            (MainGrid.Effect as BlurEffect).BeginAnimation(BlurEffect.RadiusProperty, an);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            new MainWindow(clientSideModule).Show();
            Close();
        }
    }
}
