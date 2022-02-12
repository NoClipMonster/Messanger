using Newtonsoft.Json;
using System.Text;

namespace Protocol
{
    public class MyProtocol
    {
        public Data? data;

        public MyProtocol(byte[] innerData)
        {
            Deserialize(innerData);
        }
        public MyProtocol(string innerData)
        {
            Deserialize(innerData);
        }

        public MyProtocol(Data innerData) => data = innerData;


        public byte[] SerializeToByte
        {
            get
            {
                if (data == null)
                    return Array.Empty<byte>();
                byte[] byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
                byte[] size = new byte[3];
                int objLenght = data.sizeOfObject ?? 0;
                for (int i = size.Length - 1; i >= 0; i--)
                {
                    size[i] = (byte)((byteData.Length % Math.Pow(256, size.Length - i)) / Math.Pow(256, size.Length - (i + 1)));
                }

                byte[] message = new byte[size.Length + byteData.Length + objLenght];
                size.CopyTo(message, 0);
                byteData.CopyTo(message, size.Length);
                if (data != null && data.SomeObject != null)
                {
                    data.SomeObject.CopyTo(message, size.Length + byteData.Length);
                }
                return message;
            }
        }

        public void Deserialize(string innerData)
        {
            if (innerData != null)
            {
                try
                {
                    data = new();
                    data = JsonConvert.DeserializeObject<Data>(innerData);
                }
                catch (Exception)
                {

                }

            }
        }

        public void Deserialize(byte[] innerData)
        {
            if (innerData != null)
            {
                Deserialize(Encoding.UTF8.GetString(innerData));
            }
        }

    }
}