using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MessangerServer
{
    public class Server
    {
        //TODO: Попробовать дать каждому пользователь по потоку
        static void Main()
        {
            MyServer server = new("192.168.1.101", 8080);
            server.Start();
            Console.WriteLine("Введите Help для получения списка комманд");
        }

        class MyServer
        {
            readonly TcpListener server;
            readonly List<Client> Clients;
            readonly BackgroundWorker waitForClients;
            readonly BackgroundWorker waitForInput;
            readonly BackgroundWorker checkForDisconection;

            public MyServer(string IP, int port)
            {
                IPAddress localAddr = IPAddress.Parse(IP);
                server = new(localAddr, port);
                Clients = new();

                waitForClients = new();
                waitForInput = new();
                checkForDisconection = new();

                waitForClients.WorkerSupportsCancellation = true;
                waitForInput.WorkerSupportsCancellation = true;
                checkForDisconection.WorkerSupportsCancellation = true;

                waitForClients.DoWork += WaitForClients_DoWork;
                waitForInput.DoWork += WaitForInput_DoWork;
                checkForDisconection.DoWork += CheckForDisconection_DoWork;

            }



            void WaitForCommand()
            {
                while (true)
                {
                    string? command = Console.ReadLine();
                    if (command == null)
                        continue;

                    switch (command.ToLower())
                    {
                        default:
                            Console.WriteLine("[SERVER] " + "Такой команды нет");
                            break;
                        case "help":
                            Console.WriteLine("[SERVER]\n" +
                                "   Start\n" +
                                "   Stop\n" +
                                "   Restart\n" +
                                "   Clients");
                            break;
                        case "stop":
                            Stop();
                            break;
                        case "clients":
                            if (Clients.Count == 0)
                                Console.WriteLine("[SERVER] " + "Пусто");
                            foreach (Client client in Clients)
                                Console.WriteLine(String.Format("Клиент: {0}: IP: {1}", client.clientName, client.tcpClient.Client.RemoteEndPoint));
                            break;
                        case "start":
                            Start();
                            break;
                        case "restart":
                            Stop();
                            Start();
                            break;

                    }
                }
            }

            private void CheckForDisconection_DoWork(object? sender, DoWorkEventArgs e)
            {
                while (true)
                {
                    for (int i = 0; i < Clients.Count; i++)
                    {
                        if (waitForInput.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }
                        //if (Clients[i].stream)
                    }
                }
            }

            private void WaitForInput_DoWork(object? sender, DoWorkEventArgs e)
            {
                while (!waitForInput.CancellationPending)
                {
                    for (int i = 0; i < Clients.Count; i++)
                    {
                        if (waitForInput.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }
                        if (Clients[i] != null && Clients[i].stream != null)
                            if (Clients[i].stream.DataAvailable)
                            {
                                try
                                {
                                    Console.WriteLine("[SERVER] " + "Получение данных");

                                    byte[] am = new byte[4];

                                    Clients[i].stream.Read(am, 0, 4);

                                    byte[] myReadBuffer = new byte[int.Parse(Encoding.UTF8.GetString(am, 0, 4))];
                                    Clients[i].stream.Read(myReadBuffer, 0, myReadBuffer.Length);

                                    Protocol protocol = new(myReadBuffer);

                                    if (protocol.data != null)
                                    {
                                        if (protocol.data.closeConnection)
                                        {
                                            Console.WriteLine("[SERVER] " + "Клиент отключен: " + Clients[i].tcpClient.Client.RemoteEndPoint + " : " + Clients[i].clientName);
                                            Clients[i].stream.Close();
                                            Clients[i].tcpClient.Close();
                                            Clients.RemoveAt(i);
                                            if (i != 0)
                                                i--;
                                        }
                                        else
                                        {
                                            Client? client = Clients.Find(x => x.clientName == protocol.data.targetLogin);
                                            if (client != null)
                                                client.stream.Write(protocol.SerializeToByte);
                                        }

                                    }

                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("[ERROR] " + "WaitForInput: " + ex.Message);
                                }
                            }
                    }

                }
                e.Cancel = true;

            }

            private void WaitForClients_DoWork(object? sender, DoWorkEventArgs e)
            {
                while (!waitForClients.CancellationPending)
                {
                    TcpClient? client = null;
                    try
                    {
                        if (server.Pending())
                            client = server.AcceptTcpClient();
                        else
                            continue;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("[ERROR] " + "WaitForClients: " + ex.Message);
                    }

                    if (client != null)
                    {
                        Console.Write("[SERVER] " + "Клиент подключен: " + client.Client.RemoteEndPoint);
                        byte[] login = new byte[16];

                        int kount = client.GetStream().Read(login, 0, login.Length);
                        Clients.Add(new Client(client, Encoding.UTF8.GetString(login, 0, kount)));
                        Console.WriteLine(" : " + Clients[^1].clientName);

                    }
                }
                e.Cancel = true;
            }

            #region Управление сервером
            public void Stop()
            {
                waitForInput.CancelAsync();
                waitForClients.CancelAsync();
                foreach (Client client in Clients)
                {
                    try
                    {
                        Protocol protocol = new(new Protocol.Data { closeConnection = true, senderLogin = "server", targetLogin = client.clientName });
                        client.stream.Write(protocol.SerializeToByte);
                        client.stream.Close();
                        client.tcpClient.Close();

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("[ERROR] " + "Client: " + client.tcpClient.Client.RemoteEndPoint + "\nError: " + ex.Message.ToString());
                    }
                }

                Clients.Clear();
                server.Stop();
                while (waitForClients.IsBusy || waitForInput.IsBusy)
                {
                    Console.WriteLine(".");
                    Thread.Sleep(500);
                }
                Console.WriteLine("[SERVER] " + "Сервер остановлен");
            }

            public void Start()
            {
                server.Start();
                Console.WriteLine("[SERVER] " + "Сервер запущен");
                waitForClients.RunWorkerAsync();
                waitForInput.RunWorkerAsync();
                WaitForCommand();
            }
            #endregion
        }

    }
    class Client
    {
        public Client(TcpClient tcpClient, string clientName)
        {
            stream = tcpClient.GetStream();
            this.clientName = clientName;
            this.tcpClient = tcpClient;
        }
        public NetworkStream stream;
        public string clientName;
        public TcpClient tcpClient;
    }
}

