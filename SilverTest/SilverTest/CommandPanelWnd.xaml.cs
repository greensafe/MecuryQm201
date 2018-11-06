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
        CommandPanlStatus comstatus = CommandPanlStatus.Idle;

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

        //返回上一级菜单
        private void returnparent(object sender, RoutedEventArgs e)
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

        private void EnableUI()
        {
            switch (comstatus)
            {
                case CommandPanlStatus.Idle:
                case CommandPanlStatus.Air_Sample_Finished:
                case CommandPanlStatus.Air_Test_Finished:
                case CommandPanlStatus.ContinueTest_Finished:
                case CommandPanlStatus.Liquid_Testing_Finished:
                    pm1.IsEnabled = true;               //气体测量
                    m11.IsEnabled = true;               //测量-取样
                    m12.IsEnabled = true;               //测量-清洗
                    m13.IsEnabled = true;               //测量-测量
                    m14.IsEnabled = true;               //测量-返回

                    pm2.IsEnabled = true;               //连续测量
                    pm3.IsEnabled = true;               //校准
                    pm4.IsEnabled = true;               //通信

                    pm5.IsEnabled = true;               //液体测量
                    m51.IsEnabled = true;               //液体测量-清洗
                    m52.IsEnabled = true;               //液体测量-测量
                    m53.IsEnabled = true;               //液体测量-返回

                    pm6.IsEnabled = true;               //参数设置
                    pm7.IsEnabled = true;               //状态获取
                    break;
                case CommandPanlStatus.Air_Sample_Doing:
                    pm1.IsEnabled = true;               //气体测量
                    m11.IsEnabled = true;               //测量-取样
                    m12.IsEnabled = false;               //测量-清洗
                    m13.IsEnabled = false;               //测量-测量
                    m14.IsEnabled = false;               //测量-返回

                    pm2.IsEnabled = false;               //连续测量
                    pm3.IsEnabled = false;               //校准
                    pm4.IsEnabled = false;               //通信

                    pm5.IsEnabled = false;               //液体测量
                    m51.IsEnabled = false;               //液体测量-清洗
                    m52.IsEnabled = false;               //液体测量-测量
                    m53.IsEnabled = false;               //液体测量-返回

                    pm6.IsEnabled = false;               //参数设置
                    pm7.IsEnabled = false;               //状态获取
                    break;
                case CommandPanlStatus.Air_Test_Doing:
                    pm1.IsEnabled = true;               //气体测量
                    m11.IsEnabled = false;               //测量-取样
                    m12.IsEnabled = false;               //测量-清洗
                    m13.IsEnabled = true;               //测量-测量
                    m14.IsEnabled = false;               //测量-返回

                    pm2.IsEnabled = false;               //连续测量
                    pm3.IsEnabled = false;               //校准
                    pm4.IsEnabled = false;               //通信

                    pm5.IsEnabled = false;               //液体测量
                    m51.IsEnabled = false;               //液体测量-清洗
                    m52.IsEnabled = false;               //液体测量-测量
                    m53.IsEnabled = false;               //液体测量-返回

                    pm6.IsEnabled = false;               //参数设置
                    pm7.IsEnabled = false;               //状态获取
                    break;
                case CommandPanlStatus.ContinueTest_Doing:
                    pm1.IsEnabled = false;               //气体测量
                    m11.IsEnabled = false;               //测量-取样
                    m12.IsEnabled = false;               //测量-清洗
                    m13.IsEnabled = false;               //测量-测量
                    m14.IsEnabled = false;               //测量-返回

                    pm2.IsEnabled = true;               //连续测量
                    pm3.IsEnabled = false;               //校准
                    pm4.IsEnabled = false;               //通信

                    pm5.IsEnabled = false;               //液体测量
                    m51.IsEnabled = false;               //液体测量-清洗
                    m52.IsEnabled = false;               //液体测量-测量
                    m53.IsEnabled = false;               //液体测量-返回

                    pm6.IsEnabled = false;               //参数设置
                    pm7.IsEnabled = false;               //状态获取
                    break;
                case CommandPanlStatus.Liquid_Testing_Doing:
                    pm1.IsEnabled = false;               //气体测量
                    m11.IsEnabled = false;               //测量-取样
                    m12.IsEnabled = false;               //测量-清洗
                    m13.IsEnabled = false;               //测量-测量
                    m14.IsEnabled = false;               //测量-返回

                    pm2.IsEnabled = false;               //连续测量
                    pm3.IsEnabled = false;               //校准
                    pm4.IsEnabled = false;               //通信

                    pm5.IsEnabled = true;               //液体测量
                    m51.IsEnabled = false;               //液体测量-清洗
                    m52.IsEnabled = true;               //液体测量-测量
                    m53.IsEnabled = false;               //液体测量-返回

                    pm6.IsEnabled = false;               //参数设置
                    pm7.IsEnabled = false;               //状态获取
                    break;
                default:    //保持不变
                    break;
            }
        }

        private void setParam()
        {
            //参数设置
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
                        comstatus = CommandPanlStatus.ParamSet_Finished;
                        EnableUI();
                        break;
                    //参数设置-流量
                    case 0x12:
                        if (result == 0)
                            statustxt.Text = "设置流量失败";
                        else
                            statustxt.Text = "设置流量成功";
                        comstatus = CommandPanlStatus.ParamSet_Finished;
                        EnableUI();
                        break;
                    //参数设置-清洗时长
                    case 0x13:
                        if (result == 0)
                            statustxt.Text = "设置清洗时长失败";
                        else
                            statustxt.Text = "设置清洗时长成功";
                        comstatus = CommandPanlStatus.ParamSet_Finished;
                        EnableUI();
                        break;
                    //参数设置-高压
                    case 0x14:
                        if (result == 0)
                            statustxt.Text = "设置高压失败";
                        else
                            statustxt.Text = "设置高压成功";
                        comstatus = CommandPanlStatus.ParamSet_Finished;
                        EnableUI();
                        break;
                    case 0x15: //参数设置-发大倍数
                        if (result == 0)
                            statustxt.Text = "设置放大倍数失败";
                        else
                            statustxt.Text = "设置放大倍数成功";
                        comstatus = CommandPanlStatus.ParamSet_Finished;
                        EnableUI();
                        break;
                    case 0x16: //参数设置-实时
                        if (result == 0)
                            statustxt.Text = "设置实时参数失败";
                        else
                            statustxt.Text = "设置实时参数成功";
                        comstatus = CommandPanlStatus.ParamSet_Finished;
                        EnableUI();
                        break;
                    case 0x21: //测量-取样
                        if (result == 0)
                            statustxt.Text = "取样命令执行失败";
                        else
                            statustxt.Text = "取样命令执行成功";
                        comstatus = CommandPanlStatus.Air_Sample_Finished;
                        EnableUI();
                        break;
                    case 0x22: //测量-清洗
                        if (result == 0)
                            statustxt.Text = "清洗命令执行失败";
                        else
                            statustxt.Text = "清洗命令执行成功";
                        comstatus = CommandPanlStatus.Air_Wash_Finished;
                        EnableUI();
                        break;
                    case 0x23: //测量-测量
                        if (result == 0)
                            statustxt.Text = "测量命令执行失败";
                        else
                            statustxt.Text = "测量命令执行成功";
                        comstatus = CommandPanlStatus.Air_Test_Finished;
                        EnableUI();
                        break;
                    case 0x30: //连续测量
                        if (result == 0)
                            statustxt.Text = "连续测试命令执行失败";
                        else
                            statustxt.Text = "连续测试命令执行成功";
                        comstatus = CommandPanlStatus.ContinueTest_Finished;
                        EnableUI();
                        break;
                    case 0x40: //校准
                        if (result == 0)
                            statustxt.Text = "校准命令执行失败";
                        else
                            statustxt.Text = "校转命令执行成功";
                        comstatus = CommandPanlStatus.Adjust_Finished;
                        EnableUI();
                        break;
                    case 0x50: //通信
                        if (result == 0)
                            statustxt.Text = "通信命令执行失败";
                        else
                            statustxt.Text = "同行命令执行成功";
                        comstatus = CommandPanlStatus.Communicate_Finished;
                        EnableUI();
                        break;
                    case 0x61: //液体测量-清洗
                        if (result == 0)
                            statustxt.Text = "液体清洗命令执行失败";
                        else
                            statustxt.Text = "液体清洗命令执行成功";
                        comstatus = CommandPanlStatus.Liquid_Wash_Finished;
                        EnableUI();
                        break;
                    case 0x62: //液体测量-测量
                        if (result == 0)
                            statustxt.Text = "液体测量命令执行失败";
                        else
                            statustxt.Text = "液体测量命令执行成功";
                        comstatus = CommandPanlStatus.Liquid_Testing_Finished;
                        EnableUI();
                        break;
                    case 0x70: //状态获取
                        if (result == 0)
                            statustxt.Text = "获取状态命令执行失败";
                        else
                            statustxt.Text = "获取状态命令执行成功";
                        comstatus = CommandPanlStatus.GetStatus_Finished;
                        EnableUI();
                        //显示参数
                        if (myparams != null)
                        {
                            //todo
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

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void pm1_MouseEnter(object sender, MouseEventArgs e)
        {
            m11.Visibility = Visibility.Visible;
            m12.Visibility = Visibility.Visible;
            m13.Visibility = Visibility.Visible;
            m14.Visibility = Visibility.Visible;
            m51.Visibility = Visibility.Collapsed;
            m52.Visibility = Visibility.Collapsed;
        }

        private void pm5_MouseEnter(object sender, MouseEventArgs e)
        {
            m11.Visibility = Visibility.Collapsed;
            m12.Visibility = Visibility.Collapsed;
            m13.Visibility = Visibility.Collapsed;
            m14.Visibility = Visibility.Collapsed;

            m51.Visibility = Visibility.Visible;
            m52.Visibility = Visibility.Visible;
        }

        private void buttoncontainerstp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            m11.Visibility = Visibility.Collapsed;
            m12.Visibility = Visibility.Collapsed;
            m13.Visibility = Visibility.Collapsed;
            m14.Visibility = Visibility.Collapsed;

            m51.Visibility = Visibility.Collapsed;
            m52.Visibility = Visibility.Collapsed;
        }

        private void m11_Click(object sender, RoutedEventArgs e)
        {
            //测量-采样
            //donn't repeat send
            if (comstatus == CommandPanlStatus.Air_Sample_Waiting || comstatus == CommandPanlStatus.Air_Sample_Doing)
                return;

            byte[] data = new byte[8] { 0x01, 0x01, 0x02, 0x01, 0x00, 0x00, 0, 0 };

            ushort crc = Utility.CRC16(data, 6);
            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "采样命令已发出";
                comstatus = CommandPanlStatus.Air_Sample_Waiting;
            }
            else
            {
                MessageBox.Show("端口未打开");
            }
        }

        private void m13_Click(object sender, RoutedEventArgs e)
        {
            //测量-测量
            if (comstatus == CommandPanlStatus.Air_Test_Doing || comstatus == CommandPanlStatus.Air_Test_Waiting)
                return;   //donn't repeat send command
            byte[] data = new byte[8] { 0x01, 0x01, 0x02, 0x03, 0x00, 0x00, 0, 0 };

            ushort crc = Utility.CRC16(data, 6);
            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "测试命令已发出";
                comstatus = CommandPanlStatus.Air_Test_Waiting;
            }
            else
            {
                MessageBox.Show("端口未打开");
            };
        }

        private void pm3_Click(object sender, RoutedEventArgs e)
        {
            //校准
            if (comstatus == CommandPanlStatus.Adjust_Wating)
                return; //donn't repeat send command
            byte[] data = new byte[8] { 0x01, 0x01, 0x04, 0x00, 0x00, 0x00, 0, 0 };

            ushort crc = Utility.CRC16(data, 6);
            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "校准命令已发出";
                comstatus = CommandPanlStatus.Adjust_Wating;
            }
            else
            {
                MessageBox.Show("端口未打开");
            };
        }

        private void pm4_Click(object sender, RoutedEventArgs e)
        {
            //通信
            if (comstatus == CommandPanlStatus.Communicate_Waiting)
                return; //donn't repeat send
            byte[] data = new byte[8] { 0x01, 0x01, 0x05, 0x00, 0x00, 0x00, 0, 0 };

            ushort crc = Utility.CRC16(data, 6);
            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "通信命令已发出";
                comstatus = CommandPanlStatus.Communicate_Waiting;
            }
            else
            {
                MessageBox.Show("端口未打开");
            };
        }

        private void m51_Click(object sender, RoutedEventArgs e)
        {
            //液体清洗命令
            if (comstatus == CommandPanlStatus.Liquid_Wash_Doing || comstatus == CommandPanlStatus.Liquid_Wash_Waiting)
                return; //donn't repeat send command
            byte[] data = new byte[8] { 0x01, 0x01, 0x06, 0x01, 0x00, 0x00, 0, 0 };

            ushort crc = Utility.CRC16(data, 6);

            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "液体清洗命令已发出";
                comstatus = CommandPanlStatus.Liquid_Wash_Waiting;
            }
            else
            {
                MessageBox.Show("端口未打开");
            };
        }

        private void pm7_Click(object sender, RoutedEventArgs e)
        {
            //获取状态命令
            //也许多次重发是件好事
            byte[] data = new byte[8] { 0x01, 0x01, 0x07, 0x00, 0x00, 0x00, 0, 0 };

            ushort crc = Utility.CRC16(data, 6);
            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "获取状态命令已发出";
                comstatus = CommandPanlStatus.GetStatus_Waiting;
            }
            else
            {
                MessageBox.Show("端口未打开");
            }
        }

        private void m12_Click(object sender, RoutedEventArgs e)
        {
            //测量-清洗
            if (comstatus == CommandPanlStatus.Air_Wash_Doing || comstatus == CommandPanlStatus.Air_Wash_Waiting)
                return; //donn't repeat send
            byte[] data = new byte[8] { 0x01, 0x01, 0x02, 0x02, 0x00, 0x00, 0, 0 };

            ushort crc = Utility.CRC16(data, 6);
            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "清洗命令已发出";
                comstatus = CommandPanlStatus.Air_Wash_Waiting;
            }
            else
            {
                MessageBox.Show("端口未打开");
            };
        }

        private void m52_Click(object sender, RoutedEventArgs e)
        {
            //液体测量
            if (comstatus == CommandPanlStatus.Liquid_Wash_Doing || comstatus == CommandPanlStatus.Liquid_Wash_Waiting)
                return; //donn't repeat send
            byte[] data = new byte[8] { 0x01, 0x01, 0x06, 0x02, 0x00, 0x00, 0, 0 };

            ushort crc = Utility.CRC16(data, 6);

            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "液体测量命令已发出";
                comstatus = CommandPanlStatus.Liquid_Testing_Waiting;
            }
            else
            {
                MessageBox.Show("端口未打开");
            };
        }

        private void pm6_Click(object sender, RoutedEventArgs e)
        {
            parampanel.Visibility = Visibility.Visible;
        }

        private void setparamcancel_Click(object sender, RoutedEventArgs e)
        {
            parampanel.Visibility = Visibility.Collapsed;
        }

        private void setparamok_Click(object sender, RoutedEventArgs e)
        {
            if (comstatus == CommandPanlStatus.ParamSet_Waiting)
                return; //donn't repeat setting
            setParam();
            comstatus = CommandPanlStatus.ParamSet_Waiting;
        }

        private void pm1_Click(object sender, RoutedEventArgs e)
        {
            //气体测量父菜单
            if(m11.Visibility == Visibility.Collapsed)
            {
                m11.Visibility = Visibility.Visible;
                m12.Visibility = Visibility.Visible;
                m13.Visibility = Visibility.Visible;
                m14.Visibility = Visibility.Visible;
                m51.Visibility = Visibility.Collapsed;
                m52.Visibility = Visibility.Collapsed;
            }
            else
            {
                m11.Visibility = Visibility.Collapsed;
                m12.Visibility = Visibility.Collapsed;
                m13.Visibility = Visibility.Collapsed;
                m14.Visibility = Visibility.Collapsed;
            }
        }

        private void pm5_Click(object sender, RoutedEventArgs e)
        {
            if (m51.Visibility == Visibility.Collapsed)
            {
                m11.Visibility = Visibility.Collapsed;
                m12.Visibility = Visibility.Collapsed;
                m13.Visibility = Visibility.Collapsed;
                m14.Visibility = Visibility.Collapsed;

                m51.Visibility = Visibility.Visible;
                m52.Visibility = Visibility.Visible;
            }
            else
            {
                m51.Visibility = Visibility.Collapsed;
                m52.Visibility = Visibility.Collapsed;
            }
        }

        private void teststatusbtn_Click(object sender, RoutedEventArgs e)
        {
            string s = testbox.Text;
            int i = int.Parse(s);

            comstatus =( CommandPanlStatus )i;
            EnableUI();
        }
    }

    public enum CommandPanlStatus
    {
        Idle = 0,                                   //0
        ParamSet_Waiting,                           //1   
        Air_Sample_Waiting, //等待接受气体取样命令    2
        Air_Sample_Doing,   //气体正在采样 3          3
        Air_Sample_Finished,//气体采样完成 4          4    
        Air_Wash_Waiting,   //等待接受气体清洗命令    5
        Air_Wash_Doing,     //气体正在清洗            6
        Air_Wash_Finished,  //气体清洗完成            7
        Air_Test_Waiting,   //等待接受气体测试命令    8
        Air_Test_Doing,     //气体正在测试            9
        Air_Test_Finished,  //气体测试完成            10
        ContinueTest_Waiting,       //等待接受连续测量命令      11
        ContinueTest_Doing,         //连续测量正常进行          12
        ContinueTest_Finished,      //连续测量完成              13
        Adjust_Wating,              //等待接受校准命令          14
        Adjust_Finished,            //校准完成                  15
        Communicate_Waiting,        //等待接受通信命令          16
        Communicate_Finished,       //通信命令完成              17  
        Liquid_Wash_Waiting,        //等待接受液体清洗命令      18
        Liquid_Wash_Doing,          //液体正在清洗              19
        Liquid_Wash_Finished,       //液体清洗完成              20
        Liquid_Testing_Waiting,     //等待接受测试命令          21
        Liquid_Testing_Doing,       //正在测试液体              22
        Liquid_Testing_Finished,    //液体测试完成              23
        GetStatus_Waiting,          //等待接受获取状态命令      24  
        GetStatus_Finished,         //状态获取完成              25  
        ParamSet_Finished,          //参数设置完成
    }
}
