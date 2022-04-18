using Protocol;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using static MessangerServer.MyConsole;

//TODO: Sessions


namespace MessangerServer
{
    public class Server
    {
        static void Main()
        {
            Console.Title = "Messanger Server";
            Console.WriteLine("Для получения списка комманд введите help или '?'");
            Console.WriteLine("Для активации комманды введите её номер или название");
            MyServer server = new();

            server.Start();
            server.WaitForCommand();
        }
    }
    class MyServer
    {
        readonly Settings sett = new();
        readonly TcpListener server;
        readonly BackgroundWorker WaitForNewData;
        QueryHandler queryHandler;

        public Protocol.Protocol protocol = new(4);

        bool сollectionIsСhanging = false;
        bool сollectionIsReading = false;

        int receiveTimeOut = 5000;//МС


        public MyServer()
        {
            try
            {
                sett.Load();
            }
            catch (Exception ex)
            {
                WriteLine(ex.Message, MsgType.Error);
                WriteLine("Значения настроек будут установленны по умолчанию", MsgType.Warning);
                sett.Save();
            }


            queryHandler = new();
            WriteLine("База 'Логин/пароль' загруженна");

            IPAddress localAddr = IPAddress.Parse(sett.Ip);
            server = new(localAddr, sett.Port);
            WaitForNewData = new();

            WaitForNewData.WorkerSupportsCancellation = true;
            WaitForNewData.WorkerReportsProgress = true;
            WaitForNewData.DoWork += WaitForNewData_DoWork;
            WaitForNewData.RunWorkerCompleted += WaitForNewData_RunWorkerCompleted;
            dt = DateTime.Now;

        }
        int i = 0;
        DateTime dt;


        #region Потоки
        public void WaitForCommand()
        {
            string[] commands = { "Start", "Stop", "Restart", "Close", "Clients", "Throw exceptions", "Show settings", "Show Client Commands" };
            WriteLine("Ожидание комманды");
            while (true)
            {
                string? command = Console.ReadLine();
                if (command == null)
                    continue;

                switch (command.ToLower())
                {
                    default:
                        WriteLine("Такой команды нет", MsgType.Warning);
                        break;

                    case "help":
                    case "?":
                        for (int i = 1; i <= commands.Length; i++)
                            Console.WriteLine(String.Format("  {0}){1}", i, commands[i - 1]));
                        break;

                    case "start":
                    case "1":
                        Start();
                        break;

                    case "stop":
                    case "2":
                        Stop();
                        break;

                    case "restart":
                    case "3":
                        Stop();
                        Start();
                        break;

                    case "close":
                    case "4":
                        Stop();
                        Environment.Exit(0);
                        break;
                    case "clients":
                    case "5":
                        //TODO: Добавить вывод списка клиентов
                        WriteLine("Тут пусто, нужно добавить функционал", MsgType.Error);

                        break;

                    case "throw exceptions":
                    case "6":
                        sett.ThrowAnException = !sett.ThrowAnException;
                        break;

                    case "show settings":
                    case "7":
                        sett.Show();
                        break;
                    case "showclientcommands":
                    case "8":
                        sett.ShowClientCommands = !sett.ShowClientCommands;
                        break;

                }
            }
        }

        void WaitForNewData_DoWork(object? sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (WaitForNewData.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                try
                {
                    BackgroundWorker worker = new();
                    worker.DoWork += Receive_Data_DoWork;
                    worker.RunWorkerAsync(server.AcceptTcpClient());
                }
                catch (Exception ex)
                {
                    if (sett.ThrowAnException)
                    {
                        throw ex;
                    }
                    else MyConsole.WriteLine(ex.Message, MsgType.Error);
                    e.Cancel = true;
                }

            }
        }
        private void WaitForNewData_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                return;
            }

            WaitForNewData.RunWorkerAsync();
        }
        void Receive_Data_DoWork(object? sender, DoWorkEventArgs e)
        {


            if (e.Argument is not TcpClient client)
                return;

            //  WriteLine("Запрос от: " + client.Client.RemoteEndPoint, MsgType.Client);

            byte[] size = new byte[protocol.bytesOfSizeAmount];

            client.GetStream().Read(size, 0, size.Length);
            int intSize = 0;

            for (int i = size.Length - 1; i >= 0; i--)
            {
                intSize += size[i] * (int)Math.Pow(256, size.Length - (i + 1));
            }

            byte[] receivedBuffer = new byte[intSize];
            int ReceivedBytes = 0;
            DateTime lastReceiveTime = DateTime.Now;
            while (ReceivedBytes < intSize)
            {
                byte[] buff = new byte[intSize - ReceivedBytes];
                int kol = client.GetStream().Read(buff, 0, buff.Length);
                Array.Copy(buff, 0, receivedBuffer, ReceivedBytes, kol);
                ReceivedBytes += kol;
                if (kol != 0)
                    lastReceiveTime = DateTime.Now;

                if ((DateTime.Now - lastReceiveTime).TotalMilliseconds > receiveTimeOut)
                {
                    client.GetStream().Write(protocol.SerializeToByte(Data.StatusType.TimeOut, Data.MessageType.Status));
                    return;
                }
            }
            dynamic Query = protocol.Deserialize(receivedBuffer);

            if (Query is not Data.Command.GetMessages && sett.ShowClientCommands)
                WriteLine(Query.ToString().Substring(14), MsgType.Client);
            client.GetStream().Write(queryHandler.Answer(Query));
            (sender as BackgroundWorker).Dispose();
        }

        #endregion

        #region Управление сервером
        public void Stop()
        {
            if (server.Server.LocalEndPoint == null)
            {
                WriteLine("Сервер ещё не запущен", MsgType.Error);
                return;
            }
            WaitForNewData.CancelAsync();

            while (WaitForNewData.IsBusy)
            {
                Console.Write(".");
                Thread.Sleep(500);
                WaitForNewData.CancelAsync();
            }

            server.Stop();
            Console.WriteLine();
            WriteLine("Сервер остановлен");
        }

        public void Start()
        {
            if (server.Server.LocalEndPoint != null)
            {
                WriteLine("Сервер уже запущен", MsgType.Error);
                return;
            }
            server.Start();
            WaitForNewData.RunWorkerAsync();

            WriteLine("Сервер запущен");

        }

        #endregion

    }
}

