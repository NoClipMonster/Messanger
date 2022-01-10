using System;
using System.ComponentModel;
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

        }

        private void SendBT_Click(object sender, RoutedEventArgs e)
        {
            Protocol protocol = new(new Protocol.Data()
            {
                sendingTime = DateTime.Now,
                senderLogin = SenderTB.Text,
                targetLogin = TargetTB.Text,
                message = MessageTB.Text
            });
            InsertMessage(protocol, true);

            client.Exchange(protocol.SerializeToByte);
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
                client.asynkConnecter.RunWorkerAsync(SenderTB.Text);
                StartClientBT.Content = "Stop connection";
                ConnectedIndicator.Fill = new SolidColorBrush(Colors.Yellow);
                StartClientBT.IsEnabled = false;
            }
        }

        void ConnectionChanged(bool status)
        {
            Color color = status ? Colors.Green : Colors.Red;
            ConnectedIndicator.Fill = new SolidColorBrush(color);
            connected = status;
            if (status)
            {
                client.waitForMessages.RunWorkerAsync(SenderTB.Text);
                messageGrid.Visibility = Visibility.Visible;
                StartClientBT.Content = "Close connection";
                SenderTB.IsEnabled = false;
            }
            else
            {
                StartClientBT.Content = "Start client";
                messageGrid.Visibility = Visibility.Hidden;
                SenderTB.IsEnabled = true;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //TODO: Ошибка потока при закрытии программы с подключенным клиентом и выключенным сервером
            client.CloseConnection();
        }
    }
    class Client
    {
        public BackgroundWorker asynkConnecter;
        public BackgroundWorker waitForMessages;
        NetworkStream? stream;
        TcpClient? client;
        public string? clientName;
        public Client()
        {
            waitForMessages = new();
            waitForMessages.WorkerSupportsCancellation = true;
            waitForMessages.DoWork += Check;

            asynkConnecter = new();
            asynkConnecter.WorkerSupportsCancellation = true;
            asynkConnecter.DoWork += AsynkConnecter_DoWork;

        }
        public void CloseConnection()
        {
            waitForMessages.CancelAsync();
            Protocol protocol = new(new Protocol.Data()
            {
                senderLogin = clientName,
                targetLogin = "server",
                closeConnection = true
            });
            Exchange(protocol.SerializeToByte);
        }

        private void AsynkConnecter_DoWork(object? sender, DoWorkEventArgs e)
        {
            if (e.Argument != null)
            {
                string? str = e.Argument.ToString();
                if (str != null) e.Result = Connect(str);
            }
        }

        public bool Connect(string myName)
        {
            try
            {
                client = new("91.224.137.146", 8080);
                stream = client.GetStream();
                clientName = myName;
                stream.Write(Encoding.UTF8.GetBytes(clientName, 0, clientName.Length));
                return true;
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
                    stream.Write(message);
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
                try
                {
                    if (stream != null && stream.DataAvailable)
                    {
                        byte[] am = new byte[4];

                        stream.Read(am, 0, 4);

                        byte[] myReadBuffer = new byte[int.Parse(Encoding.UTF8.GetString(am, 0, 4))];
                        stream.Read(myReadBuffer, 0, myReadBuffer.Length);

                        Protocol protocol = new(myReadBuffer);

                        e.Result = protocol;
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Check: " + ex.Message);
                }
            }

        }
    }
}
