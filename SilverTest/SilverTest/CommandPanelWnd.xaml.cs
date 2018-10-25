using SilverTest.libs;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace SilverTest
{
    /// <summary>
    /// CommandPanelWnd.xaml 的交互逻辑
    /// </summary>
    public partial class CommandPanelWnd : Window
    {
        Regex numberreg = new Regex(@"\d*");
        public CommandPanelWnd()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //打开串口
            SerialDriver.GetDriver().Open(SerialDriver.GetDriver().portname,
                SerialDriver.GetDriver().rate,
                SerialDriver.GetDriver().parity,
                SerialDriver.GetDriver().databits,
                SerialDriver.GetDriver().stopbits);

            MainWindow parentwindow = (MainWindow)this.Owner;
            this.Owner = null;
            parentwindow.showconnectedIcon();

            if (SerialDriver.GetDriver().isOpen())
            {
                //获取状态命令
                byte[] data = new byte[8] { 0x01, 0x01, 0x07, 0x00, 0x00, 0x00, 0, 0 };

                ushort crc = Utility.CRC16(data, 6);
                data[6] = (byte)(crc >> 8);
                data[7] = (byte)crc;
                if (SerialDriver.GetDriver().Send(data))
                {
                    statustxt.Text = "获取状态命令已发出";
                }
                else
                {
                    MessageBox.Show("端口未打开");
                }
            }
        }

        //获取状态命令
        private void getstatusbtn_Click(object sender, RoutedEventArgs e)
        {
            //获取状态命令
            byte[] data = new byte[8] { 0x01, 0x01, 0x07, 0x00, 0x00, 0x00, 0, 0 };

            ushort crc = Utility.CRC16(data, 6);
            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "获取状态命令已发出";
            }
            else
            {
                MessageBox.Show("端口未打开");
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("hello");
        }

        //测量-采样
        private void grabSampleBtn_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = new byte[8] { 0x01,0x01,0x02,0x01,0x00,0x00,0,0 };

            ushort crc = Utility.CRC16(data, 6);
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

            ushort crc = Utility.CRC16(data, 6);
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

            ushort crc = Utility.CRC16(data, 6);
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

            ushort crc = Utility.CRC16(data, 6);
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

            ushort crc = Utility.CRC16(data, 6);
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

        //返回上一级菜单
        private void returnparent_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = new byte[8] { 0x01, 0x01, 0x01, 0x06, 0x01, 0x04, 0, 0 };

            ushort crc = Utility.CRC16(data, 6);
            
            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "返回上一级菜单命令已发出";
            }
            else
            {
                MessageBox.Show("端口未打开");
            };
        }

        //液体清洗命令
        private void liquidWashBtn_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = new byte[8] { 0x01, 0x01, 0x06, 0x01, 0x00, 0x00, 0, 0 };

            ushort crc = Utility.CRC16(data, 6);

            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "液体清洗命令已发出";
            }
            else
            {
                MessageBox.Show("端口未打开");
            };
        }

        private void liquidTestBtn_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = new byte[8] { 0x01, 0x01, 0x06, 0x02, 0x00, 0x00, 0, 0 };

            ushort crc = Utility.CRC16(data, 6);

            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "液体测量命令已发出";
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
            crc = Utility.CRC16(data, 6);
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
            crc = Utility.CRC16(data, 6);
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
            crc = Utility.CRC16(data, 6);
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
            crc = Utility.CRC16(data, 6);
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
            crc = Utility.CRC16(data, 6);
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
            crc = Utility.CRC16(data, 6);
            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "时间设置命令已发出";
            }
        }

        private void timeParamTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            timeParamTxt.Text = numberreg.Match(timeParamTxt.Text).Value;
        }

        private void fluParamTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            fluParamTxt.Text = numberreg.Match(fluParamTxt.Text).Value;
        }

        private void enlargeParamTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            enlargeParamTxt.Text = numberreg.Match(enlargeParamTxt.Text).Value;
        }

        private void washtimeParamTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            washtimeParamTxt.Text = numberreg.Match(washtimeParamTxt.Text).Value;
        }

        private void presureParamTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            presureParamTxt.Text = numberreg.Match(presureParamTxt.Text).Value;
        }

        private void realtimeParamTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            realtimeParamTxt.Text = numberreg.Match(realtimeParamTxt.Text).Value;
        }

        //处理下维机的命令回应，在控制面板上显示下维机的状态
        //result 0 - 表示失败， 1 - 表示成功
        //目前仅仅获取状态命令有回应myparams
        public void ShowNormalCmndRes(int command, int result, byte[] myparams)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                switch (command)
                {
                    //参数设置-时间
                    case 0x11:
                        if (result == 0)
                            statustxt.Text = "设置时间失败";
                        else
                            statustxt.Text = "设置时间成功";
                        break;
                    //参数设置-流量
                    case 0x12:
                        if (result == 0)
                            statustxt.Text = "设置流量失败";
                        else
                            statustxt.Text = "设置流量成功";
                        break;
                    //参数设置-清洗时长
                    case 0x13:
                        if (result == 0)
                            statustxt.Text = "设置清洗时长失败";
                        else
                            statustxt.Text = "设置清洗时长成功";
                        break;
                    //参数设置-高压
                    case 0x14:
                        if (result == 0)
                            statustxt.Text = "设置高压失败";
                        else
                            statustxt.Text = "设置高压成功";
                        break;
                    case 0x15: //参数设置-发大倍数
                        if (result == 0)
                            statustxt.Text = "设置放大倍数失败";
                        else
                            statustxt.Text = "设置放大倍数成功";
                        break;
                    case 0x16: //参数设置-实时
                        if (result == 0)
                            statustxt.Text = "设置实时参数失败";
                        else
                            statustxt.Text = "设置实时参数成功";
                        break;
                    case 0x21: //测量-取样
                        if (result == 0)
                            statustxt.Text = "取样命令执行失败";
                        else
                            statustxt.Text = "取样命令执行成功";
                        break;
                    case 0x22: //测量-清洗
                        if (result == 0)
                            statustxt.Text = "清洗命令执行失败";
                        else
                            statustxt.Text = "清洗命令执行成功";
                        break;
                    case 0x23: //测量-测量
                        if (result == 0)
                            statustxt.Text = "测量命令执行失败";
                        else
                            statustxt.Text = "测量命令执行成功";
                        break;
                    case 0x30: //连续测量
                        if (result == 0)
                            statustxt.Text = "连续测试命令执行失败";
                        else
                            statustxt.Text = "连续测试命令执行成功";
                        break;
                    case 0x40: //校准
                        if (result == 0)
                            statustxt.Text = "校准命令执行失败";
                        else
                            statustxt.Text = "校转命令执行成功";
                        break;
                    case 0x50: //通信
                        if (result == 0)
                            statustxt.Text = "通信命令执行失败";
                        else
                            statustxt.Text = "同行命令执行成功";
                        break;
                    case 0x61: //液体测量-清洗
                        if (result == 0)
                            statustxt.Text = "液体清洗命令执行失败";
                        else
                            statustxt.Text = "液体清洗命令执行成功";
                        break;
                    case 0x62: //液体测量-测量
                        if (result == 0)
                            statustxt.Text = "液体测量命令执行失败";
                        else
                            statustxt.Text = "液体测量命令执行成功";
                        break;
                    case 0x70: //状态获取
                        if (result == 0)
                            statustxt.Text = "获取状态命令执行失败";
                        else
                            statustxt.Text = "获取状态命令执行成功";
                        //显示参数
                        if(myparams != null)
                        {
                            ;
                        }
                        break;
                    default:
                        break;
                }
            }));
        }

        private void whidebtn_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
