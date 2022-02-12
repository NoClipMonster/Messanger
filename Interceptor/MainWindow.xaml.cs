using PacketDotNet;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Interceptor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly BackgroundWorker bkw = new();
        readonly List<CaptureEventArgs> packets = new();
        ushort port = 0;
        public MainWindow()
        {
            InitializeComponent();
            bkw.DoWork += Bkw_DoWork;
            bkw.WorkerReportsProgress = true;
            bkw.ProgressChanged += Bkw_ProgressChanged;
            bkw.RunWorkerAsync();
            filtBut_Click(new(),new());
        }

        private void Bkw_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is not CaptureEventArgs capture)
                return;
            Packet packet = Packet.ParsePacket(capture.Packet.LinkLayerType, capture.Packet.Data);
            TcpPacket tcpPacket = (TcpPacket)packet.Extract(typeof(TcpPacket));//TcpPacket.GetEncapsulated(packet);
            // получение только IP пакета из всего фрейма
            IpPacket ipPacket = (IpPacket)packet.Extract(typeof(IpPacket));
            if (tcpPacket != null && ipPacket != null)
            {
                if (tcpPacket.DestinationPort != port && port != 0)
                    return;
                packets.Add(capture);
                PacketsList.Items.Add(ipPacket.SourceAddress);
            }
        }


        private void Bkw_DoWork(object? sender, DoWorkEventArgs e)
        {
            CaptureDeviceList captureDevices = CaptureDeviceList.Instance;
            ICaptureDevice captureDevice = captureDevices[3];
            captureDevice.OnPacketArrival += CaptureDevice_OnPacketArrival;
            captureDevice.Open(DeviceMode.Promiscuous, 1000);
            captureDevice.Capture();


            void CaptureDevice_OnPacketArrival(object sender, CaptureEventArgs e)
            {
                bkw.ReportProgress(0, e);
            }
        }

        void Program_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            bkw.ReportProgress(0, e);
        }

        private void PacketsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (PacketsList.SelectedIndex == -1)
                return;
            Packet packet = Packet.ParsePacket(packets[PacketsList.SelectedIndex].Packet.LinkLayerType, packets[PacketsList.SelectedIndex].Packet.Data);
            TcpPacket tcpPacket = (TcpPacket)packet.Extract(typeof(TcpPacket));//TcpPacket.GetEncapsulated(packet);
            // получение только IP пакета из всего фрейма
            IpPacket ipPacket = (IpPacket)packet.Extract(typeof(IpPacket));

            timeL.Content = "Время:" + packets[PacketsList.SelectedIndex].Packet.Timeval.Date;
            lenghL.Content = "Длинна пакета:" + packets[PacketsList.SelectedIndex].Packet.Data.Length;

            // IP адрес отправителя
            srcIpL.Content = "IP адрес отправителя:" + ipPacket.SourceAddress.ToString();

            // IP адрес получателя
            dstIpL.Content = "IP адрес получателя:" + ipPacket.DestinationAddress.ToString();

            // порт отправителя
            srcPortL.Content = "Порт отправителя:" + tcpPacket.SourcePort.ToString();
            // порт получателя
            dstPortL.Content = "Порт получателя:" + tcpPacket.DestinationPort.ToString();
            ipPacketBytesL.Content = "Байты IP пакета:";
            if (ipPacket.Bytes != null)
            {
                foreach (byte b in ipPacket.Bytes)
                    ipPacketBytesL.Content += b.ToString();
            }
            tcpPacketBytesL.Content = "Байты TCP пакета:";
            dataRTB.Document = new();
            dataRTB.AppendText("Байты TCP пакета:");
            if (ipPacket.Bytes != null)
            {
                foreach (byte b in tcpPacket.Bytes)
                    dataRTB.AppendText(b.ToString() + " ");
            }

            // данные пакета
            string uF = "";
            for (int i = 20; i < tcpPacket.Bytes.Length; i++)
                uF += tcpPacket.Bytes[i] + " ";
            UseFullL.Content = uF;


        }

        private void filtBut_Click(object sender, RoutedEventArgs e)
        {
            ProcessPort? myProcess = ProcessPorts.ProcessPortMap.Find(proc => proc.ProcessName == procNameTB.Text);
            if (myProcess != null)
            {
                port = myProcess.PortNumber;
                PacketsList.Items.Clear();
            }

        }
    }
    public static class ProcessPorts
    {
        /// <summary>
        /// A list of ProcesesPorts that contain the mapping of processes and the ports that the process uses.
        /// </summary>
        public static List<ProcessPort> ProcessPortMap
        {
            get
            {
                return GetNetStatPorts();
            }
        }


        /// <summary>
        /// This method distills the output from netstat -a -n -o into a list of ProcessPorts that provide a mapping between
        /// the process (name and id) and the ports that the process is using.
        /// </summary>
        /// <returns></returns>
        private static List<ProcessPort> GetNetStatPorts()
        {
            List<ProcessPort> ProcessPorts = new();

            try
            {
                using Process Proc = new();

                ProcessStartInfo StartInfo = new();
                StartInfo.FileName = "netstat.exe";
                StartInfo.Arguments = "-a -n -o";
                StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                StartInfo.UseShellExecute = false;
                StartInfo.RedirectStandardInput = true;
                StartInfo.RedirectStandardOutput = true;
                StartInfo.RedirectStandardError = true;

                Proc.StartInfo = StartInfo;
                Proc.Start();

                StreamReader StandardOutput = Proc.StandardOutput;
                StreamReader StandardError = Proc.StandardError;

                string NetStatContent = StandardOutput.ReadToEnd() + StandardError.ReadToEnd();
                string NetStatExitStatus = Proc.ExitCode.ToString();

                if (NetStatExitStatus != "0")
                {
                    Console.WriteLine("NetStat command failed.   This may require elevated permissions.");
                }

                string[] NetStatRows = Regex.Split(NetStatContent, "\r\n");

                foreach (string NetStatRow in NetStatRows)
                {
                    string[] Tokens = Regex.Split(NetStatRow, "\\s+");
                    if (Tokens.Length > 4 && (Tokens[1].Equals("UDP") || Tokens[1].Equals("TCP")))
                    {
                        string IpAddress = Regex.Replace(Tokens[2], @"\[(.*?)\]", "1.1.1.1");
                        try
                        {
                            ProcessPorts.Add(new ProcessPort(
                                Tokens[1] == "UDP" ? GetProcessName(Convert.ToInt16(Tokens[4])) : GetProcessName(Convert.ToInt16(Tokens[5])),
                                Tokens[1] == "UDP" ? Convert.ToInt16(Tokens[4]) : Convert.ToInt16(Tokens[5]),
                                IpAddress.Contains("1.1.1.1") ? String.Format("{0}v6", Tokens[1]) : String.Format("{0}v4", Tokens[1]),
                                Convert.ToUInt16(IpAddress.Split(':')[1])
                            ));
                        }
                        catch
                        {
                            Console.WriteLine("Could not convert the following NetStat row to a Process to Port mapping.");
                            Console.WriteLine(NetStatRow);
                        }
                    }
                    else
                    {
                        if (!NetStatRow.Trim().StartsWith("Proto") && !NetStatRow.Trim().StartsWith("Active") && !String.IsNullOrWhiteSpace(NetStatRow))
                        {
                            Console.WriteLine("Unrecognized NetStat row to a Process to Port mapping.");
                            Console.WriteLine(NetStatRow);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return ProcessPorts;
        }

        /// <summary>
        /// Private method that handles pulling the process name (if one exists) from the process id.
        /// </summary>
        /// <param name="ProcessId"></param>
        /// <returns></returns>
        private static string GetProcessName(int ProcessId)
        {
            string procName = "UNKNOWN";

            try
            {
                procName = Process.GetProcessById(ProcessId).ProcessName;
            }
            catch { }

            return procName;
        }
    }

    /// <summary>
    /// A mapping for processes to ports and ports to processes that are being used in the system.
    /// </summary>
    public class ProcessPort
    {
        private string _ProcessName = String.Empty;
        private int _ProcessId = 0;
        private string _Protocol = String.Empty;
        private ushort _PortNumber = 0;

        /// <summary>
        /// Internal constructor to initialize the mapping of process to port.
        /// </summary>
        /// <param name="ProcessName">Name of process to be </param>
        /// <param name="ProcessId"></param>
        /// <param name="Protocol"></param>
        /// <param name="PortNumber"></param>
        internal ProcessPort(string ProcessName, int ProcessId, string Protocol, ushort PortNumber)
        {
            _ProcessName = ProcessName;
            _ProcessId = ProcessId;
            _Protocol = Protocol;
            _PortNumber = PortNumber;
        }

        public string ProcessPortDescription
        {
            get
            {
                return String.Format("{0} ({1} port {2} pid {3})", _ProcessName, _Protocol, _PortNumber, _ProcessId);
            }
        }
        public string ProcessName
        {
            get { return _ProcessName; }
        }
        public int ProcessId
        {
            get { return _ProcessId; }
        }
        public string Protocol
        {
            get { return _Protocol; }
        }
        public ushort PortNumber
        {
            get { return _PortNumber; }
        }
    }
}
