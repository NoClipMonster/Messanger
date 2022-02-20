using Newtonsoft.Json;
using System;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace MessangerApp2._0
{
    /// <summary>
    /// Interaction logic for RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        string IP = "91.224.137.146";
        int PORT = 8080;
        MainWindow? mainWindow;
        TcpClient client = new();
        public RegistrationWindow()
        {
            InitializeComponent();
        }

        static byte[] ByteConstructor(string[] someString,out byte[] Length)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(someString));
            Length = new byte[4];
            for (int i = Length.Length - 1; i >= 0; i--)
            {
                Length[i] = (byte)((bytes.Length % Math.Pow(256, Length.Length - i)) / Math.Pow(256, Length.Length - (i + 1)));
            }
            
            return bytes;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            switch (Connect(LoginTB.Text, PasswordTB.Text))
            {
                case 0:
                    MessageBox.Show("Пароль не верный или пользователь не существует");
                    break;
                case 1:
                    MessageBox.Show("Пароль верный");
                    mainWindow = new MainWindow(client);
                    mainWindow.Show();
                    Close();
                    break;

                default:
                    MessageBox.Show("Какая-то ошибка");
                    break;
            }
        }
        int Connect(string login, string password)
        {
            //Todo Таймаут
            client = new TcpClient(IP, PORT);
            byte[] bytes = ByteConstructor(new string[] { login, password }, out byte[] Length);

            client.GetStream().WriteByte(0);

            client.GetStream().Write(Length, 0, Length.Length);
            client.GetStream().Write(bytes, 0, bytes.Length);
            return client.GetStream().ReadByte();
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(regRepPasswordTB.Text!=regPasswordTB.Text){
                MessageBox.Show("Пароли не совпадают");
                return;
            }

            switch (Connect(regLoginTB.Text, regPasswordTB.Text))
            {
                case 0:
                    MessageBox.Show("Такой пользователь уже существует");
                    break;
                case 1:
                    MessageBox.Show("Аккаунт создан");
                    mainWindow = new MainWindow(client);
                    mainWindow.Show();
                    client.Dispose();
                    Close();
                    break;

                default:
                    MessageBox.Show("Какая-то ошибка");
                    break;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            mainWindow = new MainWindow(client);
            mainWindow.Show();
            Close();
        }
    }
}
