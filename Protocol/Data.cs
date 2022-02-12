using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Data
    {
        [JsonRequired]
        public string senderLogin = String.Empty;

        [JsonRequired]
        public string targetLogin = String.Empty;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime sendingTime = DateTime.Now;

        public string message = String.Empty;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool closeConnection;

        [JsonIgnore]
        public byte[]? SomeObject
        {
            set
            {
                if (value != null)
                {
                    someObject = value;
                    sizeOfObject = someObject.Length;
                }
            }
            get => someObject;
        }

        [JsonIgnore]
        byte[]? someObject;

        public int? sizeOfObject;

        public string? nameOfObject;

    }
  
}
