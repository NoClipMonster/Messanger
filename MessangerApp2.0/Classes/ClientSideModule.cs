using Protocol;
using System;
using System.Net.Sockets;

using System.Windows;
using static Protocol.Data;

namespace MessangerApp2._0
{
    public class ClientSideModule
    {
        public Settings settings;
        public Classes.MessagesStorage messages;
        public Classes.ContactsStorage contacts;

        Protocol.Protocol protocol = new(4);
        TcpClient client = new();

        public delegate void MessageHandler(dynamic message);
        public event MessageHandler OnAnswerReceived;
        public event MessageHandler OnMessagesReceived;


        public byte[] SessionId { get { return settings.SessionId; } set { settings.SessionId = value; } }
        public string UserId { get { return settings.ClientInfo.Id; } set { settings.ClientInfo.Id = value; } }
        public string UserName { get { return settings.ClientInfo.Name; } set { settings.ClientInfo.Name = value; } }

        public ClientSideModule()
        {
            settings = new();
            messages = new();
            contacts = new();
        }

       public  dynamic SendPackage(byte[] message)
        {
            try
            {
                client = new(settings.IP, settings.PORT);
                client.GetStream().Write(message);
                byte[] size = new byte[protocol.bytesOfSizeAmount];
                DateTime dT = DateTime.Now;
                while (!client.GetStream().DataAvailable)
                {
                    if ((DateTime.Now - dT).TotalSeconds > 5)
                    {
                        return Data.StatusType.TimeOut;
                    }
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
                if (settings.ThrowExceptions)
                     throw ex;
                MessageBox.Show(ex.Message, "Не удалось отправить пакет данных");
                if (client.Connected)
                    client.Close();
                return Array.Empty<byte>();
            }
        }
        
        public void CheckSession()
        {

            if (settings.SessionId!=null && settings.SessionId.Length == 16)
            {
                dynamic answer = SendPackage(protocol.SerializeToByte(new Answer.Session() { SessionId = settings.SessionId }, Data.MessageType.SessionId));
                if (answer is Answer.User user)
                {
                    settings.ClientInfo = user;
                    OnAnswerReceived(StatusType.Ok);
                }
            }
            else
            {
                settings.SessionId = Array.Empty<byte>();
            }
        }

        public void CheckForMessage(object? obj)
        {
            try
            {
                
                if (Application.Current != null)
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        DateTime dt = DateTime.Now;
                        dynamic answer = SendPackage(protocol.SerializeToByte(new Command.GetMessages(SessionId, settings.ClientInfo.Id, settings.LastMessageCheck, dt), MessageType.Command));
                        if (answer is Answer.Message[] messages && messages.Length > 0)
                        {
                            settings.LastMessageCheck = dt;
                            OnMessagesReceived(messages);
                        }
                    });
            }
            catch (Exception ex)
            {
                if (settings.ThrowExceptions)
                    throw;
            }

        }

        public void Login(string login, string pass)
        {
            try
            {
                dynamic answer = SendPackage(protocol.SerializeToByte(new Command.Connection(login, pass), MessageType.Command));
                if (answer is Data.Answer.Session session)
                {
                    SessionId = session.SessionId;
                    settings.SessionId = session.SessionId;
                    if (session.User != null)
                        settings.ClientInfo = session.User;
                    OnAnswerReceived(Data.StatusType.Ok);
                }
                else if (answer is Data.StatusType)
                {
                    OnAnswerReceived(answer);
                }
                else OnAnswerReceived(Data.StatusType.Error);
            }
            catch (Exception ex)
            {
                if (settings.ThrowExceptions)
                    throw ex;
                MessageBox.Show(ex.Message);
                OnAnswerReceived(Data.StatusType.Error);
            }

        }

        public void Disconection()
        {
            try
            {
                contacts.DeleteAll();
                messages.DeleteAll();
                settings.ClearUserData();
                SendPackage(protocol.SerializeToByte(new Command.Disconnection(SessionId), MessageType.Command));
            }
            catch (Exception ex)
            {
                if (settings.ThrowExceptions)
                    throw ex;
                MessageBox.Show(ex.Message);
                OnAnswerReceived(Data.StatusType.Error);
            }
        }

        public void Registration(string login, string pass, string name, string description = "")
        {
            try
            {
                dynamic answer = SendPackage(protocol.SerializeToByte(new Command.Registration(login, pass, name, description), MessageType.Command));

                if (answer is Data.StatusType)
                {
                    OnAnswerReceived(answer);
                }
                else OnAnswerReceived(Data.StatusType.Error);
            }
            catch (Exception ex)
            {
                if (settings.ThrowExceptions)
                    throw ex;
                MessageBox.Show(ex.Message);
                OnAnswerReceived(Data.StatusType.Error);
            }
        }

        public void SendMessage(string login, string text,bool isGroup)
        {
            try
            {
                if (isGroup)
                {
                    SendPackage(protocol.SerializeToByte(new GroupMessage(SessionId, login, UserId, text), MessageType.GroupMessage));
                }
                else
                SendPackage(protocol.SerializeToByte(new DirectMessage(SessionId, login, UserId, text), MessageType.DirectMessage));
                //CheckForMessage(null);
            }
            catch (Exception ex)
            {
                if (settings.ThrowExceptions)
                    throw ex;
                MessageBox.Show(ex.Message);
                OnAnswerReceived(Data.StatusType.Error);
            }
        }

        public dynamic FindUser(string UserId)
        {
            return SendPackage(protocol.SerializeToByte(new Command.FindUser(SessionId, UserId), MessageType.Command));
        }

        public dynamic FindGroup(string UserId)
        {
            return SendPackage(protocol.SerializeToByte(new Command.FindGroup(SessionId, UserId), MessageType.Command));
        }

        public void CreateGroup(string GroupId, string GroupName, string Description)
        {
            try
            {
                dynamic answer = SendPackage(protocol.SerializeToByte(new Command.CreateGroup(SessionId, UserId, GroupId, GroupName, Description), MessageType.Command));

                if (answer is Data.StatusType)
                {
                    OnAnswerReceived(answer);
                }
                else OnAnswerReceived(Data.StatusType.Error);
            }
            catch (Exception ex)
            {
                if (settings.ThrowExceptions)
                    throw ex;
                MessageBox.Show(ex.Message);
                OnAnswerReceived(Data.StatusType.Error);
            }

        }

    }
}
