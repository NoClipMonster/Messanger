using Newtonsoft.Json;
using System.Text;

namespace MessangerServer
{
    internal class Protocol
    {
        public Data data;

        [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
        public class Data
        {
            [JsonRequired]
            public string senderLogin;

            [JsonRequired]
            public string targetLogin;

            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
            public DateTime sendingTime;

            public string message;

        }
        public byte[] SerializeToByte
        {
            get
            {
                List<byte> bytes = new List<byte>();
                bytes.AddRange(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));
                List<byte> lenght = new List<byte>();
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
    }
}
