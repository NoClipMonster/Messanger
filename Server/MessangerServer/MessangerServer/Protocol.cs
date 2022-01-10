using Newtonsoft.Json;
using System.Text;



namespace MessangerServer
{
    internal class Protocol
    {
        public Data? data;

        public Protocol(byte[] innerData)
        {
            data = new();
            Deserialize(innerData);
        }
        public Protocol(string innerData)
        {
            data = new();
            Deserialize(innerData);
        }

        public Protocol(Data innerData) => data = innerData;

        [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
        public class Data
        {
            
            [JsonRequired]
            public string? senderLogin = null;

            [JsonRequired]
            public string? targetLogin = null;

            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
            public DateTime? sendingTime = null;

            public string? message = null;

            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
            public bool closeConnection = false;

        }
        public byte[] SerializeToByte
        {
            get
            {
                List<byte> bytes = new();
                bytes.AddRange(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));
                List<byte> lenght = new();
                lenght.AddRange(Encoding.UTF8.GetBytes(bytes.Count.ToString()));
                while (lenght.Count < 4)
                    lenght.Insert(0, 48);
                bytes.InsertRange(0, lenght);
                return bytes.ToArray();
            }
        }
        public void Deserialize(string innerData)
        {
            if (innerData != null)
                data = JsonConvert.DeserializeObject<Data>(innerData);
        }
        public void Deserialize(byte[] innerData)
        {
            if (innerData != null)
                data = JsonConvert.DeserializeObject<Data>(Encoding.UTF8.GetString(innerData));
        }
    }
}
