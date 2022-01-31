using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace ClientApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    //TODO: Передача файлов клестерным методом
    //TODO: Исправить проблему зависание при ошибке сервера во время подключения
    //TODO: Ошибка при закрытии программы после не удачного подключения к серверу
    //TODO: Ошибка при отключения от выключенного сервера   
    //TODO: Проверка но разрыв соединения с сервером
    public partial class MainWindow : Window
    {
        readonly Client client;

        bool connected = false;
        public MainWindow()
        {

            InitializeComponent();
            client = new Client();
            client.waitForMessages.RunWorkerCompleted += WaitForMessages_RunWorkerCompleted;
            client.asynkConnecter.RunWorkerCompleted += BKW_RunWorkerCompleted;
        }

        private void WaitForMessages_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                client.waitForMessages.RunWorkerAsync();

                if (e.Result is Protocol protocol && protocol.data != null)
                {

                    if (protocol.data.closeConnection)
                    {
                        client.waitForMessages.CancelAsync();
                        ConnectionChanged(false);
                        ConnectedIndicator.Fill = new SolidColorBrush(Colors.AliceBlue);
                    }
                    else
                    {
                        InsertMessage(protocol, false);
                    }
                    //protocol.Dispose();
                }
            }
        }

        private void BKW_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is bool boolean)
                ConnectionChanged(boolean);

        }

        private void InsertMessage(Protocol protocol, bool user)
        {
            if (protocol.data is null)
                return;

            TextAlignment textAlignment = user ? TextAlignment.Right : TextAlignment.Left;
            string time = protocol.data.sendingTime.ToShortTimeString();
            string text = user ? protocol.data.message + " :" + time : time + ": " + protocol.data.senderLogin + ": " + protocol.data.message;

            if (DialogWindow.Document.Blocks.LastBlock != null && DialogWindow.Document.Blocks.LastBlock.TextAlignment == textAlignment)
                ((Paragraph)DialogWindow.Document.Blocks.LastBlock).Inlines.Add(text.Insert(0, "\n"));
            else
                DialogWindow.Document.Blocks.Add(new Paragraph(new Run(text)) { TextAlignment = textAlignment });
            if (!user && protocol.data.SomeObject != null && protocol.data.nameOfObject != null)
            {
                try
                {
                    if (!Directory.Exists("Files"))
                        Directory.CreateDirectory("Files");
                    File.WriteAllBytes("Files/"+protocol.data.nameOfObject, protocol.data.SomeObject);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Не удалось сохранить файл. Ошибка:" + ex.Message);
                    return;
                }
            }
            DialogWindow.ScrollToEnd();
            //protocol.Dispose();
        }

        private void SendBT_Click(object sender, RoutedEventArgs e)
        {
            if (TargetTB.Text == "" || MessageTB.Text == "")
            {
                MessageBox.Show("Поля не должны быть пустыми.");
                return;
            }
            Protocol protocol = new(new Protocol.Data()
            {
                senderLogin = LoginTB.Text,
                sendingTime = DateTime.Now,
                targetLogin = TargetTB.Text,
                message = MessageTB.Text,
                SomeObject = PathBox.Text != "" ? File.ReadAllBytes(PathBox.Text) : null,
                nameOfObject = PathBox.Text != "" ? new FileInfo(PathBox.Text).Name : null
            });
            InsertMessage(protocol, true);
            client.Exchange(protocol.SerializeToByte);
            //protocol.Dispose();
        }

        private void StartClient_Click(object sender, RoutedEventArgs e)
        {
            if (connected)
            {
                client.CloseConnection();
                ConnectionChanged(false);
                ConnectedIndicator.Fill = new SolidColorBrush(Colors.AliceBlue);
            }
            else
            {
                if (LoginTB.Text == "" || PasswordTB.Text == "")
                {
                    MessageBox.Show("Поля не должны быть пустыми.");
                    return;
                }
                client.asynkConnecter.RunWorkerAsync(new string[] { LoginTB.Text, PasswordTB.Text });
                StartClientBT.Content = "Stop connection";
                ConnectedIndicator.Fill = new SolidColorBrush(Colors.Yellow);
                StartClientBT.IsEnabled = false;
                LoginTB.IsEnabled = false;
                PasswordTB.IsEnabled = false;
            }
        }

        void ConnectionChanged(bool status)
        {
            Color color = status ? Colors.Green : Colors.Red;
            ConnectedIndicator.Fill = new SolidColorBrush(color);
            connected = status;
            if (status)
            {
                client.waitForMessages.RunWorkerAsync();
                messageGrid.Visibility = Visibility.Visible;
                loginGrid.Visibility = Visibility.Hidden;
                StartClientBT.Content = "Close connection";
            }
            else
            {

                StartClientBT.Content = "Start client";
                messageGrid.Visibility = Visibility.Hidden;
                loginGrid.Visibility = Visibility.Visible;
            }
            StartClientBT.IsEnabled = true;
            LoginTB.IsEnabled = true;
            PasswordTB.IsEnabled = true;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //TODO: Ошибка потока при закрытии программы с подключенным клиентом и выключенным сервером
            client.CloseConnection();
        }

        private void PathBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //TODO: Отправка нескольких файлов
            //TODO: Отмена отправки
            OpenFileDialog openFileDialog = new();
            openFileDialog.ShowDialog();

            if (openFileDialog.FileName != "")
            {
                PathBox.Text = openFileDialog.FileName;
            }
            
        }

    }
    class Client
    {
        public BackgroundWorker asynkConnecter;
        public BackgroundWorker waitForMessages;
        NetworkStream? stream;
        TcpClient client;
        string login = "";
        public Client()
        {
            waitForMessages = new();
            waitForMessages.WorkerSupportsCancellation = true;
            waitForMessages.DoWork += Check;

            asynkConnecter = new();
            asynkConnecter.WorkerSupportsCancellation = true;
            asynkConnecter.DoWork += AsynkConnecter_DoWork;

            client = new();
        }
        public void CloseConnection()
        {
            waitForMessages.CancelAsync();
            Protocol protocol = new(new Protocol.Data()
            {
                senderLogin = login,
                targetLogin = "server",
                closeConnection = true
            });
            Exchange(protocol.SerializeToByte);
            client.Close();
            //protocol.Dispose();
        }

        private void AsynkConnecter_DoWork(object? sender, DoWorkEventArgs e)
        {
            if (e.Argument != null)
            {
                try
                {
                    if (e.Argument != null) e.Result = Connect(e.Argument);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    e.Result = false;
                    e.Cancel = true;
                }
            }
        }

        public bool Connect(object obj)
        {
            try
            {
                if (obj == null)
                    return false;
                string[] logPas = (string[])obj;
                client = new("91.224.137.146", 8080);
                stream = client.GetStream();

                byte[] byteLogPas = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj));
                byte[] size = new byte[3];
                login = logPas[0];
                for (int i = size.Length - 1; i >= 0; i--)
                {
                    size[i] = (byte)((byteLogPas.Length % Math.Pow(256, size.Length - i)) / Math.Pow(256, size.Length - (i + 1)));
                }
                stream.Write(size, 0, size.Length);
                stream.Write(byteLogPas, 0, byteLogPas.Length);

                byte[] answer = new byte[1];
                stream.Read(answer, 0, 1);
                switch (answer[0])
                {
                    case 0:
                        MessageBox.Show("Пароль не верный");
                        return false;

                    case 1:
                        return true;

                    case 2:
                        MessageBox.Show("Регистрация прошла успешно");
                        return true;
                    case 3:
                        MessageBox.Show("Ошибка передачи данных");
                        return false;

                    default:
                        MessageBox.Show("Какая-то ошибка");
                        return false;

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Connect: " + ex.Message);

                return false;
            }
        }

        public void Exchange(byte[] message)
        {
            try
            {
                if (stream != null)
                {
                    stream.Write(message, 0, message.Length);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exchange: " + ex.Message);
            }
        }

        public void Check(object? sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (waitForMessages.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                if (stream != null && stream.DataAvailable)
                {
                    byte[] size = new byte[3];
                    client.GetStream().Read(size, 0, size.Length);



                    int intSize = 0;
                    for (int i = size.Length - 1; i >= 0; i--)
                    {
                        intSize += size[i] * (int)Math.Pow(256, size.Length - (i + 1));
                    }

                    byte[] myReadBuffer = new byte[intSize];
                    int ammount = 0; 
                    while (ammount<intSize)
                    {
                        byte[] buff = new byte[intSize];
                        int kol = stream.Read(buff, 0, buff.Length);
                        Array.Copy(buff, 0, myReadBuffer, ammount, kol);
                        ammount += kol;
                    }
                    
                    Protocol protocol = new(myReadBuffer);
                    if (protocol.data != null && protocol.data.sizeOfObject != null)
                    {
                        protocol.data.SomeObject = new byte[(int)protocol.data.sizeOfObject];
                        ammount = 0;
                        while (ammount < (int)protocol.data.sizeOfObject)
                        {
                            byte[] buff = new byte[(int)protocol.data.sizeOfObject];
                            int kol = stream.Read(buff, 0, (int)protocol.data.sizeOfObject);
                            Array.Copy(buff, 0, protocol.data.SomeObject, ammount, kol);
                           
                            ammount += kol;
                        }
                       
                    }
                    e.Result = protocol;
                  
                    return;
                }


            }

        }
    }
}
