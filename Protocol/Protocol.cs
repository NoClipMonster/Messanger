using Newtonsoft.Json;
using System.Text;

namespace Protocol
{
    public class Protocol
    {
        static void Main()
        {
            DirectMessage direct = new() { targetLogin = "someone", senderLogin = "Admin", message = "Привет", sendingTime = DateTime.Now, dataset = new Dataset(@"C:\Users\iaved\Desktop\привет.txt") };
            GroupMessage group = new() { groupIdLogin = "anyone", senderLogin = "Admin", message = "Привет", sendingTime = DateTime.Now, dataset = new Dataset(@"C:\Users\iaved\Desktop\привет.txt") };
            Command command = new() { commandType = Command.CommandType.Connection, command = "админ|админ" };

            MyProtocol mydirectProtocol = new(direct);
            MyProtocol mygroupProtocol = new(group);
            MyProtocol mycommandProtocol = new(command);

            byte[] buffer, buffer2;

            buffer = mydirectProtocol.SerializeToByte();
            buffer2 = new byte[buffer.Length - 4];
            Array.Copy(buffer, 4, buffer2, 0, buffer2.Length);

            MyProtocol mydirectProtocol2 = new(buffer2);


            buffer = mygroupProtocol.SerializeToByte();
            buffer2 = new byte[buffer.Length - 4];
            Array.Copy(buffer, 4, buffer2, 0, buffer2.Length);

            MyProtocol mygroupProtocol2 = new(buffer2);


            buffer = mycommandProtocol.SerializeToByte();
            buffer2 = new byte[buffer.Length - 4];
            Array.Copy(buffer, 4, buffer2, 0, buffer2.Length);

            MyProtocol mycommandProtocol2 = new(buffer2);

        }
    }

    public class MyProtocol
    {
        MessageType messageType;
        public MessageType MessageType
        {
            get
            {
                return messageType;
            }
        }
        public const int bytesOfSizeAmount = 4;
        DirectMessage directMessage;
        GroupMessage groupMessage;
        Command command;
        public MyProtocol(byte[] innerData)
        {
            Deserialize(innerData);
        }

        public MyProtocol(DirectMessage message)
        {
            directMessage = message;
            messageType = MessageType.Direct;
        }
        public MyProtocol(GroupMessage message)
        {
            groupMessage = message;
            messageType = MessageType.Group;
        }
        public MyProtocol(Command command)
        {
            this.command = command;
            messageType = MessageType.Command;
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
        public byte[] SerializeToByte()
        {
            byte[] byteData;
            Dataset? dataset = null;
            switch (MessageType)
            {
                case MessageType.Direct:
                    if (directMessage == null)
                        return Array.Empty<byte>();
                    byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(directMessage));
                    dataset = directMessage.dataset;
                    break;

                case MessageType.Group:
                    if (groupMessage == null)
                        return Array.Empty<byte>();
                    byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(groupMessage));
                    dataset = groupMessage.dataset;
                    break;

                case MessageType.Command:
                    if (command == null)
                        return Array.Empty<byte>();
                    byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(command));
                    break;
                default:
                    byteData = Array.Empty<byte>();
                    break;
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

            byte[] message = new byte[size.Length + 1 + byteData.Length];
            size.CopyTo(message, 0);
            message[size.Length] = (byte)MessageType;
            if (dataset != null)
            {
                while (dataset.fileWaiter.IsBusy)
                { };
            }
            byteData.CopyTo(message, size.Length + 1);
            return message;
        }

        /// <summary>
        /// 0-direct
        /// 1-group
        /// 2-command
        /// </summary>
        /// <param name="innerData"></param>
        public void Deserialize(byte[] innerData)
        {
            if (innerData != null)
            {
                string str = Encoding.UTF8.GetString(innerData, 1, innerData.Length - 1);
                switch (innerData[0])
                {
                    case 0:
                        directMessage = JsonConvert.DeserializeObject<DirectMessage>(str) ?? new();
                        messageType = MessageType.Direct;
                        break;
                    case 1:
                        groupMessage = JsonConvert.DeserializeObject<GroupMessage>(str) ?? new();
                        messageType = MessageType.Group;
                        break;
                    case 2:
                        command = JsonConvert.DeserializeObject<Command>(str) ?? new();
                        messageType = MessageType.Command;
                        break;
                    default:
                        break;
                }

            }
        }



    }
}