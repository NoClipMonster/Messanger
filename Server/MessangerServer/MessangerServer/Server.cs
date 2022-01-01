using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MessangerServer
{
    public class Server
    {
        static void Main()
        {
            myServer server = new myServer("192.168.1.101", 8080);
            server.Start();
            List<TcpClient> clients = new List<TcpClient>();
        }

        class myServer
        {
            TcpListener server;
            List<string> logins;
            List<TcpClient> clients;
            List<NetworkStream> streams;
            public myServer(string IP, int port)
            {
                IPAddress localAddr = IPAddress.Parse(IP);
                server = new TcpListener(localAddr, port);
                clients = new List<TcpClient>();
                streams = new List<NetworkStream>();
                logins = new List<string>();
                
            }
            public void Start()
            {
                server.Start();
                Console.WriteLine("[SERVER] " + "Сервер запущен");
                Thread waitForClients = new Thread(WaitForClients);
                Thread inputStream = new Thread(InputStream);
                waitForClients.Start();
                inputStream.Start();
            }
            void WaitForClients()
            {
                
                do
                {
                    TcpClient client;
                    client = server.AcceptTcpClient();
                    if (client != null)
                    {
                        Console.WriteLine("[SERVER] " + "Клиент подключен: "+client.Client.RemoteEndPoint);
                        byte[] login = new byte[16];
                       
                        int kount = client.GetStream().Read(login, 0, login.Length);
                        logins.Add(Encoding.UTF8.GetString(login,0,kount));
                        clients.Add(client);
                        streams.Add(client.GetStream());
                    }
                }
                while (true);
            }
            void InputStream()
            {
                do
                {
                    Array bufferedStreams = streams.ToArray();
                    foreach (NetworkStream stream in bufferedStreams)
                    {
                        if (stream.DataAvailable)
                        {
                            Console.WriteLine("[SERVER] " + "Получение данных");

                            byte[] am = new byte[4];
                            StringBuilder myCompleteMessage = new();

                            stream.Read(am, 0,4);
                            
                            byte[] myReadBuffer = new byte[int.Parse(Encoding.UTF8.GetString(am,0,4))];
                            stream.Read(myReadBuffer, 0, myReadBuffer.Length);

                            stream.Write(new byte[] {48}, 0, 1);
                            myCompleteMessage.AppendFormat("{0}", Encoding.UTF8.GetString(myReadBuffer, 0, myReadBuffer.Length));

                            Console.WriteLine(myCompleteMessage.ToString());
                            Protocol protocol = new();
                            protocol.Deserialize(myCompleteMessage.ToString());

                            if (protocol.data?.targetLogin != null)
                                streams[logins.IndexOf(protocol.data.targetLogin)].Write(protocol.SerializeToByte);
                        }
                    }

                }
                while (true);
            }
        }
       
        void AwaitForNewUser()
        {

            /* while (true)
             {
                 try
                 {
                     // Подключение клиента

                     NetworkStream stream = client.GetStream();
                     // Обмен данными
                     try
                     {
                         if (stream.CanRead)
                         {
                             byte[] myReadBuffer = new byte[1024];
                             StringBuilder myCompleteMessage = new();
                             int numberOfBytesRead = 0;
                             do
                             {
                                 numberOfBytesRead = stream.Read(myReadBuffer, 0, myReadBuffer.Length);
                                 myCompleteMessage.AppendFormat("{0}", Encoding.UTF8.GetString(myReadBuffer, 0, numberOfBytesRead));
                             }
                             while (stream.DataAvailable);
                             string time = myCompleteMessage.ToString();

                             Protocol.Data? data = JsonConvert.DeserializeObject<Protocol.Data>(myCompleteMessage.ToString());
                             if (data != null)
                             {
                                 Console.WriteLine(String.Format("[SERVER] Сообщение от пользователя '{0}':{1}\n" +
                                     "[SERVER] Размер: {2}\n" +
                                     "[SERVER] Время Клиент-Сервер: {3}\n" +
                                     "[SERVER] Структура: {4}",
                                     data.senderLogin, data.message,
                                     myCompleteMessage.Length,
                                     DateTime.Now - data.sendingTime,
                                     myCompleteMessage.ToString()));

                                 Byte[] responseData = Encoding.UTF8.GetBytes(myCompleteMessage.Length.ToString());
                                 stream.Write(responseData, 0, responseData.Length);
                             }

                         }
                     }
                     finally
                     {
                         stream.Close();
                         client.Close();
                     }
                 }
                 catch
                 {
                     server.Stop();
                     break;
                 }
             }*/
        }
    }
}

