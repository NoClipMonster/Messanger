using Newtonsoft.Json;
using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using static MessangerServer.MyConsole;

namespace MessangerServer
{
    public class Server
    {
        //TODO: Попробовать дать каждому пользователь по потоку
        //TODO: Сохранение сообщений
        //TODO: Ошибка уже существующего ключа при подключении уже подключенного пользолвателя
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
        static readonly string loginsDir = @"base.json";

        readonly Settings sett = new();
        readonly TcpListener server;
        readonly BackgroundWorker waitForClients;
        readonly BackgroundWorker waitForInput;
        readonly Dictionary<string, string> loginInfo;
        static readonly Dictionary<string, TcpClient> onlineClients = new();
        static readonly Dictionary<string, Timer> onlineCheckers = new();
        readonly TimerCallback timerCallback = new(CheckForConnection);
        bool сollectionIsСhanging = false;
        bool сollectionIsReading = false;
        public MyServer()
        {
            try
            {
                sett.Load();
            }
            catch (Exception ex)
            {
                WriteLine(ex.Message, MsgType.Error);
                WriteLine("Значения будут установленны по умолчанию", MsgType.Warning);
                sett.Save();
            }
            //TODO: TRY тут
            loginInfo = new Dictionary<string, string>();

            if (File.Exists(loginsDir))
            {
                string[][]? bb = JsonConvert.DeserializeObject<string[][]?>(File.ReadAllText(loginsDir));
                if (bb != null)
                    for (int i = 0; i < bb.Length; i++)
                        loginInfo.Add(bb[i][0], bb[i][1]);
            }
            WriteLine("База 'Логин/пароль' загруженна");

            IPAddress localAddr = IPAddress.Parse(sett.Ip);
            server = new(localAddr, sett.Port);
            waitForClients = new();
            waitForInput = new();

            waitForClients.WorkerSupportsCancellation = true;
            waitForInput.WorkerSupportsCancellation = true;

            waitForClients.DoWork += WaitForClients_DoWork;
            waitForInput.DoWork += WaitForInput_DoWork;

        }


        #region Потоки
        public void WaitForCommand()
        {
            string[] commands = { "Start", "Stop", "Restart","Close", "Clients", "Backup", "Throw exceptions", "Show settings" };

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
                        if (onlineClients.Count == 0)
                            WriteLine("Пусто", MsgType.Warning);
                        while (сollectionIsСhanging) { }
                        сollectionIsReading = true;
                        foreach (KeyValuePair<string, TcpClient> client in onlineClients)
                            WriteLine(String.Format("Клиент: {0}: IP: {1}", client.Key, client.Value.Client.RemoteEndPoint), MsgType.Client);
                        сollectionIsReading = false;
                        break;

                    case "backup":
                    case "6":
                        Backup(loginInfo);
                        break;

                    case "throw exceptions":
                    case "7":
                        sett.ThrowAnException = !sett.ThrowAnException;
                        break;

                    case "show settings":
                    case "8":
                        sett.Show();
                        break;

                }
            }
        }

        void WaitForInput_DoWork(object? sender, DoWorkEventArgs e)
        {
            while (!waitForInput.CancellationPending)
            {
                while (сollectionIsСhanging)
                {

                }
                сollectionIsReading = true;
                foreach (KeyValuePair<string, TcpClient> client in onlineClients)
                {
                    if (client.Value == null)
                        continue;
                    if (waitForInput.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    if (client.Value.Connected)
                        if (client.Value.GetStream().DataAvailable)
                            try
                            {
                                WriteLine("Получение данных от клиента", MsgType.Client);
                                //TODO: Заменить везде статичный максимальный размер на переменную
                                //TODO: Кластерная передача если больше максимального размера

                                byte[] size = new byte[3];
                                client.Value.GetStream().Read(size, 0, size.Length);

                                int intSize = 0;
                                for (int i = size.Length - 1; i >= 0; i--)
                                {
                                    intSize += size[i] * (int)Math.Pow(256, size.Length - (i + 1));
                                }   

                                byte[] myReadBuffer = new byte[intSize];
                                int ammount = 0;
                               
                                while (ammount < intSize)
                                {
                                    byte[] buff = new byte[intSize];
                                    int kol = client.Value.GetStream().Read(buff, 0, buff.Length);
                                    Array.Copy(buff,0,myReadBuffer,ammount,kol);
                                    ammount += kol;
                                }
                                Protocol protocol = new(myReadBuffer);

                                if (protocol.data != null)
                                {
                                    protocol.data.senderLogin = client.Key;
                                    if (protocol.data.sizeOfObject != null)
                                    {
                                        byte[] obj = new byte[(int)protocol.data.sizeOfObject];
                                        ammount = 0;
                                        DateTime dt = DateTime.Now;
                                        while (ammount < (int)protocol.data.sizeOfObject)
                                        {
                                            byte[] buff = new byte[(int)protocol.data.sizeOfObject];
                                            int kol = client.Value.GetStream().Read(buff, 0, buff.Length);
                                            Array.Copy(buff, 0, obj, ammount, kol);
                                            ammount += kol;                                          
                                        }
                                       WriteLine(String.Format("\n  Размер файла: {0} Kb" +
                                            "\n  Время потраченное на получение: {1} s" +
                                            "\n  Скорость передачи: {2} Kb/s",Math.Round((decimal)protocol.data.sizeOfObject/1024,3)
                                            ,(DateTime.Now-dt).TotalSeconds,
                                            Math.Round(((decimal)protocol.data.sizeOfObject / 1024)/(decimal)(DateTime.Now - dt).TotalSeconds,3)
                                            ), MsgType.Client);
                                        protocol.data.SomeObject = obj;
                                    }
                                    if (protocol.data.closeConnection)
                                    {
                                        WriteLine("Клиент отключен: " + client.Value.Client.RemoteEndPoint + " : " + client.Key, MsgType.Client);
                                        client.Value.GetStream().Close();
                                        client.Value.Close();
                                        onlineClients.Remove(client.Key);
                                        onlineCheckers[client.Key].Dispose();
                                        onlineCheckers.Remove(client.Key);
                                    }
                                    else
                                    {
                                        if (protocol.data.targetLogin != null && onlineClients.TryGetValue(protocol.data.targetLogin, out TcpClient? tcpClient))
                                        {
                                            if (tcpClient != null)
                                                tcpClient.GetStream().Write(protocol.SerializeToByte,0,protocol.SerializeToByte.Length);
                                        }
                                    }

                                }
                           
                            }
                            catch (Exception ex)
                            {
                                if (sett.ThrowAnException)
                                    throw;
                                WriteLine("WaitForInput: " + ex.Message, MsgType.Error);
                            }

                }
                сollectionIsReading = false;
            }
            e.Cancel = true;
        }

        void WaitForClients_DoWork(object? sender, DoWorkEventArgs e)
        {
            while (!waitForClients.CancellationPending)
            {
                try
                {

                    if (server.Pending())
                    {
                        TcpClient? client = server.AcceptTcpClient();

                        byte[] size = new byte[3];
                        client.GetStream().Read(size, 0, size.Length);

                        int intSize = 0;
                        for (int i = size.Length - 1; i >= 0; i--)
                        {
                            intSize += size[i] * (int)Math.Pow(256, size.Length - (i + 1));
                        }

                        byte[] myReadBuffer = new byte[intSize];
                        int ammount = 0;
                        while (ammount < intSize)
                        {
                            byte[] buff = new byte[intSize];
                            int kol = client.GetStream().Read(buff, 0, buff.Length);
                            Array.Copy(buff, 0, myReadBuffer, ammount, kol);
                            
                            ammount += kol;
                        }

                        string[]? logPas = JsonConvert.DeserializeObject<string[]>(Encoding.UTF8.GetString(myReadBuffer));
                        if (logPas == null)
                        {
                            client.GetStream().Write(new byte[] { 3 }, 0, 1);
                            continue;
                        }

                        if (loginInfo.TryGetValue(logPas[0], out string? basePassword) && basePassword != null)
                        {
                            if (basePassword == logPas[1])
                            {
                                client.GetStream().Write(new byte[] { 1 }, 0, 1);
                            }
                            else
                            {
                                client.GetStream().Write(new byte[] { 0 }, 0, 1);
                                continue;
                            }
                        }
                        else
                        {
                            client.GetStream().Write(new byte[] { 2 }, 0, 1);
                            loginInfo.Add(logPas[0], logPas[1]);
                            Backup(loginInfo);
                        }
                        WriteLine("Клиент подключен: " + client.Client.RemoteEndPoint + " : " + logPas[0], MsgType.Client);
                        сollectionIsСhanging = true;
                        while (сollectionIsReading) { }

                        try
                        {
                            onlineClients.Add(logPas[0], client);
                        }
                        catch (Exception ex)
                        {
                            if (sett.ThrowAnException)
                                throw;
                            onlineClients[logPas[0]] = client;
                            WriteLine("onlineClients.Add: " + ex.Message, MsgType.Error);
                        }

                        try
                        {
                            onlineCheckers.Add(logPas[0], new(timerCallback, logPas[0], 0, 5 * 1000));
                        }
                        catch (Exception ex)
                        {
                            if (sett.ThrowAnException)
                                throw;
                            onlineClients[logPas[0]] = client;
                            WriteLine("onlineCheckers.Add: " + ex.Message, MsgType.Error);
                        }
                        сollectionIsСhanging = false;

                    }
                    else
                        continue;
                }
                catch (Exception ex)
                {
                    if (sett.ThrowAnException)
                        throw;
                    WriteLine("waitForClients: " + ex.Message, MsgType.Error);
                }
            }
            e.Cancel = true;
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
            waitForInput.CancelAsync();
            waitForClients.CancelAsync();
            foreach (KeyValuePair<string, TcpClient> client in onlineClients)
            {

                try
                {
                    Protocol protocol = new(new Protocol.Data { closeConnection = true, senderLogin = "server", targetLogin = client.Key });
                    if (client.Value.Connected)
                    {
                        client.Value.GetStream().Write(protocol.SerializeToByte,0,protocol.SerializeToByte.Length);
                        client.Value.GetStream().Close();
                        client.Value.Close();
                    }
                   
                }
                catch (Exception ex)
                {
                    if (sett.ThrowAnException)
                        throw;
                    WriteLine("Client: " + client.Value.Client.RemoteEndPoint + "\nError: " + ex.Message.ToString(), MsgType.Error);
                }
            }

            onlineClients.Clear();

            foreach (KeyValuePair<string, Timer> checker in onlineCheckers)
            {
                checker.Value.Dispose();
            }
            onlineCheckers.Clear();

            server.Stop();

            while (waitForClients.IsBusy || waitForInput.IsBusy)
            {
                Console.Write(".");
                Thread.Sleep(500);
            }
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
            waitForClients.RunWorkerAsync();
            waitForInput.RunWorkerAsync();
            WriteLine("Сервер запущен");

        }

        static void Backup(object? obj)
        {
            if (obj != null)
            {
                WriteLine("Резервирование данных началось");

                Dictionary<string, string> dict = (Dictionary<string, string>)obj;
                string[][] bb = new string[dict.Count][];
                for (int i = 0; i < bb.Length; i++)
                    bb[i] = new string[] { dict.Keys.ElementAt(i), dict.Values.ElementAt(i) };

                Task task = File.WriteAllTextAsync(loginsDir, JsonConvert.SerializeObject(bb));
                while (task.Status == TaskStatus.Running)
                {
                    Console.Write(".");
                }
                WriteLine("Резервирование данных выполненно");
            }
        }

        #endregion

        static void CheckForConnection(object? obj)
        {

            if (obj is not string name)
                return;
            //TODO: The given key 'admin' was not present in the dictionary.'

            TcpClient client = onlineClients[name];
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections().Where(x => x.LocalEndPoint.Equals(client.Client.LocalEndPoint) && x.RemoteEndPoint.Equals(client.Client.RemoteEndPoint)).ToArray();

            if (tcpConnections != null && tcpConnections.Length > 0)
            {
                TcpState stateOfConnection = tcpConnections.First().State;
                if (stateOfConnection != TcpState.Established)
                {
                    WriteLine("Обнаружен разрыв c " + client.Client.RemoteEndPoint + " : " + name, MsgType.Warning);
                    onlineClients.Remove(name);
                    onlineCheckers[name].Dispose();
                    onlineCheckers.Remove(name);
                }
            }
        }




    }
}

