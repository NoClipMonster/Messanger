using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MessangerClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
           Client client = new Client();
            /* while (true)
             {
                 string buf = Console.ReadKey(true).Key.ToString();
                 DateTime dateTime = DateTime.Now;
                 stream.Write(Encoding.UTF8.GetBytes(buf));
                 stream.Read(new byte[1], 0, 1);
                 Console.WriteLine(DateTime.Now - dateTime);
             }
            */
         
        }
         class Client
        {
            NetworkStream stream;
            TcpClient client;
           public Client()
            {
                client = new("91.224.137.146", 8080);
                stream = client.GetStream();


                string name = "admin";

                stream.Write(Encoding.UTF8.GetBytes(name, 0, name.Length));

                byte[] message = Exchange("Привет");

                stream.Write(message);

                Thread checkForInput = new Thread(check);
                checkForInput.Start();

            }
            public static byte[] Exchange(string outMessage)
            {
                Protocol protocol = new();
                protocol.data = new()
                {
                    senderLogin = "admin",
                    targetLogin = "admin",
                    sendingTime = DateTime.Now,
                    message = outMessage
                };
                return protocol.SerializeToByte;
                /*
                // Инициализация
                TcpClient client = new(address, port);
                Byte[] data = Encoding.UTF8.GetBytes(outMessage);
                NetworkStream stream = client.GetStream();
                try
                {
                    // Отправка сообщения
                    stream.Write(data, 0, data.Length);
                    // Получение ответа
                    Byte[] readingData = new Byte[256];
                    String responseData = String.Empty;
                    StringBuilder completeMessage = new();
                    int numberOfBytesRead = 0;
                    do
                    {
                        numberOfBytesRead = stream.Read(readingData, 0, readingData.Length);
                        completeMessage.AppendFormat("{0}", Encoding.UTF8.GetString(readingData, 0, numberOfBytesRead));
                    }
                    while (stream.DataAvailable);
                    responseData = completeMessage.ToString();
                    return responseData;
                }
                finally
                {
                    stream.Close();
                    client.Close();
                }*/
            }
            public void check()
            {
                do
                {
                    if (stream.DataAvailable)
                    {
                        Console.WriteLine("[CLIENT] " + "Получение данных");

                        byte[] am = new byte[4];
                        StringBuilder myCompleteMessage = new();

                        stream.Read(am, 0, 4);

                        byte[] myReadBuffer = new byte[int.Parse(Encoding.UTF8.GetString(am, 0, 4))];
                        stream.Read(myReadBuffer, 0, myReadBuffer.Length);

                        myCompleteMessage.AppendFormat("{0}", Encoding.UTF8.GetString(myReadBuffer, 0, myReadBuffer.Length));

                        Console.WriteLine(myCompleteMessage.ToString());
                        Protocol protocol = new();
                        protocol.Deserialize(myCompleteMessage.ToString());

                        Console.WriteLine(String.Format(
                            "Сообщение от: {0}\n" +
                            "Отправленно в: {1}\n" + protocol.data.message,
                            protocol.data.senderLogin,
                            protocol.data.sendingTime));
                    }


                }
                while (true);

            }
        }
    }
       
}
