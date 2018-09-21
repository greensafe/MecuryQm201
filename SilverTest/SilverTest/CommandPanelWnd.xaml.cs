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

        //测量-采样
        private void grabSampleBtn_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = new byte[8] { 0x01,0x01,0x02,0x01,0x00,0x00,0,0 };

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

        //测量-测量
        private void starttestBtn_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = new byte[8] { 0x01, 0x01, 0x02, 0x03, 0x00, 0x00, 0, 0 };

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

        //校准
        private void verifyBtn_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = new byte[8] { 0x01, 0x01, 0x04, 0x00, 0x00, 0x00, 0, 0 };

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

        //通信
        private void communicateBtn_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = new byte[8] { 0x01, 0x01, 0x05, 0x00, 0x00, 0x00, 0, 0 };

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

        //测量-清洗
        private void wash_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = new byte[8] { 0x01, 0x01, 0x02, 0x02, 0x00, 0x00, 0, 0 };

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

        //参数设置
        private void setParamBtn_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = new byte[8] { 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0, 0 };
            ushort crc;

            if (timeParamTxt.Text != null && timeParamTxt.Text!= "")
            {
                data[3] = 0x01; //子菜单
                data[4] = 0x00;  //清空数据高位
                data[5] = byte.Parse(timeParamTxt.Text);
            }
            crc = (ushort)byMarc.Net2.Library.Crc.Crc16.Compute(data, 6);
            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "时间设置命令已发出";
            }

            if (fluParamTxt.Text != null && fluParamTxt.Text != "")
            {
                data[3] = 0x02; //子菜单
                data[4] = 0x00;  //清空数据高位
                data[5] = byte.Parse(fluParamTxt.Text);
            }
            crc = (ushort)byMarc.Net2.Library.Crc.Crc16.Compute(data, 6);
            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "流量设置命令已发出";
            }

            if (presureParamTxt.Text != null && presureParamTxt.Text != "")
            {
                data[3] = 0x04; //子菜单
                int pres = int.Parse(presureParamTxt.Text);
                data[4] = (byte)(pres >> 8);
                data[5] = (byte)pres;
            }
            crc = (ushort)byMarc.Net2.Library.Crc.Crc16.Compute(data, 6);
            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "高压设置命令已发出";
            }

            if (enlargeParamTxt.Text != null && enlargeParamTxt.Text != "")
            {
                data[3] = 0x05; //子菜单
                data[4] = 0x00;  //清空数据高位
                data[5] = byte.Parse(enlargeParamTxt.Text);
            }
            crc = (ushort)byMarc.Net2.Library.Crc.Crc16.Compute(data, 6);
            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "放大倍数设置命令已发出";
            }

            if (washtimeParamTxt.Text != null && washtimeParamTxt.Text != "")
            {
                data[3] = 0x03; //子菜单
                data[4] = 0x00;  //清空数据高位
                data[5] = byte.Parse(washtimeParamTxt.Text);
            }
            crc = (ushort)byMarc.Net2.Library.Crc.Crc16.Compute(data, 6);
            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "清洗时长设置命令已发出";
            }

            if (realtimeParamTxt.Text != null && realtimeParamTxt.Text != "")
            {
                data[3] = 0x06; //子菜单
                data[4] = 0x00;  //清空数据高位
                data[5] = byte.Parse(realtimeParamTxt.Text);
            }
            crc = (ushort)byMarc.Net2.Library.Crc.Crc16.Compute(data, 6);
            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "时间设置命令已发出";
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            byte[] data = new byte[8] { 0x01, 0x01, 0xF0, 0x00, 0x00, 0x00, 0, 0 };

            ushort crc = (ushort)byMarc.Net2.Library.Crc.Crc16.Compute(data, 6);
            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            SerialDriver.GetDriver().Open(SerialDriver.GetDriver().portname,
                SerialDriver.GetDriver().rate,
                SerialDriver.GetDriver().parity,
                SerialDriver.GetDriver().databits,
                SerialDriver.GetDriver().stopbits).Send(data);

            MainWindow parentwindow = (MainWindow)this.Owner;
            parentwindow.showconnectedIcon();
            //todo: show the result
        }

    }
}
