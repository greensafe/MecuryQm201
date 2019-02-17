using SilverTest.libs;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace SilverTest
{
    /// <summary>
    /// CommandPanelWnd.xaml 的交互逻辑
    /// </summary>
    public partial class CommandPanelWnd : Window
    {
        Regex numberreg = new Regex(@"\d*"); //整数
        Regex minusnumber = new Regex(@"^-\d*"); //负数
        Regex realnumber = new Regex(@"^(\d{1}[.][0-9]*)$"); //小数
        CommandPanlStatus comstatus = CommandPanlStatus.Idle;
        public bool is_watching;
        MainWindow parentwindow;

        //正在返回上级菜单
        //参数设置界面点击"返回时候置为真"
        bool is_returning_top_menu = false;
        bool is_returning_top_menu_for_tunnel = false;

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

            parentwindow = (MainWindow)this.Owner;
            //this.Owner = null;
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

            switch((this.Owner as MainWindow).moudleid)
            {
                case MainWindow.ModuleID.STANDARD:
                    pm8.Visibility = Visibility.Collapsed;  //监控
                    pm9.Visibility = Visibility.Collapsed;  //报警
                    pm5.Visibility = Visibility.Collapsed;  //液体测量
                    break;
                case MainWindow.ModuleID.ALARM:
                    pm8.Visibility = Visibility.Visible;  //监控
                    pm9.Visibility = Visibility.Visible;  //报警
                    pm5.Visibility = Visibility.Collapsed;  //液体测量
                    break;
                case MainWindow.ModuleID.LIQUID:
                    pm8.Visibility = Visibility.Collapsed;  //监控
                    pm9.Visibility = Visibility.Collapsed;  //报警
                    pm5.Visibility = Visibility.Visible;  //液体测量
                    break;
            }
        }

        
        private void returnparent(object sender, RoutedEventArgs e)
        {

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
                    m15.IsEnabled = true;               //测量-保存

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
                    m15.IsEnabled = false;               //测量-保存

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
                    m15.IsEnabled = false;

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
                    m15.IsEnabled = false;

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
                    m15.IsEnabled = false;

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

            if (timeckb.IsChecked == true)
            {
                if (timeParamTxt.Text != null && timeParamTxt.Text != "")
                {
                    data[3] = 0x01; //子菜单
                    data[4] = 0x00;  //清空数据高位
                    //data[5] = byte.Parse(timeParamTxt.Text);
                    data[5] = (byte)timeParamTxt.SelectedIndex;
                }
                crc = Utility.CRC16(data, 6);
                data[6] = (byte)(crc >> 8);
                data[7] = (byte)crc;
                if (SerialDriver.GetDriver().Send(data))
                {
                    statustxt.Text = "时间设置命令已发出";
                    statustxt_2.Content = "时间设置命令已发出";
                    comstatus = CommandPanlStatus.ParamSet_Waiting;
                }
            }

            if (fluentckb.IsChecked == true)
            {
                if (fluParamTxt.Text != null && fluParamTxt.Text != "")
                {
                    data[3] = 0x02; //子菜单
                    data[4] = 0x00;  //清空数据高位
                    //data[5] = byte.Parse(fluParamTxt.Text);
                    data[5] = (byte)fluParamTxt.SelectedIndex;
                }
                crc = Utility.CRC16(data, 6);
                data[6] = (byte)(crc >> 8);
                data[7] = (byte)crc;
                if (SerialDriver.GetDriver().Send(data))
                {
                    statustxt.Text = "流量设置命令已发出";
                    statustxt_2.Content = "流量设置命令已发出";
                    comstatus = CommandPanlStatus.ParamSet_Waiting;
                }
            }

            if (highpressureckb.IsChecked == true)
            {
                if (presureParamTxt.Text != null && presureParamTxt.Text != "")
                {
                    data[3] = 0x04; //子菜单
                    int pres = 0;
                    if (int.TryParse(presureParamTxt.Text,out pres))
                    {

                    };
                    pres *= -1;
                    data[4] = (byte)(pres >> 8);
                    data[5] = (byte)pres;
                }
                crc = Utility.CRC16(data, 6);
                data[6] = (byte)(crc >> 8);
                data[7] = (byte)crc;
                if (SerialDriver.GetDriver().Send(data))
                {
                    statustxt.Text = "高压设置命令已发出";
                    statustxt_2.Content = "高压设置命令已发出";
                    comstatus = CommandPanlStatus.ParamSet_Waiting;
                }
            }

            if (amplifyckb.IsChecked == true)
            {
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
                    statustxt_2.Content = "放大倍数设置命令已发出";
                    comstatus = CommandPanlStatus.ParamSet_Waiting;
                }
            }

            if (washtimeckb.IsChecked == true)
            {
                if (washtimeParamTxt.Text != null && washtimeParamTxt.Text != "")
                {
                    data[3] = 0x03; //子菜单
                    data[4] = 0x00;  //清空数据高位
                    //data[5] = byte.Parse(washtimeParamTxt.Text);
                    data[5] = (byte)washtimeParamTxt.SelectedIndex;
                }
                crc = Utility.CRC16(data, 6);
                data[6] = (byte)(crc >> 8);
                data[7] = (byte)crc;
                if (SerialDriver.GetDriver().Send(data))
                {
                    statustxt.Text = "清洗时长设置命令已发出";
                    statustxt_2.Content = "清洗时长设置命令已发出";
                    comstatus = CommandPanlStatus.ParamSet_Waiting;
                }
            }
            if (maintunckb.IsChecked == true)
            {
                if (maintuntxt.Text != null && maintuntxt.Text != "")
                {
                    data[3] = 0x07; //子菜单
                    data[4] = 0x00;  //清空数据高位
                    //data[5] = byte.Parse(washtimeParamTxt.Text);
                    data[5] = (byte)maintuntxt.SelectedIndex;
                }
                crc = Utility.CRC16(data, 6);
                data[6] = (byte)(crc >> 8);
                data[7] = (byte)crc;
                if (SerialDriver.GetDriver().Send(data))
                {
                    statustxt.Text = "主通道设置命令已发出";
                    statustxt_2.Content = "主通道设置命令已发出";

                }
            }
            if(vicetunckb.IsChecked == true)
            {
                if (vicetuntxt.Text != null && vicetuntxt.Text != "")
                {
                    data[3] = 0x08; //子菜单
                    data[4] = 0x00;  //清空数据高位
                    //data[5] = byte.Parse(washtimeParamTxt.Text);
                    data[5] = (byte)vicetuntxt.SelectedIndex;
                }
                crc = Utility.CRC16(data, 6);
                data[6] = (byte)(crc >> 8);
                data[7] = (byte)crc;
                if (SerialDriver.GetDriver().Send(data))
                {
                    statustxt.Text = "副通道设置命令已发出";
                    statustxt_2.Content = "副通道设置命令已发出";
                }
            }
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
            presureParamTxt.Text = minusnumber.Match(presureParamTxt.Text).Value;
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
                        {
                            statustxt.Text = "设置时间失败";
                            statustxt_2.Content = "设置时间失败";
                        }
                        else
                        {
                            statustxt.Text = "设置时间成功";
                            statustxt_2.Content = "设置时间成功";
                        }
                        comstatus = CommandPanlStatus.ParamSet_Finished;
                        EnableUI();
                        break;
                    //参数设置-流量
                    case 0x12:
                        if (result == 0)
                        {
                            statustxt.Text = "设置流量失败";
                            statustxt_2.Content = "设置流量失败";
                        }
                        else
                        {
                            statustxt.Text = "设置流量成功";
                            statustxt_2.Content = "设置流量成功";
                        }
                        comstatus = CommandPanlStatus.ParamSet_Finished;
                        EnableUI();
                        break;
                    //参数设置-清洗时长
                    case 0x13:
                        if (result == 0)
                        {
                            statustxt.Text = "设置清洗时长失败";
                            statustxt_2.Content = "设置清洗时长失败";
                        }
                        else
                        {
                            statustxt.Text = "设置清洗时长成功";
                            statustxt_2.Content = "设置清洗时长成功";
                        }
                        comstatus = CommandPanlStatus.ParamSet_Finished;
                        EnableUI();
                        break;
                    //参数设置-高压
                    case 0x14:
                        if (result == 0)
                        {
                            statustxt.Text = "设置高压失败";
                            statustxt_2.Content = "设置高压失败";
                        }
                        else
                        {
                            statustxt.Text = "设置高压成功";
                            statustxt_2.Content = "设置高压成功";
                        }
                        comstatus = CommandPanlStatus.ParamSet_Finished;
                        EnableUI();
                        break;
                    case 0x15: //参数设置-发大倍数
                        if (result == 0)
                        {
                            statustxt.Text = "设置放大倍数失败";
                            statustxt_2.Content = "设置放大倍数失败";
                        }
                        else if (result == 1)
                        {
                            statustxt.Text = "正在设置放大倍数";
                            statustxt_2.Content = "正在设置放大倍数";
                        }
                        else if (result == 2)
                        {
                            statustxt.Text = "设置放大倍数成功";
                            statustxt_2.Content = "设置放大倍数成功";
                        }
                        comstatus = CommandPanlStatus.ParamSet_Finished;
                        EnableUI();
                        break;
                    case 0x16: //参数设置-返回上一级
                        if (result == 0)
                        {
                            statustxt.Text = "返回命令被拒绝";
                            statustxt_2.Content = "返回命令被拒绝";
                        }
                        else if (result == 1)
                        {
                            statustxt.Text = "正在归0";
                            statustxt_2.Content = "正在归0";
                        } else if (result == 2)
                        {
                            statustxt.Text = "归0完成";
                            statustxt_2.Content = "归0完成";
                        }
                        comstatus = CommandPanlStatus.ParamSet_Finished;
                        EnableUI();
                        break;
                    case 0x17: //参数设置-主通道值
                        if (result == 0)
                        {
                            statustxt.Text = "设置主通道值失败";
                            statustxt_2.Content = "设置主通道值失败";
                        }
                        else if (result == 1)
                        {
                            statustxt.Text = "正在设置主通道值";
                            statustxt_2.Content = "正在设置主通道值";
                        }
                        else if (result == 2)
                        {
                            statustxt.Text = "设置主通道值成功";
                            statustxt_2.Content = "设置主通道值成功";
                        }
                        comstatus = CommandPanlStatus.ParamSet_Finished;
                        EnableUI();
                        break;
                    case 0x18: //参数设置-副通道值
                        if (result == 0)
                        {
                            statustxt.Text = "设置副通道值失败";
                            statustxt_2.Content = "设置副通道值失败";
                        }
                        else if (result == 1)
                        {
                            statustxt.Text = "正在设置副通道值";
                            statustxt_2.Content = "正在设置副通道值";
                        }
                        else if (result == 2)
                        {
                            statustxt.Text = "设置副通道值成功";
                            statustxt_2.Content = "设置副通道值成功";
                        }
                        comstatus = CommandPanlStatus.ParamSet_Finished;
                        EnableUI();
                        break;
                    case 0x21: //测量-取样
                        if (result == 0)
                        {
                            statustxt.Text = "取样命令执行失败";
                            statustxt_2.Content = "取样命令执行失败";
                        }
                        else if (result == 1)
                        {
                            statustxt.Text = "正在取样...";
                            statustxt_2.Content = "正在取样...";
                            m11.IsEnabled = true;   //测量-采样按钮
                            m12.IsEnabled = false;  //测量-清洗按钮
                            m13.IsEnabled = false;  //测量-测量
                            m14.IsEnabled = false;  //测量-返回上一级菜单
                            m21.IsEnabled = false;  //连续测量-开始
                            m22.IsEnabled = false;  //连续测量-退出
                            m23.IsEnabled = false;   //连续测量-保存
                            pm3.IsEnabled = false;  //校准
                            pm4.IsEnabled = false;  //通信
                            m51.IsEnabled = false;  //液体-清洗
                            m52.IsEnabled = false;  //液体-测量
                            m53.IsEnabled = false;  //液体-返回上一级
                            pm6.IsEnabled = false;  //参数设置
                            pm7.IsEnabled = false;  //状态获取

                        }
                        else if (result == 2)
                        {
                            statustxt.Text = "取样命令执行成功";
                            statustxt_2.Content = "取样命令执行成功";
                            m11.IsEnabled = true;   //测量-采样按钮
                            m12.IsEnabled = true;  //测量-清洗按钮
                            m13.IsEnabled = true;  //测量-测量
                            m14.IsEnabled = true;  //测量-返回上一级菜单
                            m21.IsEnabled = true;  //连续测量-开始
                            m22.IsEnabled = true;  //连续测量-退出
                            m23.IsEnabled = true;  //连续测量-save
                            pm3.IsEnabled = true;  //校准
                            pm4.IsEnabled = true;  //通信
                            m51.IsEnabled = true;  //液体-清洗
                            m52.IsEnabled = true;  //液体-测量
                            m53.IsEnabled = true;  //液体-返回上一级
                            pm6.IsEnabled = true;  //参数设置
                            pm7.IsEnabled = true;  //状态获取
                        }
                        comstatus = CommandPanlStatus.Air_Sample_Finished;
                        EnableUI();
                        break;
                    case 0x22: //测量-清洗
                        if (result == 0)
                        {
                            statustxt.Text = "清洗命令执行失败";
                            statustxt_2.Content = "清洗命令执行失败";
                        }
                        else if(result == 1)
                        {
                            statustxt.Text = "正在清洗";
                            statustxt_2.Content = "正在清洗";
                        }
                        else if(result == 2)
                        {
                            statustxt.Text = "清洗命令执行成功";
                            statustxt_2.Content = "清洗命令执行成功";
                        }
                        comstatus = CommandPanlStatus.Air_Wash_Finished;
                        EnableUI();
                        break;
                    case 0x23: //测量-测量
                        if (result == 0)
                        {
                            statustxt.Text = "测量命令执行失败";
                            statustxt_2.Content = "测量命令执行失败";
                        }
                        else if(result == 1)
                        {
                            statustxt.Text = "正在测量...";
                            statustxt_2.Content = "正在测量...";
                            m11.IsEnabled = false;   //测量-采样按钮
                            m12.IsEnabled = false;  //测量-清洗按钮
                            m13.IsEnabled = true;  //测量-测量
                            m14.IsEnabled = false;  //测量-返回上一级菜单
                            m21.IsEnabled = false;  //连续测量-开始
                            m22.IsEnabled = false;  //连续测量-退出
                            m23.IsEnabled = false; ;  //连续测量-save
                            pm3.IsEnabled = false;  //校准
                            pm4.IsEnabled = false;  //通信
                            m51.IsEnabled = false;  //液体-清洗
                            m52.IsEnabled = false;  //液体-测量
                            m53.IsEnabled = false;  //液体-返回上一级
                            pm6.IsEnabled = false;  //参数设置
                            pm7.IsEnabled = false;  //状态获取

                        }
                        else if(result == 2)
                        {
                            statustxt.Text = "测量完成";
                            statustxt_2.Content = "测量完成";
                            m11.IsEnabled = true;   //测量-采样按钮
                            m12.IsEnabled = true;  //测量-清洗按钮
                            m13.IsEnabled = true;  //测量-测量
                            m14.IsEnabled = true;  //测量-返回上一级菜单
                            m21.IsEnabled = true;  //连续测量-开始
                            m22.IsEnabled = true;  //连续测量-退出
                            m23.IsEnabled = true;  //连续测量-save
                            pm3.IsEnabled = true;  //校准
                            pm4.IsEnabled = true;  //通信
                            m51.IsEnabled = true;  //液体-清洗
                            m52.IsEnabled = true;  //液体-测量
                            m53.IsEnabled = true;  //液体-返回上一级
                            pm6.IsEnabled = true;  //参数设置
                            pm7.IsEnabled = true;  //状态获取
                        }else if(result == 3)   //拒绝保存
                        {
                            statustxt.Text = "拒绝测量保存命令";
                            statustxt_2.Content = "拒绝测量保存命令";
                            m11.IsEnabled = true;   //测量-采样按钮
                            m12.IsEnabled = true;  //测量-清洗按钮
                            m13.IsEnabled = true;  //测量-测量
                            m14.IsEnabled = true;  //测量-返回上一级菜单
                            m21.IsEnabled = true;  //连续测量-开始
                            m22.IsEnabled = true;  //连续测量-退出
                            m23.IsEnabled = true;  //连续测量-save
                            pm3.IsEnabled = true;  //校准
                            pm4.IsEnabled = true;  //通信
                            m51.IsEnabled = true;  //液体-清洗
                            m52.IsEnabled = true;  //液体-测量
                            m53.IsEnabled = true;  //液体-返回上一级
                            pm6.IsEnabled = true;  //参数设置
                            pm7.IsEnabled = true;  //状态获取
                        }else if(result == 4)
                        {
                            statustxt.Text = "接受测量保存命令";
                            statustxt_2.Content = "接受测量保存命令";
                        }else if(result == 5)
                        {
                            statustxt.Text = "测量保存命令执行成功";
                            statustxt_2.Content = "测量保存命令执行成功";
                        }
                        comstatus = CommandPanlStatus.Air_Test_Finished;
                        EnableUI();
                        break;
                    case 0x31: //连续测量-开始
                        if (result == 0)
                        {
                            statustxt.Text = "开始连续测量命令被拒绝";
                            statustxt_2.Content = "开始连续测量命令被拒绝";
                        }
                        else if(result == 1)
                        {
                            statustxt.Text = "连续测量即将开始";
                            statustxt_2.Content = "连续测量即将开始";
                        }
                        else if(result == 3)
                        {
                            statustxt.Text = "进入采样阶段";
                            statustxt_2.Content = "进入采样阶段";
                            m11.IsEnabled = false;   //测量-采样按钮
                            m12.IsEnabled = false;  //测量-清洗按钮
                            m13.IsEnabled = false;  //测量-测量
                            m14.IsEnabled = false;  //测量-返回上一级菜单
                            m21.IsEnabled = false;  //连续测量-开始
                            m22.IsEnabled = true;  //连续测量-退出
                            m23.IsEnabled = true;  //连续测量-save
                            pm3.IsEnabled = false;  //校准
                            pm4.IsEnabled = false;  //通信
                            m51.IsEnabled = false;  //液体-清洗
                            m52.IsEnabled = false;  //液体-测量
                            m53.IsEnabled = false;  //液体-返回上一级
                            pm6.IsEnabled = false;  //参数设置
                            pm7.IsEnabled = false;  //状态获取
                        }
                        else if(result == 4)
                        {
                            statustxt.Text = "采样完成";
                            statustxt_2.Content = "采样完成";
                            m11.IsEnabled = true;   //测量-采样按钮
                            m12.IsEnabled = true;  //测量-清洗按钮
                            m13.IsEnabled = true;  //测量-测量
                            m14.IsEnabled = true;  //测量-返回上一级菜单
                            m21.IsEnabled = true;  //连续测量-开始
                            m22.IsEnabled = true;  //连续测量-退出
                            m23.IsEnabled = true;  //连续测量-save
                            pm3.IsEnabled = true;  //校准
                            pm4.IsEnabled = true;  //通信
                            m51.IsEnabled = true;  //液体-清洗
                            m52.IsEnabled = true;  //液体-测量
                            m53.IsEnabled = true;  //液体-返回上一级
                            pm6.IsEnabled = true;  //参数设置
                            pm7.IsEnabled = true;  //状态获取
                        }
                        else if(result ==5)
                        {
                            statustxt.Text = "进入清洗阶段";
                            statustxt_2.Content = "进入清洗阶段";
                        }
                        else if(result == 6)
                        {
                            statustxt.Text = "清洗完成";
                            statustxt_2.Content = "清洗完成";

                        }
                        else if(result == 7)
                        {
                            statustxt.Text = "进入测量阶段";
                            statustxt_2.Content = "进入测量阶段";
                            m11.IsEnabled = false;   //测量-采样按钮
                            m12.IsEnabled = false;  //测量-清洗按钮
                            m13.IsEnabled = false;  //测量-测量
                            m14.IsEnabled = false;  //测量-返回上一级菜单
                            m21.IsEnabled = false;  //连续测量-开始
                            m22.IsEnabled = true;  //连续测量-退出
                            m23.IsEnabled = true;  //连续测量-save
                            pm3.IsEnabled = false;  //校准
                            pm4.IsEnabled = false;  //通信
                            m51.IsEnabled = false;  //液体-清洗
                            m52.IsEnabled = false;  //液体-测量
                            m53.IsEnabled = false;  //液体-返回上一级
                            pm6.IsEnabled = false;  //参数设置
                            pm7.IsEnabled = false;  //状态获取
                        }
                        else if(result == 8)
                        {
                            statustxt.Text = "测量完成";
                            statustxt_2.Content = "测量完成";
                            m11.IsEnabled = true;   //测量-采样按钮
                            m12.IsEnabled = true;  //测量-清洗按钮
                            m13.IsEnabled = true;  //测量-测量
                            m14.IsEnabled = true;  //测量-返回上一级菜单
                            m21.IsEnabled = true;  //连续测量-开始
                            m22.IsEnabled = true;  //连续测量-退出
                            m23.IsEnabled = true;  //连续测量-save
                            pm3.IsEnabled = true;  //校准
                            pm4.IsEnabled = true;  //通信
                            m51.IsEnabled = true;  //液体-清洗
                            m52.IsEnabled = true;  //液体-测量
                            m53.IsEnabled = true;  //液体-返回上一级
                            pm6.IsEnabled = true;  //参数设置
                            pm7.IsEnabled = true;  //状态获取
                        }
                        else if(result == 10)
                        {
                            statustxt.Text = "拒绝连续测量保存命令";
                            statustxt_2.Content = "拒绝连续测量保存命令";
                        }else if(result == 11)
                        {
                            statustxt.Text = "接受连续测量保存命令";
                            statustxt_2.Content = "接受连续测量保存命令";
                        }else if(result == 12)
                        {
                            statustxt.Text = "连续测量保存命令执行成功";
                            statustxt_2.Content = "连续测量保存命令执行成功";
                        }
                        comstatus = CommandPanlStatus.ContinueTest_Finished;
                        EnableUI();
                        break;
                    case 0x32: //连续测量-停止
                        if (result == 0)
                        {
                            statustxt.Text = "无法停止连续测量";
                            statustxt_2.Content = "无法停止连续测量";
                        }
                        else if (result == 2)
                        {
                            statustxt.Text = "连续测量已停止";
                            statustxt_2.Content = "连续测量已停止";
                            m11.IsEnabled = true;   //测量-采样按钮
                            m12.IsEnabled = true;  //测量-清洗按钮
                            m13.IsEnabled = true;  //测量-测量
                            m14.IsEnabled = true;  //测量-返回上一级菜单
                            m21.IsEnabled = true;  //连续测量-开始
                            m22.IsEnabled = true;  //连续测量-退出
                            m23.IsEnabled = true;  //连续测量-save
                            pm3.IsEnabled = true;  //校准
                            pm4.IsEnabled = true;  //通信
                            m51.IsEnabled = true;  //液体-清洗
                            m52.IsEnabled = true;  //液体-测量
                            m53.IsEnabled = true;  //液体-返回上一级
                            pm6.IsEnabled = true;  //参数设置
                            pm7.IsEnabled = true;  //状态获取
                        }
                        comstatus = CommandPanlStatus.ContinueTest_Finished;
                        EnableUI();
                        break;
                    case 0x40: //校准
                        if (result == 0)
                        {
                            statustxt.Text = "校准命令执行失败";
                            statustxt_2.Content = "校准命令执行失败";
                        }
                        else
                        {
                            statustxt.Text = "校转命令执行成功";
                            statustxt_2.Content = "校转命令执行成功";
                        }
                        comstatus = CommandPanlStatus.Adjust_Finished;
                        EnableUI();
                        break;
                    case 0x50: //通信
                        if (result == 0)
                        {
                            statustxt.Text = "通信命令执行失败";
                            statustxt_2.Content = "通信命令执行失败";
                        }
                        else
                        {
                            statustxt.Text = "同行命令执行成功";
                            statustxt_2.Content = "同行命令执行成功";
                        }
                        comstatus = CommandPanlStatus.Communicate_Finished;
                        EnableUI();
                        break;
                    case 0xc1: //液体测量-清洗
                        if (result == 0)
                        {
                            statustxt.Text = "液体清洗命令执行失败";
                            statustxt_2.Content = "液体清洗命令执行失败";
                        }
                        else
                        {
                            statustxt.Text = "液体清洗命令执行成功";
                            statustxt_2.Content = "液体清洗命令执行成功";

                        }
                        comstatus = CommandPanlStatus.Liquid_Wash_Finished;
                        EnableUI();
                        break;
                    case 0xc2: //液体测量-测量
                        if (result == 0)
                        {
                            statustxt.Text = "液体测量命令执行失败";
                            statustxt_2.Content = "液体测量命令执行失败";
                        }
                        else if(result == 1)
                        {
                            statustxt.Text = "正在测量液体...";
                            statustxt_2.Content = "正在测量液体...";
                            m11.IsEnabled = false;   //测量-采样按钮
                            m12.IsEnabled = false;  //测量-清洗按钮
                            m13.IsEnabled = false;  //测量-测量
                            m14.IsEnabled = false;  //测量-返回上一级菜单
                            m21.IsEnabled = false;  //连续测量-开始
                            m22.IsEnabled = false;  //连续测量-退出
                            m23.IsEnabled = false;  //连续测量-save
                            pm3.IsEnabled = false;  //校准
                            pm4.IsEnabled = false;  //通信
                            m51.IsEnabled = false;  //液体-清洗
                            m52.IsEnabled = false;  //液体-测量
                            m53.IsEnabled = true;  //液体-返回上一级
                            pm6.IsEnabled = false;  //参数设置
                            pm7.IsEnabled = false;  //状态获取
                        }
                        else if(result == 2)
                        {
                            statustxt.Text = "液体测量命令执行成功";
                            statustxt_2.Content = "液体测量命令执行成功";
                            m11.IsEnabled = true;   //测量-采样按钮
                            m12.IsEnabled = true;  //测量-清洗按钮
                            m13.IsEnabled = true;  //测量-测量
                            m14.IsEnabled = true;  //测量-返回上一级菜单
                            m21.IsEnabled = true;  //连续测量-开始
                            m22.IsEnabled = true;  //连续测量-退出
                            m23.IsEnabled = true;  //连续测量-save
                            pm3.IsEnabled = true;  //校准
                            pm4.IsEnabled = true;  //通信
                            m51.IsEnabled = true;  //液体-清洗
                            m52.IsEnabled = true;  //液体-测量
                            m53.IsEnabled = true;  //液体-返回上一级
                            pm6.IsEnabled = true;  //参数设置
                            pm7.IsEnabled = true;  //状态获取
                        }
                        comstatus = CommandPanlStatus.Liquid_Testing_Finished;
                        EnableUI();
                        break;
                    case 0x70: //状态获取
                        if (result == 0)
                        {
                            statustxt.Text = "获取状态命令执行失败";
                            statustxt_2.Content = "获取状态命令执行失败";
                        }
                        else
                        {
                            statustxt.Text = "获取状态命令执行成功";
                            statustxt_2.Content = "获取状态命令执行成功";
                        }
                        comstatus = CommandPanlStatus.GetStatus_Finished;
                        EnableUI();
                        //显示参数
                        if (myparams != null)
                        {
                            //todo
                            ;
                        }
                        break;
                    case 0x81:
                        if (result == 0)
                        {
                            statustxt.Text = "开始监控命令执行失败";
                            statustxt_2.Content = "开始监控命令执行失败";
                        }
                        else
                        {
                            statustxt.Text = "开始监控命令执行成功";
                            statustxt_2.Content = "开始监控命令执行成功";
                        }
                        comstatus = CommandPanlStatus.GetStatus_Finished;
                        EnableUI();
                        //显示参数
                        if (myparams != null)
                        {
                            //todo
                            ;
                        }
                        break;
                    case 0x82:
                        if (result == 0)
                        {
                            statustxt.Text = "停止监控命令执行失败";
                            statustxt_2.Content = "停止监控命令执行失败";
                        }
                        else
                        {
                            statustxt.Text = "停止监控命令执行成功";
                            statustxt_2.Content = "停止监控命令执行成功";
                        }
                        comstatus = CommandPanlStatus.GetStatus_Finished;
                        EnableUI();
                        //显示参数
                        if (myparams != null)
                        {
                            //todo
                            ;
                        }
                        break;
                    case 0x91:// 报警测试
                        if (result == 0)
                        {
                            statustxt.Text = "报警测试命令执行失败";
                            statustxt_2.Content = "报警测试命令执行失败";
                        }
                        else
                        {
                            statustxt.Text = "报警测试命令执行成功";
                            statustxt_2.Content = "报警测试命令执行成功";
                        }
                        comstatus = CommandPanlStatus.GetStatus_Finished;
                        EnableUI();
                        //显示参数
                        if (myparams != null)
                        {
                            //todo
                            ;
                        }
                        break;
                    case 0x92: //停止报警测试
                        if (result == 0)
                        {
                            statustxt.Text = "停止报警测试命令执行失败";
                            statustxt_2.Content = "停止报警测试命令执行失败";
                        }
                        else
                        {
                            statustxt.Text = "停止报警测试命令执行成功";
                            statustxt_2.Content = "停止报警测试命令执行成功";
                        }
                        comstatus = CommandPanlStatus.GetStatus_Finished;
                        EnableUI();
                        //显示参数
                        if (myparams != null)
                        {
                            //todo
                            ;
                        }
                        break;
                    case 0xa1:
                        //快速测量
                        if (result == 0)
                        {
                            statustxt.Text = "快速测量命令执行失败";
                            statustxt_2.Content = "快速测量命令执行失败";
                        }
                        else
                        {
                            statustxt.Text = "快速测量命令执行成功";
                            statustxt_2.Content = "快速测量命令执行成功";
                        }
                        comstatus = CommandPanlStatus.GetStatus_Finished;
                        EnableUI();
                        //显示参数
                        if (myparams != null)
                        {
                            //todo
                            ;
                        }
                        break;
                    case 0xa2:
                        //停止快速测量
                        if (result == 0)
                        {
                            statustxt.Text = "停止快速测量命令执行失败";
                            statustxt_2.Content = "停止快速测量命令执行失败";
                        }
                        else
                        {
                            statustxt.Text = "停止快速测量命令执行成功";
                            statustxt_2.Content = "停止快速测量命令执行成功";
                        }
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

        //下维机获取状态回应命令
        //result 0 - 表示失败 ，1 - 接受命令, 2 - 表示成功. res_datas中存放的，比如31
        public void ShowGetStatusRes(int result, byte[] res_datas)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                switch (result)
                {
                    case 0:
                        statustxt.Text = "获取状态失败";
                        statustxt_2.Content = "获取状态失败";
                        break;
                    case 1:
                    case 2:
                        statustxt.Text = "获取状态成功";
                        statustxt_2.Content = "获取状态成功";
                        //主道值
                        maintunvalue_txt.Text = (res_datas[4]-0x30).ToString();
                        //副道值
                        viceunvalue_txt.Text = (res_datas[5]-0x30).ToString();
                        //取样时间
                        sampletime_txt.Text = Utility.MapSampleTime((res_datas[0]-0x30)*10+(res_datas[1]-0x30)).ToString()+"m";
                        //取样流量
                        samplefluent_txt.Text = Utility.MapSampleFluent(res_datas[2] - 0x30).ToString()+"L/m";
                        //清洗时长
                        washtime_txt.Text = Utility.MapWashTime(res_datas[3] - 0x30);
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
            /*
            m11.Visibility = Visibility.Visible;
            m12.Visibility = Visibility.Visible;
            m13.Visibility = Visibility.Visible;
            m14.Visibility = Visibility.Visible;
            m51.Visibility = Visibility.Collapsed;
            m52.Visibility = Visibility.Collapsed;
            */
        }

        private void pm5_MouseEnter(object sender, MouseEventArgs e)
        {
            /*
            m11.Visibility = Visibility.Collapsed;
            m12.Visibility = Visibility.Collapsed;
            m13.Visibility = Visibility.Collapsed;
            m14.Visibility = Visibility.Collapsed;

            m51.Visibility = Visibility.Visible;
            m52.Visibility = Visibility.Visible;
            */
        }

        private void buttoncontainerstp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            m51.Visibility = Visibility.Collapsed;
            m52.Visibility = Visibility.Collapsed;

            m11.Visibility = Visibility.Collapsed;
            m12.Visibility = Visibility.Collapsed;
            m13.Visibility = Visibility.Collapsed;
            m14.Visibility = Visibility.Collapsed;
            m15.Visibility = Visibility.Collapsed;

            m21.Visibility = Visibility.Collapsed;
            m22.Visibility = Visibility.Collapsed;
            m23.Visibility = Visibility.Collapsed;

            ma1.Visibility = Visibility.Collapsed;
            ma2.Visibility = Visibility.Collapsed;

            m81.Visibility = Visibility.Collapsed;
            m82.Visibility = Visibility.Collapsed;

            m91.Visibility = Visibility.Collapsed;
            m92.Visibility = Visibility.Collapsed;
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
                statustxt_2.Content = "采样命令已发出";
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
            if (comstatus == CommandPanlStatus.Air_Test_Doing || comstatus == CommandPanlStatus.Air_Test_Waiting ||
                !parentwindow.IsSelected())
                return;   //donn't repeat send command
            byte[] data = new byte[8] { 0x01, 0x01, 0x02, 0x03, 0x00, 0x00, 0, 0 };

            ushort crc = Utility.CRC16(data, 6);
            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "测试命令已发出";
                statustxt_2.Content = "测试命令已发出";
                comstatus = CommandPanlStatus.Air_Test_Waiting;

                parentwindow.StartTest();
                parentwindow.testingtype = MainWindow.TestingType.SINGLE;
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
                statustxt_2.Content = "校准命令已发出";
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
                statustxt_2.Content = "通信命令已发出";
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
            byte[] data = new byte[8] { 0x01, 0x01, 0x0c, 0x01, 0x00, 0x00, 0, 0 };

            ushort crc = Utility.CRC16(data, 6);

            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "液体清洗命令已发出";
                statustxt_2.Content = "液体清洗命令已发出";
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
                statustxt_2.Content = "获取状态命令已发出";
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
                statustxt_2.Content = "清洗命令已发出";
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
            byte[] data = new byte[8] { 0x01, 0x01, 0x0c, 0x02, 0x00, 0x00, 0, 0 };

            ushort crc = Utility.CRC16(data, 6);

            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "液体测量命令已发出";
                statustxt_2.Content = "液体测量命令已发出";
                comstatus = CommandPanlStatus.Liquid_Testing_Waiting;
            }
            else
            {
                MessageBox.Show("端口未打开");
            };
        }

        private void pm6_Click(object sender, RoutedEventArgs e)
        {
            if(parampanel.Visibility ==  Visibility.Collapsed)
                parampanel.Visibility = Visibility.Visible;
            else
                parampanel.Visibility = Visibility.Collapsed;
        }

        private void setparamcancel_Click(object sender, RoutedEventArgs e)
        {
 
            //启动定时器，发出返回命令
            if(is_returning_top_menu == false)
            {
                //禁用UI
                setparamcancel.IsEnabled = false;
                setparamok.IsEnabled = false;
                //开启timer，发送三次推出命令
                DispatcherTimer atimer = new DispatcherTimer();
                atimer.Interval = new TimeSpan(0, 0, 0, 0,100);
                atimer.Tag = 0;
                atimer.Tick += new EventHandler(returnTimerHdlr);
                atimer.Start();
            }
        }

        private void returnTimerHdlr(object sender, EventArgs e)
        {
            DispatcherTimer atimer = (sender as DispatcherTimer);
            
            //退出参数设置界面
            byte[] data = new byte[8] { 0x01, 0x01, 0x01, 0x06, 0x00, 0x00, 0, 0 };
            ushort crc;

            crc = Utility.CRC16(data, 6);
            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "返回命令已发出";
                statustxt_2.Content = "返回命令已发出";
            }
            Console.WriteLine("返回上级菜单命令发出一次");

            atimer.Tag = (int)atimer.Tag + 1;
            if((int)atimer.Tag == 3)
            {
                atimer.Stop();
                atimer = null;
                is_returning_top_menu = false;

                setparamcancel.IsEnabled = true;
                setparamok.IsEnabled = true;
                parampanel.Visibility = Visibility.Collapsed;
            }
        }

        private void setparamok_Click(object sender, RoutedEventArgs e)
        {
            /*
            if (comstatus == CommandPanlStatus.ParamSet_Waiting)
            {
                //parampanel.Visibility = Visibility.Collapsed;
                return; //donn't repeat setting
            }
            */
            setParam();
            //parampanel.Visibility = Visibility.Collapsed;
        }

        private void pm1_Click(object sender, RoutedEventArgs e)
        {
            //气体测量父菜单
            if(m12.Visibility == Visibility.Collapsed)
            {
                m11.Visibility = Visibility.Collapsed;
                m12.Visibility = Visibility.Visible;
                m13.Visibility = Visibility.Visible;
                m14.Visibility = Visibility.Visible;
                m15.Visibility = Visibility.Visible;

                m51.Visibility = Visibility.Collapsed;
                m52.Visibility = Visibility.Collapsed;
            }
            else
            {
                m12.Visibility = Visibility.Collapsed;
                m12.Visibility = Visibility.Collapsed;
                m13.Visibility = Visibility.Collapsed;
                m14.Visibility = Visibility.Collapsed;
                m15.Visibility = Visibility.Collapsed;
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

        private void m14_Click(object sender, RoutedEventArgs e)
        {
            //气体测量-返回上一级菜单
            byte[] data = new byte[8] { 0x01, 0x01, 0x01, 0x06, 0x00, 0x00, 0, 0 };

            ushort crc = Utility.CRC16(data, 6);

            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "返回上一级菜单命令已发出";
                statustxt_2.Content = "返回上一级菜单命令已发出";
                comstatus = CommandPanlStatus.AirTestReturn_Finished;
            }
            else
            {
                MessageBox.Show("端口未打开");
            };
        }

        private void m53_Click(object sender, RoutedEventArgs e)
        {
            //液体测量-返回上一级菜单
            byte[] data = new byte[8] { 0x01, 0x01, 0x0c, 0x06, 0x00, 0x00, 0, 0 };

            ushort crc = Utility.CRC16(data, 6);

            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "返回上一级菜单命令已发出";
                statustxt_2.Content = "返回上一级菜单命令已发出";
                comstatus = CommandPanlStatus.LiquidTestReturn_Finished;
            }
            else
            {
                MessageBox.Show("端口未打开");
            };
        }

        private void timeckb_Checked(object sender, RoutedEventArgs e)
        {
            timeParamTxt.IsEnabled = true;
        }

        private void timeckb_Unchecked(object sender, RoutedEventArgs e)
        {
            timeParamTxt.IsEnabled = false;
        }

        private void fluentckb_Checked(object sender, RoutedEventArgs e)
        {
            fluParamTxt.IsEnabled = true;
        }

        private void fluentckb_Unchecked(object sender, RoutedEventArgs e)
        {
            fluParamTxt.IsEnabled = false;
        }

        private void amplifyckb_Unchecked(object sender, RoutedEventArgs e)
        {
            enlargeParamTxt.IsEnabled = false;
        }

        private void amplifyckb_Checked(object sender, RoutedEventArgs e)
        {
            enlargeParamTxt.IsEnabled = true;
        }

        private void washtimeckb_Checked(object sender, RoutedEventArgs e)
        {
            washtimeParamTxt.IsEnabled = true;
        }

        private void washtimeckb_Unchecked(object sender, RoutedEventArgs e)
        {
            washtimeParamTxt.IsEnabled = false;
        }

        private void highpressureckb_Unchecked(object sender, RoutedEventArgs e)
        {
            presureParamTxt.IsEnabled = false;
        }

        private void highpressureckb_Checked(object sender, RoutedEventArgs e)
        {
            presureParamTxt.IsEnabled = true;
        }

        private void pm2_Click(object sender, RoutedEventArgs e)
        {
            if (m21.Visibility == Visibility.Collapsed)
            {
                m21.Visibility = Visibility.Visible;
                m22.Visibility = Visibility.Visible;
                m23.Visibility = Visibility.Visible;
            }
            else
            {
                m21.Visibility = Visibility.Collapsed;
                m22.Visibility = Visibility.Collapsed;
                m23.Visibility = Visibility.Collapsed;
            }
        }

        private void m21_Click(object sender, RoutedEventArgs e)
        {
            //连续测量-开始
            byte[] data = new byte[8] { 0x01, 0x01, 0x03, 0x01, 0x00, 0x00, 0, 0 };

            ushort crc = Utility.CRC16(data, 6);

            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "开始连续测量命令已发出";
                statustxt_2.Content = "开始连续测量命令已发出";
                comstatus = CommandPanlStatus.LiquidTestReturn_Finished;
                parentwindow.StartTest();
                parentwindow.testingtype = MainWindow.TestingType.CONTINUE;
            }
            else
            {
                MessageBox.Show("端口未打开");
            };
        }

        private void m22_Click(object sender, RoutedEventArgs e)
        {
            //液体测量-返回上一级菜单
            byte[] data = new byte[8] { 0x01, 0x01, 0x03, 0x02, 0x00, 0x00, 0, 0 };

            ushort crc = Utility.CRC16(data, 6);

            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "停止连续测量命令已发出";
                statustxt_2.Content = "停止连续测量命令已发出";
                comstatus = CommandPanlStatus.LiquidTestReturn_Finished;
            }
            else
            {
                MessageBox.Show("端口未打开");
            };
        }

        private void maintunckb_Unchecked(object sender, RoutedEventArgs e)
        {
            maintuntxt.IsEnabled = false;
        }

        private void maintunckb_Checked(object sender, RoutedEventArgs e)
        {
            maintuntxt.IsEnabled = true;
        }

        private void vicetunckb_Unchecked(object sender, RoutedEventArgs e)
        {
            vicetuntxt.IsEnabled = false;
        }

        private void vicetunckb_Checked(object sender, RoutedEventArgs e)
        {
            vicetuntxt.IsEnabled = true;
        }

        private void Pm9_Click(object sender, RoutedEventArgs e)
        {
            if(m91.Visibility == Visibility.Collapsed)
            {
                m91.Visibility = Visibility.Visible;
                m92.Visibility = Visibility.Visible;
            }
            else
            {
                m91.Visibility = Visibility.Collapsed;
                m92.Visibility = Visibility.Collapsed;
            }
        }

        private void M91_Click(object sender, RoutedEventArgs e)
        {
            //向报警器发出报警命令
            byte[] data = new byte[8] { 0x01, 0x01, 0x09, 0x01, 0x00, 0x00, 0, 0 };

            ushort crc = Utility.CRC16(data, 6);

            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "报警测试命令已发出";
                statustxt_2.Content = "报警测试命令已发出";
            }
            else
            {
                MessageBox.Show("端口未打开");
            };
        }

        private void M92_Click(object sender, RoutedEventArgs e)
        {
            //向报警器发出停止报警命令
            byte[] data = new byte[8] { 0x01, 0x01, 0x09, 0x02, 0x00, 0x00, 0, 0 };

            ushort crc = Utility.CRC16(data, 6);

            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "停止报警测试命令已发出";
                statustxt_2.Content = "停止报警测试命令已发出";
            }
            else
            {
                MessageBox.Show("端口未打开");
            };
        }

        private void M81_Click(object sender, RoutedEventArgs e)
        {
            //通知下维机开始监控
            byte[] data = new byte[8] { 0x01, 0x01, 0x08, 0x01, 0x00, 0x00, 0, 0 };

            ushort crc = Utility.CRC16(data, 6);

            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "开始监控命令已发出";
                statustxt_2.Content = "开始监控命令已发出";
            }
            else
            {
                MessageBox.Show("端口未打开");
            };

            is_watching = true;
        }

        private void M82_Click(object sender, RoutedEventArgs e)
        {
            is_watching = false;
            //通知下维机停止监控
            byte[] data = new byte[8] { 0x01, 0x01, 0x08, 0x02, 0x00, 0x00, 0, 0 };

            ushort crc = Utility.CRC16(data, 6);

            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "停止监控命令已发出";
                statustxt_2.Content = "停止监控命令已发出";
            }
            else
            {
                MessageBox.Show("端口未打开");
            };
        }

        private void Pm8_Click(object sender, RoutedEventArgs e)
        {
            if (m81.Visibility == Visibility.Visible)
            {
                m81.Visibility = Visibility.Collapsed;
                m82.Visibility = Visibility.Collapsed;
            }
            else
            {
                m81.Visibility = Visibility.Visible;
                m82.Visibility = Visibility.Visible;
            }
        }

        private void Pma_Click(object sender, RoutedEventArgs e)
        {
            if(ma1.Visibility == Visibility.Visible)
            {
                ma1.Visibility = Visibility.Collapsed;
                ma2.Visibility = Visibility.Collapsed;
            }
            else
            {
                ma1.Visibility = Visibility.Visible;
                ma2.Visibility = Visibility.Visible;
            }
        }

        private void Ma1_Click(object sender, RoutedEventArgs e)
        {
            //发出快速测量命令
            byte[] data = new byte[8] { 0x01, 0x01, 0x0a, 0x01, 0x00, 0x00, 0, 0 };

            ushort crc = Utility.CRC16(data, 6);

            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "快速测量命令已发出";
                statustxt_2.Content = "快速测量命令已发出";
            }
            else
            {
                MessageBox.Show("端口未打开");
            };
        }

        private void Ma2_Click(object sender, RoutedEventArgs e)
        {
            //发出快速测量命令
            byte[] data = new byte[8] { 0x01, 0x01, 0x0a, 0x02, 0x00, 0x00, 0, 0 };

            ushort crc = Utility.CRC16(data, 6);

            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "停止快速测量命令已发出";
                statustxt_2.Content = "停止快速测量命令已发出";
            }
            else
            {
                MessageBox.Show("端口未打开");
            };
        }

        private void Pmb_Click(object sender, RoutedEventArgs e)
        {
            if (channelsetpanel.Visibility == Visibility.Visible)
                channelsetpanel.Visibility = Visibility.Collapsed;
            else
                channelsetpanel.Visibility = Visibility.Visible;
        }

        private void setchannelclose_Click(object sender, RoutedEventArgs e)
        {
            //返回主菜单
            //启动定时器，发出返回命令
            if (is_returning_top_menu_for_tunnel == false)
            {
                //禁用UI
                setchannelok.IsEnabled = false;
                setchannelclose.IsEnabled = false;
                //开启timer，发送二次推出命令
                DispatcherTimer atimer = new DispatcherTimer();
                atimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
                atimer.Tag = 0;
                atimer.Tick += new EventHandler(ReturnForChannelTimerHdlr);
                atimer.Start();
            }
        }

        private void ReturnForChannelTimerHdlr(object sender, EventArgs e)
        {
            DispatcherTimer atimer = (sender as DispatcherTimer);

            //从能量值调整界面返回
            byte[] data = new byte[8] { 0x01, 0x01, 0x04, 0x06, 0x00, 0x00, 0, 0 };
            ushort crc;

            crc = Utility.CRC16(data, 6);
            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "能量值调整返回命令已发出";
                statustxt_2.Content = "能量值调整返回命令已发出";
            }
            Console.WriteLine("能量值调整界面返回命令发出一次");

            atimer.Tag = (int)atimer.Tag + 1;
            if ((int)atimer.Tag == 2)
            {
                atimer.Stop();
                atimer = null;
                is_returning_top_menu_for_tunnel = false;

                setchannelok.IsEnabled = true;
                setchannelclose.IsEnabled = true;
                channelsetpanel.Visibility = Visibility.Collapsed;
            }
        }

        private void CmdPanelWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            switch ((this.Owner as MainWindow).moudleid)
            {
                case MainWindow.ModuleID.STANDARD:
                    pm8.Visibility = Visibility.Collapsed;  //监控
                    pm9.Visibility = Visibility.Collapsed;  //报警
                    pm5.Visibility = Visibility.Collapsed;  //液体测量
                    break;
                case MainWindow.ModuleID.ALARM:
                    pm8.Visibility = Visibility.Visible;  //监控
                    pm9.Visibility = Visibility.Visible;  //报警
                    pm5.Visibility = Visibility.Collapsed;  //液体测量
                    break;
                case MainWindow.ModuleID.LIQUID:
                    pm8.Visibility = Visibility.Collapsed;  //监控
                    pm9.Visibility = Visibility.Collapsed;  //报警
                    pm5.Visibility = Visibility.Visible;  //液体测量
                    break;
            }
        }

        private void m15_Click(object sender, RoutedEventArgs e)
        {
            //测量-保存
            
            if (comstatus == CommandPanlStatus.AIR_DATA_SAVING)
                return;   //donn't repeat send command
            
            byte[] data = new byte[8] { 0x01, 0x01, 0x02, 0x03, 0x00, 0x01, 0, 0 };

            ushort crc = Utility.CRC16(data, 6);
            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "测量数据保存命令已发出";
                statustxt_2.Content = "测量数据保存命令已发出";
                comstatus = CommandPanlStatus.AIR_DATA_SAVING;
            }
            else
            {
                MessageBox.Show("端口未打开");
            };

        }

        private void m23_Click(object sender, RoutedEventArgs e)
        {
            //连续测量-保存数据
            byte[] data = new byte[8] { 0x01, 0x01, 0x03, 0x01, 0x00, 0x01, 0, 0 };

            ushort crc = Utility.CRC16(data, 6);

            data[6] = (byte)(crc >> 8);
            data[7] = (byte)crc;
            if (SerialDriver.GetDriver().Send(data))
            {
                statustxt.Text = "连续测量保存命令已发出";
                statustxt_2.Content = "连续测量保存命令已发出";
                comstatus = CommandPanlStatus.Idle;
            }
            else
            {
                MessageBox.Show("端口未打开");
            };
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
        AirTestReturn_Waiting,      //气体测量返回上一级菜单   
        AirTestReturn_Finished,     //气体测量返回上一级菜单命令完成
        LiquidTestReturn_Waiting,   //液体测量返回上一级菜单   
        LiquidTestReturn_Finished,  //液体测量返回上一级菜单命令完成
        AIR_DATA_SAVING    ,        //测量数据保存中
    }
}
