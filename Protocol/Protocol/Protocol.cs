using Newtonsoft.Json;
using System;

namespace Protocol
{
    /// <summary>
    /// 
    /// </summary>
    internal class Protocol
    {

        public Data data;

        [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
        public class Data
        {
            [JsonRequired]
            public string senderLogin;

            [JsonRequired]
            public string senderPassword;

            [JsonRequired]
            public string targetLogin;

            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
            public DateTime sendingTime;

            public string message;

        }

        public string Serialized
        {
            get
            {
                string package = JsonConvert.SerializeObject(data, Formatting.Indented);
                return package;
            }
        }
        public void Deserialize(string innerData)
        {
            data = JsonConvert.DeserializeObject<Data>(innerData);
        }
    }
}

