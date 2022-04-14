using Newtonsoft.Json;
using System.Text;
using static Protocol.Data;

namespace Protocol
{

    /* Пример использования
    public class Protocol
    {
        static void Main()
        {
            DirectMessage direct = new() { targetLogin = "someone", senderLogin = "Admin", message = "Привет", sendingTime = DateTime.Now, dataset = new Dataset(@"C:\Users\iaved\Desktop\привет.txt") };
            GroupMessage group = new() { groupIdLogin = "anyone", senderLogin = "Admin", message = "Привет", sendingTime = DateTime.Now, dataset = new Dataset(@"C:\Users\iaved\Desktop\привет.txt") };
            Command commandCon = new(Command.CommandType.Connection, "adminlog", "adminpass");
            Command commandDis = new(Command.CommandType.Disconnection, "adminlog");

            MyProtocol mydirectProtocol = new(direct);
            MyProtocol mygroupProtocol = new(group);
            MyProtocol mycommandconProtocol = new(commandCon);
            MyProtocol mycommanddisProtocol = new(commandDis);
            byte[] buffer, buffer2;

            buffer = mydirectProtocol.SerializeToByte();
            buffer2 = new byte[buffer.Length - 4];
            Array.Copy(buffer, 4, buffer2, 0, buffer2.Length);

            MyProtocol mydirectProtocol2 = new(buffer2);


            buffer = mygroupProtocol.SerializeToByte();
            buffer2 = new byte[buffer.Length - 4];
            Array.Copy(buffer, 4, buffer2, 0, buffer2.Length);

            MyProtocol mygroupProtocol2 = new(buffer2);


            buffer = mycommandconProtocol.SerializeToByte();
            buffer2 = new byte[buffer.Length - 4];
            Array.Copy(buffer, 4, buffer2, 0, buffer2.Length);

            MyProtocol mycommandProtocol2 = new(buffer2);


            buffer = mycommanddisProtocol.SerializeToByte();
            buffer2 = new byte[buffer.Length - 4];
            Array.Copy(buffer, 4, buffer2, 0, buffer2.Length);

            MyProtocol mycommanddisProtocol2 = new(buffer2);

        }
    }
    */
    public class Protocol
    {
        public readonly int bytesOfSizeAmount;
        public Protocol(int bytesOfSizeAmount)
        {
            this.bytesOfSizeAmount = bytesOfSizeAmount;
        }

        /// <summary>
        /// <para>
        /// Размер пакета в количестве <paramref name="bytesOfSizeAmount"/> байт 
        /// </para>
        /// <para>
        ///Тип пакета в размере 1 байта
        /// </para>
        /// <para>
        /// Пакет размера до (256^ bytesOfSizeAmount) - 1
        /// </para>
        /// </summary>
        /// <returns>Размер, тип, и содержимое пакета в виде byte[]</returns>

        public byte[] SerializeToByte(dynamic Data, MessageType messageType)
        {
            if (Data == null)
                return Array.Empty<byte>();
            byte[] bufByteData;
            Dataset? dataset = null;

          
            bufByteData = Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(Data));
            if (messageType == MessageType.DirectMessage || messageType == MessageType.GroupMessage || messageType == MessageType.File)
                dataset = Data.dataset;
            byte[] byteData;
            if(messageType == MessageType.Command)
            {
                byteData = new byte[bufByteData.Length + 2];
                byteData[0] = (byte)messageType;
                byteData[1] = (byte)Data.commandType;
                Array.Copy(bufByteData, 0, byteData, 2, bufByteData.Length);
            }
            else
            {
                byteData = new byte[bufByteData.Length + 1];
                byteData[0] = (byte)messageType;
                Array.Copy(bufByteData, 0, byteData, 1, bufByteData.Length);
            }
           
            if (byteData.Length > Math.Pow(256, bytesOfSizeAmount))
            {
                throw new Exception("Packet size is too large");
            }
            byte[] size = new byte[bytesOfSizeAmount];

            for (int i = size.Length - 1; i >= 0; i--)
            {
                size[i] = (byte)((byteData.Length % Math.Pow(256, size.Length - i)) / Math.Pow(256, size.Length - (i + 1)));
            }

            byte[] message = new byte[size.Length + byteData.Length];
            size.CopyTo(message, 0);
            if (dataset != null)
            {
                while (dataset.fileWaiter.IsBusy)
                { };
            }

            byteData.CopyTo(message, size.Length);
            return message;
        }

        public dynamic? Deserialize(byte[] innerData)
        {
            if (innerData != null)
            {
                string str;
                if ((MessageType)innerData[0] == MessageType.Command)
                    str = Encoding.Unicode.GetString(innerData, 2, innerData.Length - 2);
                else
                 str = Encoding.Unicode.GetString(innerData, 1, innerData.Length - 1);
                switch ((MessageType)innerData[0])
                {
                    case MessageType.DirectMessage:
                        return JsonConvert.DeserializeObject<DirectMessage>(str);
                    case MessageType.GroupMessage:
                        return JsonConvert.DeserializeObject<GroupMessage>(str);
                    case MessageType.Command:
                        switch ((Command.CommandType)innerData[1])
                        {
                            case Command.CommandType.Connection:
                                return JsonConvert.DeserializeObject<Command.Connection>(str);
                            case Command.CommandType.Disconnection:
                                return JsonConvert.DeserializeObject<Command.Disconnection>(str);
                            case Command.CommandType.Registration:
                                return JsonConvert.DeserializeObject<Command.Registration>(str);
                            case Command.CommandType.FindUser:
                                return JsonConvert.DeserializeObject<Command.FindUser>(str);
                            case Command.CommandType.GetMessages:
                                return JsonConvert.DeserializeObject<Command.GetMessages>(str);
                            case Command.CommandType.GetFile:
                                return JsonConvert.DeserializeObject<Command.GetFile>(str);
                            case Command.CommandType.CheckSession:
                                return JsonConvert.DeserializeObject<byte[]>(str);
                            case Command.CommandType.CreateGroup:
                                return JsonConvert.DeserializeObject<Command.CreateGroup>(str);
                            case Command.CommandType.FindGroup:
                                return JsonConvert.DeserializeObject<Command.FindGroup>(str);
                        }
                        break;
                    case MessageType.Status:
                        return JsonConvert.DeserializeObject<StatusType>(str);
                    case MessageType.SessionId:
                        return JsonConvert.DeserializeObject<Answer.Session>(str);
                    case MessageType.User:
                        return JsonConvert.DeserializeObject<Answer.User>(str);
                    case MessageType.Users:
                        return JsonConvert.DeserializeObject<Answer.User[]>(str);
                    case MessageType.Messages:
                        return JsonConvert.DeserializeObject<Answer.Message[]>(str);
                    case MessageType.File:
                        return JsonConvert.DeserializeObject<Answer.File>(str);
                    case MessageType.Group:
                        return JsonConvert.DeserializeObject<Answer.Group>(str);
                    case MessageType.Groups:
                        return JsonConvert.DeserializeObject<Answer.Group[]>(str);


                }

            }
            return null;
        }
    }
}