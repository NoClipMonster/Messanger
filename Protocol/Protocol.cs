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

            /*  switch (messageType)
              {
                  case MessageType.Direct:
                      bufByteData = Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(Data as DirectMessage));
                      break;

                  case MessageType.Group:
                      bufByteData = Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(Data as GroupMessage));
                      break;

                  case MessageType.Command:
                      bufByteData = Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(Data as Command));
                      break;
                  case MessageType.Status:
                      bufByteData = Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(Data));
                      break;
                  default:
                      bufByteData = Array.Empty<byte>();
                      break;
              }*/
            bufByteData = Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(Data));
            if (messageType != MessageType.Command && messageType != MessageType.Answer && messageType != MessageType.Status)
                dataset = Data.dataset;
            byte[] byteData = new byte[bufByteData.Length + 1];
            byteData[0] = (byte)messageType;
            Array.Copy(bufByteData, 0, byteData, 1, bufByteData.Length);
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

        public dynamic Deserialize(byte[] innerData)
        {
            if (innerData != null)
            {
                string str = Encoding.Unicode.GetString(innerData, 1, innerData.Length - 1);
                switch (innerData[0])
                {
                    case 0:
                        return JsonConvert.DeserializeObject<DirectMessage>(str) ?? new DirectMessage();
                    case 1:
                        return JsonConvert.DeserializeObject<GroupMessage>(str) ?? new GroupMessage();

                    case 2:
                        return JsonConvert.DeserializeObject<Command>(str) ?? new Command();
                    case 3:
                        return JsonConvert.DeserializeObject<StatusType>(str);
                    case 4:
                        return JsonConvert.DeserializeObject(str) ?? null;
                }

            }
            return null;
        }
    }
}