using SilverTest.libs;
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
using System.Windows.Shapes;

namespace SilverTest
{
    /// <summary>
    /// SetPortWnd.xaml 的交互逻辑
    /// </summary>
    public partial class SetPortWnd : Window
    {
        public SetPortWnd()
        {
            InitializeComponent();
        }

        private void dataExitbtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            //初始化data 端口列表
            string[] ports = SerialDriver.GetDriver().GetPortList();
            foreach(string item in ports)
            {
                dataComportCombo.Items.Add(item);
            }
            //初始化报警端口列表
            foreach (string item in ports)
            {
                AlarmComportCombo.Items.Add(item);
            }
        }

        private void dataApplybtn_Click(object sender, RoutedEventArgs e)
        {
            if(dataComportCombo.SelectedValue == null)
                SerialDriver.GetDriver().portname = "COM1";
            else
                SerialDriver.GetDriver().portname = dataComportCombo.SelectedValue as string;

            SerialDriver.GetDriver().databits = int.Parse(dataDatacombo.SelectedValue as string);
            SerialDriver.GetDriver().parity = 0;        //paritycombo.SelectedValue as string;
            SerialDriver.GetDriver().rate = int.Parse(dataSpeedcombo.SelectedValue as string);
            SerialDriver.GetDriver().stopbits = int.Parse(dataStopcombo.SelectedValue as string);
            MessageBox.Show("端口设置成功");
        }

        private void OnAlarmSelected(object sender, RoutedEventArgs e)
        {

        }

        private void OnTestDataSelected(object sender, RoutedEventArgs e)
        {

        }

        private void AlarmApplybtn_Click(object sender, RoutedEventArgs e)
        {
            SerialDriver.GetDriver().alarm_portname = AlarmComportCombo.SelectedValue as string;
            SerialDriver.GetDriver().alarm_databits = int.Parse(AlarmDatacombo.SelectedValue as string);
            SerialDriver.GetDriver().alarm_parity = 0;        //paritycombo.SelectedValue as string;
            SerialDriver.GetDriver().alarm_rate = int.Parse(AlarmSpeedcombo.SelectedValue as string);
            SerialDriver.GetDriver().alarm_stopbits = int.Parse(AlarmStopcombo.SelectedValue as string);
            //尝试打开报警串口
            if (SerialDriver.GetDriver().alarm_isOpen())
            {
                MessageBox.Show("报警串口已经打开,串口号=" + SerialDriver.GetDriver().alarm_portname);
                Console.WriteLine("报警串口已经打开,串口号="+SerialDriver.GetDriver().alarm_portname);
            }
            else
            {
                SerialDriver.GetDriver().alarm_Open(SerialDriver.GetDriver().alarm_portname, SerialDriver.GetDriver().alarm_rate, SerialDriver.GetDriver().alarm_parity
                    ,SerialDriver.GetDriver().alarm_databits, SerialDriver.GetDriver().alarm_stopbits);
                if (SerialDriver.GetDriver().alarm_isOpen())
                {
                    MessageBox.Show("报警串口"+ SerialDriver.GetDriver().alarm_portname + "打开成功");
                }
                else
                {
                    MessageBox.Show("报警串口" + SerialDriver.GetDriver().alarm_portname + "打开失败");
                }
            }
            //only test
        }

        private void AlarmExitbtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void WatchApplybtn_Click(object sender, RoutedEventArgs e)
        {
            double alarmvalue = 0;

            try
            {
                alarmvalue = double.Parse(alarm_value_txt.Text);
                MainWindow pw= this.Owner as MainWindow; 
                pw.AlarmValue = alarmvalue;
            }
            catch
            {
                MessageBox.Show("报警值格式不正确，请重新输入");
            }
            //设置监控时间间隔
            byte[] data = new byte[8] { 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0, 0 };
            ushort crc;

            if (watchspantxt.Text != null && watchspantxt.Text != "")
            {
                data[3] = 0x09; //子菜单
                data[4] = 0x00;  //清空数据高位
                                 //data[5] = byte.Parse(watchspantxt.Text);
                data[5] = (byte)watchspantxt.SelectedIndex;
            }
            crc = Utility.CRC16(data, 6);
            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            /*
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "时间设置命令已发出";
                statustxt_2.Content = "时间设置命令已发出";
                comstatus = CommandPanlStatus.ParamSet_Waiting;
            }
            */
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            switch((this.Owner as MainWindow).moudleid)
            {
                case MainWindow.ModuleID.STANDARD:
                    watchtab.Visibility = Visibility.Collapsed;
                    alarmtabitem.Visibility = Visibility.Collapsed;
                    break;
                case MainWindow.ModuleID.ALARM:
                    watchtab.Visibility = Visibility.Visible;
                    alarmtabitem.Visibility = Visibility.Visible;
                    break;
                case MainWindow.ModuleID.LIQUID:
                    watchtab.Visibility = Visibility.Collapsed;
                    alarmtabitem.Visibility = Visibility.Collapsed;
                    break;
            }
            ;
        }
    }
}
