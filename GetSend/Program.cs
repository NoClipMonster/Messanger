using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Threading;

namespace sendget
{
    class Program
    {

        static void Main(string[] args)
        {
            BackgroundWorker bkw2 = new BackgroundWorker();
            Console.WriteLine("Hello World!");
            bkw2.DoWork += Bkw2_DoWork;
            bkw2.RunWorkerAsync();
            while (true)
            {

            }
        }
        static private void Bkw2_DoWork(object sender, DoWorkEventArgs e)
        {
            TcpListener listener = new TcpListener(System.Net.IPAddress.Parse("192.168.1.101"), 8080);
            listener.Start();
            TcpClient client = new TcpClient();
            client.Connect("91.224.137.146", 8080);
            TcpClient newClient = listener.AcceptTcpClient();
          
                Console.ReadLine();
                client.GetStream().Write(new byte[] { 234, 235 });
                byte[] bt = new byte[2];
                newClient.GetStream().Read(bt, 0, 2);
                Console.ReadLine();
                newClient.GetStream().Write(new byte[] { 235, 234 });
                byte[] bt2 = new byte[2];
                client.GetStream().Read(bt2, 0, 2);
                Console.WriteLine("Done");

            client.Close();
        }
    }
}
