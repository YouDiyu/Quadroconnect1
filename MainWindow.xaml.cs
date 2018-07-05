using System;
using System.Collections.Generic;
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
using System.IO.Ports;
using KTCQuadroControl;
using SharpDX.XInput;
using System.Threading;

namespace Quadroconnect1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        SerialPort COMPort;
        public delegate void Delstring(string hexstring);
        Delstring Delobject;
        Thread TH_Heartbeats;
        public MainWindow()
        {
            InitializeComponent();
            Delobject = new Delstring(SchickenHexString);
            
        }
        public void ComDataReceived(Object sender, SerialDataReceivedEventArgs e)
        {
            int len = COMPort.BytesToRead;
            Byte[] readbuffer = new byte[len];
            COMPort.Read(readbuffer, 0, len);
            Rec.AppendText(BytesToString(readbuffer));
            //String Recei = COMPort.ReadExisting();
            //Rec.Text = Recei;
        }
        /*private void Button_Click(object sender, RoutedEventArgs e)
        {
            //byte[] uavtt = System.Text.Encoding.Default.GetBytes("3c 23 08 00 e8 b7 75 3f");
            //Default.GetBytes("3c 23 08 00 e8 b7 75 3f");
            string HexStr = "3c 23 08 00 e8 b7 75 3f";
            byte[] uavtt = HexStringToBytes(HexStr);
            HexStr = BytesToString(uavtt);
            //test.Text = HexStr;
            byte[] uavT = { 0x3C, 0x23, 0x08, 0x00, 0xE8, 0xB7, 0x75, 0x3F };
            //byte[] uavT = { 0x3C, 0x22, 0x1D, 0x00, 0xE8, 0xB7, 0x75, 0x3F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE5 };
            byte[] cccc = new byte[1];
            cccc[0] = _crc(uavtt);
            byte[] bytestosend = uavtt.Concat(cccc).ToArray();
            test.Text = BytesToString(bytestosend);
            
        }*/
        private byte _crc(byte[] uavtalk)//CRC8 Checksum berechnen
        {
            int len = uavtalk.Length;
            byte ccrc = 0;
            for (int k = 0; k < len; k++)
            {
                ccrc = _CRC_TABLE[((byte)(ccrc ^ uavtalk[k])) & 0xFF];
            }
            return (ccrc);
        }
        private byte[] HexStringToBytes(string HexString)
        {
            string str;
            str = HexString.Replace(" ","");
            byte[] returnBytes = new byte[str.Length/2];
            for(int i =0; i < returnBytes.Length; i++)
            {
                returnBytes[i] = Convert.ToByte(str.Substring(i * 2, 2), 16);
            }
            return returnBytes;
        }
       private string BytesToString(byte[] bytes)
        {
            string HexString = string.Empty;
            if (bytes!=null)
            {
                StringBuilder StrB = new StringBuilder();
                for(int i = 0; i < bytes.Length; i++)
                {
                    StrB.Append(bytes[i].ToString("X2"));
                    StrB.Append(" ");
                }
                HexString = StrB.ToString();
            }
            return HexString;
        }
        public void SchickenHexString(String hexstring)
        {
            byte[] bytesToSend;
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, Delobject, hexstring);
            }
            else
            {
                byte[] bytesfromString = HexStringToBytes(hexstring);
                byte[] Checksum = new byte[1];
                Checksum[0] = _crc(bytesfromString);
                bytesToSend = bytesfromString.Concat(Checksum).ToArray();
                
                try
                {
                    COMPort.Write(BytesToString(bytesToSend));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                //COMPort.Write(bytesToSend,0,bytesToSend.Length);
                test.Text = BytesToString(bytesToSend);
            }
        }

        private void Port_Click(object sender, RoutedEventArgs e)//Port verbinden und initialisieren
        {
            try
            {
                COMPort = new SerialPort();
                COMPort.PortName = PortCOM.Text;
                COMPort.BaudRate = 57600;
                COMPort.Parity = Parity.None;
                COMPort.DataBits = 8;
                COMPort.StopBits = StopBits.One;
                COMPort.Open();
                if (COMPort.IsOpen)
                {
                    MessageBox.Show("open");
                }
                else
                {
                    MessageBox.Show("error");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("error"+ex.ToString());
            }
            COMPort.DataReceived += new SerialDataReceivedEventHandler(ComDataReceived);
        }

        private void Verbinden_Click(object sender, RoutedEventArgs e)
        {
            Delobject("3C 20 2F 00 0A DC D1 CA 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");
            Delobject("3C 20 2F 00 0A DC D1 CA 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01");
            Delobject("3C 20 2F 00 0A DC D1 CA 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 03");
            Delobject("3c 20 12 00 81 74 10 c4 00 00 41 00 c0 07 64 00 00 00");
            Delobject("3c 20 12 00 5c 98 09 c4 00 00 41 00 d0 07 64 00 00 00");
            TH_Heartbeats = new Thread(Heartbeats);
            TH_Heartbeats.Start();
            Thread.Sleep(100);
            Delobject("3C 20 38 00 80 74 10 C4 00 00 00 00 80 BF 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00");
            Thread.Sleep(100);
            Delobject("3C 20 0E 00 5A 98 09 C4 00 00 00 00 80 3F");
            MessageBox.Show("Verbunden");
        }
        private void Heartbeats()
        {
            while (true)
            {
                Delobject("3C 20 2F 00 0A DC D1 CA 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 03");
                Thread.Sleep(1500);
            }
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            string HexstringToPort;
            HexstringToPort = StringToPort.Text;
            Delobject(HexstringToPort);
        }
        byte[] _CRC_TABLE = {

        0x00, 0x07, 0x0e, 0x09, 0x1c, 0x1b, 0x12, 0x15, 0x38, 0x3f, 0x36, 0x31, 0x24, 0x23, 0x2a, 0x2d,

        0x70, 0x77, 0x7e, 0x79, 0x6c, 0x6b, 0x62, 0x65, 0x48, 0x4f, 0x46, 0x41, 0x54, 0x53, 0x5a, 0x5d,

        0xe0, 0xe7, 0xee, 0xe9, 0xfc, 0xfb, 0xf2, 0xf5, 0xd8, 0xdf, 0xd6, 0xd1, 0xc4, 0xc3, 0xca, 0xcd,

        0x90, 0x97, 0x9e, 0x99, 0x8c, 0x8b, 0x82, 0x85, 0xa8, 0xaf, 0xa6, 0xa1, 0xb4, 0xb3, 0xba, 0xbd,

        0xc7, 0xc0, 0xc9, 0xce, 0xdb, 0xdc, 0xd5, 0xd2, 0xff, 0xf8, 0xf1, 0xf6, 0xe3, 0xe4, 0xed, 0xea,

        0xb7, 0xb0, 0xb9, 0xbe, 0xab, 0xac, 0xa5, 0xa2, 0x8f, 0x88, 0x81, 0x86, 0x93, 0x94, 0x9d, 0x9a,

        0x27, 0x20, 0x29, 0x2e, 0x3b, 0x3c, 0x35, 0x32, 0x1f, 0x18, 0x11, 0x16, 0x03, 0x04, 0x0d, 0x0a,

        0x57, 0x50, 0x59, 0x5e, 0x4b, 0x4c, 0x45, 0x42, 0x6f, 0x68, 0x61, 0x66, 0x73, 0x74, 0x7d, 0x7a,

        0x89, 0x8e, 0x87, 0x80, 0x95, 0x92, 0x9b, 0x9c, 0xb1, 0xb6, 0xbf, 0xb8, 0xad, 0xaa, 0xa3, 0xa4,

        0xf9, 0xfe, 0xf7, 0xf0, 0xe5, 0xe2, 0xeb, 0xec, 0xc1, 0xc6, 0xcf, 0xc8, 0xdd, 0xda, 0xd3, 0xd4,

        0x69, 0x6e, 0x67, 0x60, 0x75, 0x72, 0x7b, 0x7c, 0x51, 0x56, 0x5f, 0x58, 0x4d, 0x4a, 0x43, 0x44,

        0x19, 0x1e, 0x17, 0x10, 0x05, 0x02, 0x0b, 0x0c, 0x21, 0x26, 0x2f, 0x28, 0x3d, 0x3a, 0x33, 0x34,

        0x4e, 0x49, 0x40, 0x47, 0x52, 0x55, 0x5c, 0x5b, 0x76, 0x71, 0x78, 0x7f, 0x6a, 0x6d, 0x64, 0x63,

        0x3e, 0x39, 0x30, 0x37, 0x22, 0x25, 0x2c, 0x2b, 0x06, 0x01, 0x08, 0x0f, 0x1a, 0x1d, 0x14, 0x13,

        0xae, 0xa9, 0xa0, 0xa7, 0xb2, 0xb5, 0xbc, 0xbb, 0x96, 0x91, 0x98, 0x9f, 0x8a, 0x8d, 0x84, 0x83,

        0xde, 0xd9, 0xd0, 0xd7, 0xc2, 0xc5, 0xcc, 0xcb, 0xe6, 0xe1, 0xe8, 0xef, 0xfa, 0xfd, 0xf4, 0xf3

        };
    }
}
