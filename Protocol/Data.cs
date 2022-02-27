using System.ComponentModel;

namespace Protocol
{
    public enum MessageType { Direct, Group, Command };
    public class Command
    {
        public enum CommandType {Connection, Disconnection}
        public CommandType commandType;
        public string command = string.Empty;
    }
    public class DirectMessage : Message
    {
        public string targetLogin = String.Empty;
    }

    public class GroupMessage : Message
    {
        public string groupIdLogin = String.Empty;
    }

    public class Message
    {
        public string senderLogin = String.Empty;
        public DateTime sendingTime = DateTime.Now;
        public string message = String.Empty;
        public Dataset? dataset;
    }

    public class Dataset
    {
        public BackgroundWorker fileWaiter;
        public Dataset(string path)
        {
            fileWaiter = new BackgroundWorker();
            fileWaiter.DoWork += FileWaiter_DoWork;
            fileWaiter.RunWorkerAsync(path);

            void FileWaiter_DoWork(object? sender, DoWorkEventArgs e)
            {
                if (e.Argument != null)
                {
                    someObject = File.ReadAllBytes((string)e.Argument);
                    sizeOfObject = someObject.Length;
                    nameOfObject = new FileInfo((string)e.Argument).Name;
                }
            }
        }

        public byte[] someObject = Array.Empty<byte>();

        public int sizeOfObject = -1;

        public string nameOfObject = String.Empty;

    }
}
