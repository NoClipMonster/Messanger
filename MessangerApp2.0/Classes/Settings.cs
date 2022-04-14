using Newtonsoft.Json;
using System;
using System.Windows;
using System.Windows.Media;

namespace MessangerApp2._0
{
    public class Settings
    {
        Classes.FileLoader<Data> loader;
        string path = @"Settings.json";
        public Settings()
        {
            
            try
            {
            loader = new(path);
                if (loader.Data == null)
                    loader.Data = new Data();
            }
            catch(Exception)
            {
                loader = new();
                loader.Data = new Data();
            }
        }
        [JsonObject]
        protected class Data
        {
            public Protocol.Data.Answer.User ClientInfo = new();

            public string IP = "192.168.1.101";

            public int PORT = 8080;

            public bool ThrowExceptions = false;

            public DateTime LastMessageCheck = DateTime.MinValue;

            public byte[] SessionId = Array.Empty<byte>();

            public Brush MainColor = new SolidColorBrush(new Color() { R = 32, G = 32, B = 32, A = 255 });

            public Brush AdditionalColor = new SolidColorBrush(new Color() { R = 45, G = 45, B = 45, A = 255 });
            public Data()
            {
            }
            public Data(Protocol.Data.Answer.User clientInfo, string iP, int pORT,bool throwExceptions, DateTime lastMessageCheck, byte[] sessionId, Brush mainColor, Brush additionalColor)
            {
                ClientInfo = clientInfo;
                IP = iP;
                PORT = pORT;
                ThrowExceptions = throwExceptions;
                LastMessageCheck = lastMessageCheck;
                SessionId = sessionId;
                MainColor = mainColor;
                AdditionalColor = additionalColor;
            }

        }
        public void ClearUserData()
        {
            loader.Data.ClientInfo = new();
            loader.Data.LastMessageCheck = DateTime.MinValue;
            loader.Data.SessionId = Array.Empty<byte>();
            loader.Save();
        }
        public Protocol.Data.Answer.User ClientInfo
        {
            get { return loader.Data.ClientInfo; }
            set { loader.Data.ClientInfo = value; loader.Save(); }
        }
        public string IP
        {
            get { return loader.Data.IP; }
            set { loader.Data.IP = value; loader.Save(); }
        }
        public int PORT
        {
            get { return loader.Data.PORT; }
            set { loader.Data.PORT = value; loader.Save(); }
        }
        public bool ThrowExceptions
        {
            get { return loader.Data.ThrowExceptions; }
            set { loader.Data.ThrowExceptions = value; loader.Save(); }
        }
        public DateTime LastMessageCheck
        {
            get { return loader.Data.LastMessageCheck; }
            set { loader.Data.LastMessageCheck = value; loader.Save(); }
        }

        public byte[] SessionId
        {
            get { return loader.Data.SessionId; }
            set { loader.Data.SessionId = value; loader.Save(); }
        }

        public Brush MainColor
        {
            get { return loader.Data.MainColor; }
            set { loader.Data.MainColor = value; loader.Save(); }
        }

        public Brush AdditionalColor
        {
            get { return loader.Data.AdditionalColor; }
            set { loader.Data.AdditionalColor = value; loader.Save(); }
        }
    }
}
