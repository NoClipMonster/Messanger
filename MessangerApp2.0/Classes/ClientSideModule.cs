using Protocol;
using System;
using System.Net.Sockets;
using System.Windows;
using static Protocol.Data;

namespace MessangerApp2._0
{
    public class ClientSideModule
    {
        string IP = "192.168.1.101";
        int PORT = 8080;

        Protocol.Protocol protocol = new(4);
        TcpClient client = new();

        public delegate void MessageHandler(dynamic message);
        public event MessageHandler OnAnswerReceived;

        byte[] sessionId;
        public byte[] SessionId { get { return sessionId; } }

        dynamic SendPackage(byte[] message)
        {
            try
            {
                client=new(IP, PORT);
                client.GetStream().Write(message);
                byte[] size = new byte[protocol.bytesOfSizeAmount];
                while (!client.GetStream().DataAvailable)
                {

                }
                for (int i = 0; i < size.Length; i++)
                {
                    size[i] = (byte)client.GetStream().ReadByte();
                }

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
                client.Close();
                return protocol.Deserialize(myReadBuffer);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Не удалось отправить пакет данных");
                if (client.Connected)
                    client.Close();
                return Array.Empty<byte>();
            }
        }

        public void Login(string login, string pass)
        {
            var answer = SendPackage(protocol.SerializeToByte(new Data.Command{connection = new() { Login = login, Password = pass } }, MessageType.Command));
            if (answer is Data.Answer.Session session)
            {
                sessionId = session.SessionId;
                OnAnswerReceived(Data.StatusType.Ok);
            }
            else OnAnswerReceived(Data.StatusType.AuthorizationDenied);
            
        }

    }
}
