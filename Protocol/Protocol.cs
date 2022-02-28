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
            byte[] byteData;
            Dataset? dataset = null;

            switch (messageType)
            {
                case MessageType.Direct:
                    byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Data as DirectMessage));
                    break;

                case MessageType.Group:
                    byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Data as GroupMessage));
                    break;

                case MessageType.Command:
                    byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Data as Command));
                    break;
                default:
                    byteData = Array.Empty<byte>();
                    break;
            }
            if (messageType != MessageType.Command)
                dataset = Data.Dataset;
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
            message[size.Length] = (byte)messageType;
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
        public dynamic Deserialize(byte[] innerData,out MessageType messageType)
        {
            if (innerData != null)
            {
                string str = Encoding.UTF8.GetString(innerData, 1, innerData.Length - 1);
                switch (innerData[0])
                {
                    case 0:
                        messageType = MessageType.Direct;
                        return JsonConvert.DeserializeObject<DirectMessage>(str) ?? new DirectMessage();
                       
                    case 1:
                        messageType = MessageType.Group;
                        return JsonConvert.DeserializeObject<GroupMessage>(str) ?? new GroupMessage();
                        
                    case 2:
                        messageType = MessageType.Command;
                        return JsonConvert.DeserializeObject<Command>(str) ?? new Command();
                }

            }
            messageType = MessageType.Command;
            return null;
        }
    }
}