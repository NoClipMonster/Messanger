using Newtonsoft.Json;
using static MessangerServer.MyConsole;

namespace MessangerServer
{
    internal class Settings
    {
        string ip = "192.168.1.101";
        int port = 8080;
        bool throwAnException = false;
        string path = @"settings.json";

        public string Ip
        {
            get { return ip; }
            set
            {
                ip = value;
                WriteLine("Значение параметра Ip установленно на " + value, MsgType.Settings);
                Save();
            }
        }
        public int Port
        {
            get { return port; }
            set
            {
                port = value;
                WriteLine("Значение параметра Port установленно на " + value, MsgType.Settings);
                Save();
            }
        }
        public bool ThrowAnException
        {
            get { return throwAnException; }
            set
            {
                throwAnException = value;
                WriteLine("Значение параметра ThrowAnException установленно на " + value, MsgType.Settings);
                Save();
            }
        }
        public string Path
        {
            get { return path; }
            set
            {
                path = value;
                WriteLine("Значение параметра Path установленно на " + value, MsgType.Settings);
                Save();
            }
        }

        public void Save() =>System.IO.File.WriteAllText(Path, JsonConvert.SerializeObject(this));

        public void Load()
        {
            if (System.IO.File.Exists(Path))
            {
                object? obj = JsonConvert.DeserializeObject<Settings>(System.IO.File.ReadAllText(Path));
                if (obj is Settings sett)
                {
                    ip = sett.ip;
                    port = sett.port;
                    throwAnException = sett.throwAnException;
                    path = sett.path;
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
            string str = "\nIp: " + ip +
                "\nPort: " + port +
                "\nThrowAnException: " + throwAnException +
                "\nPath: " + path;

            WriteLine(str, MsgType.Settings);
        }
    }
}
