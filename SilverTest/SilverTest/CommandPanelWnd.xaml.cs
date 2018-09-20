using Modbus.Device;
using Modbus.Serial;
using SilverTest.libs;
using System;
using System.Collections.Generic;
using System.IO.Ports;
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
using System.Windows.Shapes;

namespace SilverTest
{
    /// <summary>
    /// CommandPanelWnd.xaml 的交互逻辑
    /// </summary>
    public partial class CommandPanelWnd : Window
    {
        public CommandPanelWnd()
        {
            InitializeComponent();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("hello");
        }

        //命令-开始采样
        private void grabSampleBtn_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = new byte[8] { 0x01,0x03,0x01,0x00,0x00,0x01,0,0 };

            ushort crc = (ushort)byMarc.Net2.Library.Crc.Crc16.Compute(data, 6);
            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "采样命令已发出";
            }
            else
            {
                MessageBox.Show("端口未打开");
            }
        }

        //命令-开始测量
        private void starttestBtn_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = new byte[8] { 0x01, 0x03, 0x02, 0x00, 0x00, 0x01, 0, 0 };

            ushort crc = (ushort)byMarc.Net2.Library.Crc.Crc16.Compute(data, 6);
            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "测试命令已发出";
            }
            else
            {
                MessageBox.Show("端口未打开");
            };
        }

        //命令-校准
        private void verifyBtn_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = new byte[8] { 0x01, 0x03, 0x06, 0x00, 0x00, 0x01, 0, 0 };

            ushort crc = (ushort)byMarc.Net2.Library.Crc.Crc16.Compute(data, 6);
            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "校准命令已发出";
            }
            else
            {
                MessageBox.Show("端口未打开");
            };

        }

        //命令-通信
        private void communicateBtn_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = new byte[8] { 0x01, 0x03, 0x05, 0x00, 0x00, 0x01, 0, 0 };

            ushort crc = (ushort)byMarc.Net2.Library.Crc.Crc16.Compute(data, 6);
            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "通信命令已发出";
            }
            else
            {
                MessageBox.Show("端口未打开");
            };
        }

        //命令-清洗
        private void wash_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = new byte[8] { 0x01, 0x03, 0x03, 0x00, 0x00, 0x01, 0, 0 };

            ushort crc = (ushort)byMarc.Net2.Library.Crc.Crc16.Compute(data, 6);
            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "清洗命令已发出";
            }
            else
            {
                MessageBox.Show("端口未打开");
            };
        }

        //名利-参数设置
        private void setParamBtn_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = new byte[17] { 0x01, 0x10, 0x07, 0x00, 0x00, 0x04, 0x08, 0x07,0x00,0x05,0x06,0x00,0x03,0x06,0x09,0,0 };

            ushort crc = (ushort)byMarc.Net2.Library.Crc.Crc16.Compute(data, 15);
            data[15] = (byte)(crc >> 8);
            data[16] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "参数设置命令已发出";
            }
            else
            {
                MessageBox.Show("端口未打开");
            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            byte[] data = new byte[8] { 0x01, 0x03, 0x08, 0x00, 0x00, 0x08, 0, 0 };

            ushort crc = (ushort)byMarc.Net2.Library.Crc.Crc16.Compute(data, 6);
            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            SerialDriver.GetDriver().Open(SerialDriver.GetDriver().portname,
                SerialDriver.GetDriver().rate,
                SerialDriver.GetDriver().parity,
                SerialDriver.GetDriver().databits,
                SerialDriver.GetDriver().stopbits).Send(data);
            //todo: show the result
        }

    }
}
