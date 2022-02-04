using PacketDotNet;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Interceptor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ICaptureDevice captureDevice;
        BackgroundWorker bw;
        public MainWindow()
        {
            bw = new BackgroundWorker();
            bw.DoWork += Bw_DoWork;
            InitializeComponent();
            // метод для получения списка устройств
            CaptureDeviceList deviceList = CaptureDeviceList.Instance;
            // выбираем первое устройство в спсике (для примера)
            captureDevice = deviceList[3];
            // регистрируем событие, которое срабатывает, когда пришел новый пакет
            captureDevice.OnPacketArrival += new PacketArrivalEventHandler(Program_OnPacketArrival);
            // открываем в режиме promiscuous, поддерживается также нормальный режим
            captureDevice.Open(DeviceMode.Promiscuous, 1000);
            // начинаем захват пакетов
            bw.WorkerReportsProgress = true;
            bw.ProgressChanged += Bw_ProgressChanged;
            bw.RunWorkerAsync();
            
            
        }

        private void Bw_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            string str = "";
            foreach(byte b in (byte[])e.UserState)
            {
                str += b + "  :  ";
            }
            MainList.Items.Add(new Label() { Content = str });
            MainList.Items.Add(new Label() { Content = Encoding.UTF8.GetString((byte[])e.UserState) });
        }

        private void Bw_DoWork(object? sender, DoWorkEventArgs e)
        {
            captureDevice.Capture();
        }

        void Program_OnPacketArrival(object sender, CaptureEventArgs e)
        {

            if (e.Packet.LinkLayerType == LinkLayers.Null)
                return;
            // парсинг всего пакета
            Packet packet = Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
            
            // получение только TCP пакета из всего фрейма
            var tcpPacket = TcpPacket.GetEncapsulated(packet);
            // получение только IP пакета из всего фрейма
            var ipPacket = IpPacket.GetEncapsulated(packet);
            if (tcpPacket != null && ipPacket != null)
            {
            if(tcpPacket.DestinationPort!=8080 && tcpPacket.SourcePort != 8080)
                {
                    return;
                }
                DateTime time = e.Packet.Timeval.Date;
                int len = e.Packet.Data.Length;

                // IP адрес отправителя
                var srcIp = ipPacket.SourceAddress.ToString();
                // IP адрес получателя
                var dstIp = ipPacket.DestinationAddress.ToString();
                if (dstIp == "91.224.137.146")
                    bw.ReportProgress(len, e.Packet.Data);
                // порт отправителя
                var srcPort = tcpPacket.SourcePort.ToString();
                // порт получателя
                var dstPort = tcpPacket.DestinationPort.ToString();
                // данные пакета
                var data = tcpPacket.PayloadPacket;
                ;
                
            }
        }
    }
}
