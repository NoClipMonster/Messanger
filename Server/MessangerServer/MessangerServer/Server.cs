using Protocol;
using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using static MessangerServer.MyConsole;
using static Protocol.Data.Command;

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
        readonly Settings sett = new();
        readonly TcpListener server;
        readonly BackgroundWorker waitForClients;
        readonly BackgroundWorker waitForInput;

        public Protocol.Protocol protocol = new(4);




        static readonly Dictionary<string, TcpClient> onlineClients = new();
        static readonly Dictionary<string, Timer> onlineCheckers = new();
        readonly TimerCallback timerCallback = new(CheckForConnection);
        bool сollectionIsСhanging = false;
        bool сollectionIsReading = false;
        readonly DbClientData clientBase;
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


            clientBase = new();
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
            string[] commands = { "Start", "Stop", "Restart", "Close", "Clients", "Throw exceptions", "Show settings" };

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

                    case "throw exceptions":
                    case "6":
                        sett.ThrowAnException = !sett.ThrowAnException;
                        break;

                    case "show settings":
                    case "7":
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

                                byte[] size = new byte[protocol.bytesOfSizeAmount];
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
                                    byte[] buff = new byte[intSize - ammount];
                                    int kol = client.Value.GetStream().Read(buff, 0, buff.Length);
                                    Array.Copy(buff, 0, myReadBuffer, ammount, kol);
                                    ammount += kol;
                                }
                                var message = protocol.Deserialize(myReadBuffer, out Data.MessageType messageType);

                                if (message != null)
                                {
                                    Message msg;
                                    switch (messageType)
                                    {
                                        case Data.MessageType.Direct:

                                            if (message is not Data.DirectMessage dir)
                                                return;
                                            if (dir.senderLogin != client.Key)
                                                MyConsole.WriteLine("Полученный логин \"" + dir.senderLogin + "\" не соответствует заданному \"" + client.Key + "\"", MsgType.Warning);
                                            msg = new() { Date = dir.sendingTime, SenderID = dir.senderLogin, RecipientID = dir.targetLogin, Id = clientBase.messages.Count(), Text = dir.message, File = dir.dataset.someObject, FileName = dir.dataset.nameOfObject };
                                            clientBase.AddMessage(msg);
                                            if (dir.targetLogin != String.Empty && onlineClients.TryGetValue(dir.targetLogin, out TcpClient? tcpClient))
                                                if (tcpClient != null)
                                                {
                                                    byte[] mess = protocol.SerializeToByte(dir, Data.MessageType.Direct);
                                                    tcpClient.GetStream().Write(mess, 0, mess.Length);
                                                }
                                            break;

                                        case Data.MessageType.Group:
                                            break;

                                        case Data.MessageType.Command:

                                            if (message is not Data.Command command)
                                                return;
                                            switch (command.type)
                                            {
                                                case CommandType.Disconnection:
                                                    WriteLine("Клиент отключен: " + client.Value.Client.RemoteEndPoint + " : " + client.Key, MsgType.Client);
                                                    client.Value.GetStream().Close();
                                                    client.Value.Close();
                                                    onlineClients.Remove(client.Key);
                                                    onlineCheckers[client.Key].Dispose();
                                                    onlineCheckers.Remove(client.Key);
                                                    break;
                                            }
                                            break;
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
                        TcpClient client = server.AcceptTcpClient();
                        WriteLine("Попытка подключение от: " + client.Client.RemoteEndPoint, MsgType.Client);
                        byte[] size = new byte[protocol.bytesOfSizeAmount];
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
                            byte[] buff = new byte[intSize - ammount];
                            int kol = client.GetStream().Read(buff, 0, buff.Length);
                            Array.Copy(buff, 0, myReadBuffer, ammount, kol);

                            ammount += kol;
                        }
                        Data.Command command = protocol.Deserialize(myReadBuffer,out Data.MessageType messageType);
                        /*
                         0 - пользователь существует или пароль не верный
                         1 - успешная регистрация
                         2 - ошибка
                         */
                        if (command.login == String.Empty || command.password == String.Empty)
                        {
                            client.GetStream().Write(new byte[] { 2 }, 0, 1);
                            continue;
                        }

                        if (command.type == CommandType.Registration)
                        {

                            if (clientBase.AddUser(command.login, command.password))
                                client.GetStream().Write(new byte[] { 1 }, 0, 1);
                            else
                            {
                                client.GetStream().Write(new byte[] { 0 }, 0, 1);
                                continue;
                            }

                        }
                        else if (command.type == CommandType.Connection)
                        {
                            User? user = clientBase.users.FirstOrDefault(g => g.Id == command.login);
                            if (user == null)
                            {
                                client.GetStream().Write(new byte[] { 0 }, 0, 1);
                                continue;
                            }
                            if (user.Password == command.password)
                                client.GetStream().Write(new byte[] { 1 }, 0, 1);
                            else
                            {
                                client.GetStream().Write(new byte[] { 0 }, 0, 1);
                                continue;
                            }
                        }


                        WriteLine("Клиент подключен: " + client.Client.RemoteEndPoint + " : " + command.login[0], MsgType.Client);
                        сollectionIsСhanging = true;
                        while (сollectionIsReading) { }

                        try
                        {
                            onlineClients.Add(command.login, client);
                        }
                        catch (Exception ex)
                        {
                            if (sett.ThrowAnException)
                                throw;
                            onlineClients[command.login] = client;
                            WriteLine("onlineClients.Add: " + ex.Message, MsgType.Error);
                        }
                        try
                        {
                            onlineCheckers.Add(command.login, new(timerCallback, command.login, 0, 5 * 1000));
                        }
                        catch (Exception ex)
                        {
                            if (sett.ThrowAnException)
                                throw;
                            onlineCheckers[command.login] = new(timerCallback, command.login, 0, 5 * 1000);
                            WriteLine("onlineCheckers.Add: " + ex.Message, MsgType.Error);
                        }
                        сollectionIsСhanging = false;

                    }
                    else
                        Thread.Sleep(250);
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
                    if (client.Value.Connected)
                    {
                    byte[] msg = protocol.SerializeToByte(new Data.Command(CommandType.Disconnection,client.Key),Data.MessageType.Command);
                        client.Value.GetStream().Write(msg, 0, msg.Length);
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

        /* 
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
        */
        #endregion

        static void CheckForConnection(object? obj)
        {

            if (obj is not string name)
                return;

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

