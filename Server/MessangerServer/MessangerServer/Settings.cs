using Newtonsoft.Json;
using static MessangerServer.MyConsole;

namespace MessangerServer
{
    internal class Settings
    {
        internal class Data
        {
           public string ip = "192.168.1.101";
            public int port = 8080;
            public bool throwAnException = false;
            public bool showClientCommands = false;
            public string path = @"settings.json";
        }
        Data data = new();
        public bool ShowClientCommands
        {
            get { return data.showClientCommands; }
            set
            {
                data.showClientCommands = value;
                WriteLine("Значение параметра ShowClientCommands установленно на " + value, MsgType.Settings);
                Save();
            }
        }

        public string Ip
        {
            get { return data.ip; }
            set
            {
                data.ip = value;
                WriteLine("Значение параметра Ip установленно на " + value, MsgType.Settings);
                Save();
            }
        }
        public int Port
        {
            get { return data.port; }
            set
            {
                data.port = value;
                WriteLine("Значение параметра Port установленно на " + value, MsgType.Settings);
                Save();
            }
        }
        public bool ThrowAnException
        {
            get { return data.throwAnException; }
            set
            {
                data.throwAnException = value;
                WriteLine("Значение параметра ThrowAnException установленно на " + value, MsgType.Settings);
                Save();
            }
        }
        public string Path
        {
            get { return data.path; }
            set
            {
                data.path = value;
                WriteLine("Значение параметра Path установленно на " + value, MsgType.Settings);
                Save();
            }
        }

        public void Save() =>System.IO.File.WriteAllText(Path, JsonConvert.SerializeObject(data));

        public void Load()
        {
            if (System.IO.File.Exists(Path))
            {
                object? obj = JsonConvert.DeserializeObject<Data>(System.IO.File.ReadAllText(Path));
                if (obj is Data sett)
                {
                    data = sett;
                    WriteLine("Настройки загруженны", MsgType.Settings);
                }
                else
                    throw new Exception("Данные в файле не соответствуют настройкам");
            }
            else
                throw new Exception("Отсутствует файл настроек");
        }

        public void Show()
        {
            string str = "\nIp: " + data.ip +
                "\nPort: " + data.port +
                "\nThrowAnException: " + data.throwAnException +
                "\nPath: " + data.path +
                "\nShowClientCommands: " + data.showClientCommands;

            WriteLine(str, MsgType.Settings);
        }
    }
}
