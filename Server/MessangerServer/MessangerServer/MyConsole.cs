namespace MessangerServer
{
    internal class MyConsole
    {
        public static void WriteLine(string message, MsgType type = MsgType.Information)
        {
            switch (type)
            {
                case MsgType.Information:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write(DateTime.Now.ToShortTimeString() + " [SERVER] ");
                    break;
                case MsgType.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(DateTime.Now.ToShortTimeString() + " [WARN] ");
                    break;
                case MsgType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(DateTime.Now.ToShortTimeString() + " [ERROR] ");
                    break;
                case MsgType.Settings:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(DateTime.Now.ToShortTimeString() + " [SETTINGS] ");
                    break;
                case MsgType.Client:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(DateTime.Now.ToShortTimeString() + " [CLIENT] ");
                    break;
            }
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
        public enum MsgType
        {
            Warning,
            Information,
            Error,
            Settings,
            Client
        }
    }
}
