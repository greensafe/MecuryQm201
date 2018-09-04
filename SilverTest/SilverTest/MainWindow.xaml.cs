using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Win32;
using SilverTest.libs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using static SilverTest.libs.DataFormater;
using static SilverTest.libs.PhyCombine;

/*
 * 变量尾缀约定
 * Clt = Collection
 * XDP = XmlDataProvider
 * Dgd = DataGrid 
 * Bbx = GroupBox
 * Cmb = ComboBox
 * Txb = TextBox
 * Viw = ICollectionView
 * 
 * 函数尾缀约定
 * Hdlr = handler 事件处理函数
 * 
 * 函数前缀约定
 * On = 事件注册函数
*/

namespace SilverTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>

    //模拟数据
    public struct RowItem
    {
        public string no { get; set; }
        public string sampleName { get; set; }
        public string responseTime { get; set; }
    }

    //样本类型
    public enum SampleType
    {
        LIQUID,
        AIR,
        SOLID
    }

    public partial class MainWindow : Window
    {
        //演示代码
        //int seconds;
        //DispatcherTimer demoTimer = new DispatcherTimer();

        //使用DynamicDataDisplay控件显示波形
        private ObservableDataSource<Point> realCptDs = new ObservableDataSource<Point>();
        //private DispatcherTimer realCptTimer = new DispatcherTimer();
        private int dots_start_abs = 0;
        bool mode = true;
        Random rd = new Random();
        private int currentSecond = 0;
        int xaxis = 0;
        int yaxis = 0;
        int group = 200;//组距
        Queue<int> q = new Queue<int>();
        //配置
        //调整面积积分的积分结果
        readonly double ratio = 1.0000;
        //响应值R1面积积分起始点
        readonly int R1_start = 2000;
        //响应值R1面积积分结束点
        readonly int R1_end = 7000;
        //停止测试点位
        readonly int stop_test_position = 7500;
        //瞬时最大值
        double maxResponse = 0;
        //载入气体温度汞浓度资源
        XmlDocument AirDensityXml = new XmlDocument();
        //样本类型
        SampleType sampleType = SampleType.AIR;

        private SerialPort ComDevice = null;
        private ObservableCollection<NewTestTarget> newTestClt;
        private ObservableCollection<StandardSample> standardSampleClt;

        ICollectionView StandardCvw;

        public MainWindow()
        {
            InitializeComponent();

            //初始化使用DynamicDataDisplay控件
            realCpt.AddLineGraph(realCptDs, Colors.Red, 2, "百分比");
            realCpt.LegendVisible = true;
            realCpt.Viewport.FitToView();
            //realCptTimer.Interval = TimeSpan.FromSeconds(1);
            //realCptTimer.Tick += realTck;
            //realCptTimer.IsEnabled = true;

            //初始化xml文档
            AirDensityXml.Load(@"resources\ChinaAirDensity.xml");
    
            newTestClt =
                    Utility.getNewTestTargetDataFromXml("resources\\NewTestTarget_Table.xml");
            NewTargetDgd.DataContext = newTestClt;
            standardSampleClt =
                Utility.getStandardTargetDataFromXml("resources\\StandardSamples_Table.xml");
            standardSampleDgd.DataContext = standardSampleClt;
            ;
            StandardCvw = CollectionViewSource.GetDefaultView(standardSampleClt);
            StandardCvw.GroupDescriptions.Add(new PropertyGroupDescription("GroupName"));

        }

        private void realTck(object sender, EventArgs e)
        {
       
        }


        private void window_Loaded(object sender, RoutedEventArgs e)
        {


            //加载config数据
            //string s = Utility.GetValueFrXml("QM201H/response/compute", "ratio");
            string rs = Utility.GetValueFrXml("/config/QM201H/response/compute/R1", "pointstart");
            string re = Utility.GetValueFrXml("/config/QM201H/response/compute/R1", "end");

            //初始化RS232驱动，注册回调函数
            ComDevice = new SerialPort();
            ComDevice.DataReceived += new SerialDataReceivedEventHandler(Com_DataReceived);

            //初始化串口数据分析模块
            DotManager.GetDotManger().onPacketCorrected(CorrectedPacketReceived);
            DotManager.GetDotManger().onPacketRecevied(PacketReceived);
            DotManager.GetDotManger().Start();

            //drawWave_simulate(1001);

        }



        public void Com_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            //接受数据
            /*
            byte[] ReDatas = new byte[ComDevice.BytesToRead];
            ComDevice.Read(ReDatas, 0, ReDatas.Length);
            */

            byte[] ReDatas = SerialDriver.GetDriver().Read();
            if (ReDatas == null) return;
            string re = "";
            for (int i = 0; i < ReDatas.Length; i++)
            {
                re += (char)ReDatas[i];
            }
            //Console.WriteLine("serial . received data: " + re);
            DotManager.GetDotManger().GetDot(ReDatas);

            //DataFormater.getDataFormater().GetDot(ReDatas);
        }

        //打开端口
        private bool openRSPort()
        {
            //打开端口
            if (ComDevice.IsOpen == false)
            {
                ComDevice.PortName = "COM1";
                ComDevice.BaudRate = 115200;
                ComDevice.Parity = (Parity)0;
                ComDevice.DataBits = 8;
                ComDevice.StopBits = (StopBits)1;
                try
                {
                    ComDevice.Open();
                    //MessageBox.Show("打开成功");
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "打开发生错误");
                    return false;
                }

            }
            //已经打开
            return true;
        }


        private void closeRSPort()
        {
            if (ComDevice.IsOpen == true) {
                try
                {
                    ComDevice.Close();
                    MessageBox.Show("打开端口已经被关闭");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "无法关闭");
                };
            }
        }

        //发送命令
        private void sendRSCommand(byte[] data)
        {
            if (data == null) return;

            if (!openRSPort())
            {
                return;
            }
            try
            {
                ComDevice.Write(data, 0, data.Length);//发送数据  
            }
            catch (Exception ex)
            {
                MessageBox.Show("发送数据错误");
            }
        }

        private void AddRowBtn_Click(object sender, RoutedEventArgs e)
        {
            int se = 0;
            switch (sampletab.SelectedIndex)
            {
                //新样测试
                case 0:
                    NewTestTarget newitem = new NewTestTarget();
                    //计算序号
                    for (int i = 0; i < newTestClt.Count; i++)
                    {
                        //处理边界情况
                        if (i == newTestClt.Count - 1) {
                            se = int.Parse(newTestClt[i].Code) + 1;
                            break;
                        }
                        //序号连续情况下，跳过连续号码
                        if (int.Parse(newTestClt[i].Code) + 1 == int.Parse(newTestClt[i + 1].Code))
                            continue;
                        //找到一个可能的空洞
                        se = int.Parse(newTestClt[i].Code + 1);
                        /*
                        bool found = true;
                        //看下剩下的数组是否被占用
                        for(int j = i + 1; j < newTestClt.Count; i++)
                        {
                            if( int.Parse(newTestClt[i].Code) == int.Parse(newTestClt[j].Code))
                            {
                                //被占用
                                found = false;
                                break;
                            }
                        }
                        */

                    }
                    newitem.Code = se.ToString();
                    newTestClt.Add(newitem);
                    break;
                //标样测试
                case 1:
                    StandardSample standarditem = new StandardSample();
                    //计算序号
                    for (int i = 0; i < standardSampleClt.Count; i++)
                    {
                        //处理边界情况
                        if (i == standardSampleClt.Count - 1) {
                            se = int.Parse(standardSampleClt[i].Code) + 1;
                            break;
                        }
                        //序号连续情况下，跳过连续号码
                        if (int.Parse(standardSampleClt[i].Code) + 1 == int.Parse(standardSampleClt[i + 1].Code))
                            continue;
                        //找到一个空洞
                        se = int.Parse(standardSampleClt[i].Code + 1);
                    }
                    standarditem.Code = se.ToString();
                    standardSampleClt.Add(standarditem);
                    break;
                default:

                    break;
            }


        }

        private void DelRowBtn_Click(object sender, RoutedEventArgs e)
        {
            int index = 0;
            switch (sampletab.SelectedIndex)
            {
                //新样测试
                case 0:
                    if (NewTargetDgd.SelectedIndex < 0)
                        return;
                    index = getNewCltIndex(NewTargetDgd.SelectedIndex);
                    newTestClt.RemoveAt(index);
                    break;
                //标样测试
                case 1:
                    if (standardSampleDgd.SelectedIndex < 0)
                        return;
                    index = getStandardCltIndex(standardSampleDgd.SelectedIndex);
                    standardSampleClt.RemoveAt(index);
                    break;
                default:

                    break;
            }
        }

        private void OnStandardTabSelected(object sender, RoutedEventArgs e)
        {
            if (window.IsLoaded)
            {
                /*
                var tab = sender as TabItem;
                */
                paramGbx.Visibility = Visibility.Collapsed;
                RBtn.Visibility = Visibility.Visible;
                //rCanvas.Visibility = Visibility.Visible;
                //RparamSp.Visibility = Visibility.Visible;
                Rstackpanel.Visibility = Visibility.Visible;
                printRbtn.Visibility = Visibility.Visible;
                //realCpt.Visibility = Visibility.Collapsed;
                //waveContainer.Visibility = Visibility.Collapsed;
            }
        }

        private void OnNewTabSelected(object sender, RoutedEventArgs e)
        {

            if (window.IsLoaded)
            {
                /*
                var tab = sender as TabItem;
                */
                paramGbx.Visibility = Visibility.Visible;
                RBtn.Visibility = Visibility.Hidden;
                //rCanvas.Visibility = Visibility.Collapsed;
                //RparamSp.Visibility = Visibility.Hidden;
                Rstackpanel.Visibility = Visibility.Collapsed;
                printRbtn.Visibility = Visibility.Collapsed;
                //realCpt.Visibility = Visibility.Visible;
                waveContainer.Visibility = Visibility.Visible;
            }
        }

        private void sampletab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void newItemHead_Click(object sender, RoutedEventArgs e)
        {

        }

        private int getNewCltIndex(int index)
        {
            string code = (NewTargetDgd.SelectedItem as NewTestTarget).Code;
            for (int i = 0; i < newTestClt.Count; i++)
            {
                if (newTestClt[i].Code == code)
                    return i;
            }
            return 0;
        }

        private void standardCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            StandardSample asample = e.AddedItems[0] as StandardSample;
            aTxb.Text = asample.A;
            bTxb.Text = asample.B;
            rTxt.Text = asample.R;

            //如果有平均值则计算汞浓度
            int rowNo = NewTargetDgd.SelectedIndex;
            int cltindex = getNewCltIndex(rowNo);
            if (rowNo < 0)
                return;
            if (newTestClt[cltindex].ResponseValue1 == "" ||
                newTestClt[cltindex].ResponseValue1 == null ||
                //newTestClt[rowNo].ResponseValue2 == "" ||
                //newTestClt[rowNo].ResponseValue2 == null ||
                //newTestClt[rowNo].ResponseValue3 == "" ||
                //newTestClt[rowNo].ResponseValue3 == null
                asample.A is null ||
                asample.B is null ||
                asample.R is null
                )
                return;

            //newTestClt[rowNow].Density = "";
            //double avr = double.Parse(newTestClt[cltindex].ResponseValue1);
            // int.Parse(newTestClt[rowNo].ResponseValue2) +
            // int.Parse(newTestClt[rowNo].ResponseValue3);
            //avr /= 3;
            //newTestClt[cltindex].AverageValue = avr.ToString();

            double a = double.Parse(asample.A);
            double b = double.Parse(asample.B);
            //double d = (avr * t2 * t3 / 1000);
            //double den = (avr - b) / a;
            //newTestClt[cltindex].Density = den.ToString();
            

            //NewTargetDgd.DataContext = null;
            //NewTargetDgd.DataContext = newTestClt;
            //计算气体体积
            if (newTestClt[cltindex].AirFluent == "" || newTestClt[cltindex].AirFluent is null||
                newTestClt[cltindex].AirSampleTime == "" || newTestClt[cltindex].AirSampleTime is null)
            {
                return;
            }
            newTestClt[cltindex].AirTotolBulk = (Math.Round(double.Parse(newTestClt[cltindex].AirFluent) * double.Parse(newTestClt[cltindex].AirSampleTime),
                2)).ToString();
            //y-b/a
            newTestClt[cltindex].AirG = Math.Round(0.001*(double.Parse(newTestClt[cltindex].ResponseValue1) - b) / a, 5).ToString();
        }

        private void modifyBtn_Click(object sender, RoutedEventArgs e)
        {
            //newTestClt[1].Density = "0.333";
            //NewTargetDgd.DataContext = null;
            //NewTargetDgd.DataContext = newTestClt;
            Utility.SaveToNewXmlFileCls.SaveToNewXmlFile(newTestClt, "resources\\NewTestTarget_Table.xml");
            Utility.SaveToStandardXmlFileCls.SaveToStandardXmlFile(standardSampleClt, "resources\\StandardSamples_Table.xml");
        }

        private void saveall_Click(object sender, RoutedEventArgs e)
        {
            NewTargetDgd.DataContext = null;
            NewTargetDgd.DataContext = newTestClt;
            Utility.SaveToNewXmlFileCls.SaveToNewXmlFile(newTestClt, "resources\\NewTestTarget_Table.xml");
            Utility.SaveToStandardXmlFileCls.SaveToStandardXmlFile(standardSampleClt, "resources\\StandardSamples_Table.xml");
        }

        private void exportexelBtn_Click(object sender, RoutedEventArgs e)
        {
            Utility.Save2excel(NewTargetDgd);
            ;
        }

        private void startTestBtn_Click(object sender, RoutedEventArgs e)
        {
            int newcltindex = 0;
            SerialDriver.GetDriver().OnReceived(Com_DataReceived);
            switch (sampletab.SelectedIndex)
            {
                case 0:     //新样
                    switch (startTestBtn.Content as string)
                    {
                        case "开始测试":
                            if (NewTargetDgd.SelectedIndex == -1)
                            {
                                MessageBox.Show("请选择一条样本");
                                return;
                            }
                            newcltindex = getNewCltIndex(NewTargetDgd.SelectedIndex);
                            if (newTestClt[newcltindex].ResponseValue1 != "" && 
                                newTestClt[newcltindex].ResponseValue1 != null)
                            {
                                if (MessageBox.Show("将清除上次的测试结果，是否继续?", "", MessageBoxButton.YesNo) == MessageBoxResult.No) return ;
                                newTestClt[newcltindex].ResponseValue1 = "";
                                newTestClt[newcltindex].AirFluent = "";
                                newTestClt[newcltindex].AirSampleTime = "";
                                newTestClt[newcltindex].AirTotolBulk = "";
                                newTestClt[newcltindex].AirG = "";
                            }

                            statusBtn.Visibility = Visibility.Visible;
                            AnimatedColorButton.Visibility = Visibility.Visible;
                            dots_start_abs = DotManager.GetDotManger().GetDots().Count;
                            //清空图形记录及DotManager中数据
                            realCptDs.Collection.Clear();
                            DotManager.GetDotManger().ReleaseData();

                            //demoTimer.Start();
                            //realCptTimer.Start();
                            startTestBtn.Content = "停止测试";
                            if (SerialDriver.GetDriver().isOpen() == false)
                            {
                                //SerialDriver.GetDriver().Open("COM1", 9600, 0, 8, 1);
                                SerialDriver.GetDriver().Open("COM5", 38400, 0, 8, 1);
                            }
                            break;
                        case "停止测试":
                            statusBtn.Visibility = Visibility.Hidden;
                            AnimatedColorButton.Visibility = Visibility.Hidden;
                            //demoTimer.Stop();
                            //realCptTimer.Stop();
                            
                            startTestBtn.Content = "开始测试";
                            if (SerialDriver.GetDriver().isOpen() == true)
                            {
                                try
                                {
                                    //SerialDriver.GetDriver().RemoveHandler(Com_DataReceived);
                                    System.Threading.Thread CloseDown = 
                                        new System.Threading.Thread(new System.Threading.ThreadStart(closeSerialAsc));
                                    CloseDown.Start();
                                    //SerialDriver.GetDriver().Close();
                                }
                                catch (Exception ex)
                                {
                                    ;
                                }
                            }
                            break;
                        default:

                            break;
                    }
                    break;
                case 1:     //标样
                    switch (startTestBtn.Content as string)
                    {
                        case "开始测试":
                            if (standardSampleDgd.SelectedIndex == -1)
                            {
                                MessageBox.Show("请选择一条样本");
                                return;
                            }
                            if (standardSampleClt[getStandardCltIndex( standardSampleDgd.SelectedIndex )].ResponseValue1 != "" && 
                                standardSampleClt[getStandardCltIndex( standardSampleDgd.SelectedIndex )].ResponseValue1 != null)
                            {
                                if (MessageBox.Show("将清除上次的测试结果，是否继续?", "", MessageBoxButton.YesNo) == MessageBoxResult.No)
                                    return;
                                standardSampleClt[getStandardCltIndex(standardSampleDgd.SelectedIndex)].ResponseValue1 = "";
                            }

                            statusBtn.Visibility = Visibility.Visible;
                            AnimatedColorButton.Visibility = Visibility.Visible;
                            dots_start_abs = DotManager.GetDotManger().GetDots().Count;
                            //清空图形记录及DotManager中数据
                            realCptDs.Collection.Clear();
                            DotManager.GetDotManger().ReleaseData();

                            //demoTimer.Start();
                            //realCptTimer.Start();
                            startTestBtn.Content = "停止测试";
                            if (SerialDriver.GetDriver().isOpen() == false)
                            {
                                //SerialDriver.GetDriver().Open("COM1", 9600, 0, 8, 1);
                                SerialDriver.GetDriver().Open("COM5", 38400, 0, 8, 1);
                            }
                            break;
                        case "停止测试":
                            statusBtn.Visibility = Visibility.Hidden;
                            AnimatedColorButton.Visibility = Visibility.Hidden;
                            //demoTimer.Stop();
                            //realCptTimer.Stop();
                            startTestBtn.Content = "开始测试";
                            if (SerialDriver.GetDriver().isOpen() == true)
                            {
                                //SerialDriver.GetDriver().Close();
                                System.Threading.Thread CloseDown1 =
                                    new System.Threading.Thread(new System.Threading.ThreadStart(closeSerialAsc));
                                CloseDown1.Start();
                            }
                            //计算响应值，填入datagrid之中
                            if (standardSampleClt[getStandardCltIndex( standardSampleDgd.SelectedIndex )].ResponseValue1 == "" ||
                                standardSampleClt[getStandardCltIndex( standardSampleDgd.SelectedIndex )].ResponseValue1 == null)
                                standardSampleClt[getStandardCltIndex( standardSampleDgd.SelectedIndex )].ResponseValue1 = Utility.ComputeResponseValue(
                                    dots_start_abs, DotManager.GetDotManger().GetDots().Count).ToString();
                            break;
                        default:

                            break;
                    }
                    break;
            }
        }

        /*
         * 新线程中关闭serialdriver，避免deadlock
         */
        private void closeSerialAsc()
        {
            SerialDriver.GetDriver().Close();
        }

        private void exitBtn_Click(object sender, RoutedEventArgs e)
        {
            
            this.Close();

        }

        private void testBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void debugBtn_Click(object sender, RoutedEventArgs e)
        {

        }


        private void PacketReceived(object dot, int sequence, PacketType ptype)
        {
            int newcltindex = 0;
            int standardcltindex = 0;
            switch(ptype)
            {
                case PacketType.AIR_FLUENT:
                    Console.WriteLine("气体流量包: "+sequence.ToString());
                    Dispatcher.Invoke(new Action(() =>
                    {
                        if (NewTargetDgd.SelectedIndex == -1)
                            return;
                        newcltindex = getNewCltIndex(NewTargetDgd.SelectedIndex);
                        newTestClt[newcltindex].AirFluent = sequence.ToString();
                    }));
                    break;
                case PacketType.AIR_SAMPLE_TIME:
                    Console.WriteLine("气体采样时间包: " + sequence.ToString());
                    Dispatcher.Invoke(new Action(() =>
                    {
                        if (NewTargetDgd.SelectedIndex == -1)
                            return;
                        newcltindex = getNewCltIndex(NewTargetDgd.SelectedIndex);
                        newTestClt[newcltindex].AirSampleTime = sequence.ToString();
                    }));
                    break;
                case PacketType.DATA_VALUE:

                    double x = currentSecond;
                    double y = (dot as ADot).Rvalue;

                    if (y > maxResponse)
                        maxResponse = y;

                    Point point = new Point(x, y);
                    realCptDs.AppendAsync(base.Dispatcher, point);
                    
                    /*
                    if (false)
                    {
                        if (q.Count < group)
                        {
                            q.Enqueue((int)y);//入队
                            yaxis = 0;
                            foreach (int c in q)
                                if (c > yaxis)
                                    yaxis = c;
                        }
                        else
                        {
                            q.Dequeue();//出队
                            q.Enqueue((int)y);//入队
                            yaxis = 0;
                            foreach (int c in q)
                                if (c > yaxis)
                                    yaxis = c;
                        }
                        if (currentSecond - group > 0)
                            xaxis = currentSecond - group;
                        else
                            xaxis = 0;
                        Dispatcher.Invoke(new Action(() =>
                        {
                            realCpt.Viewport.Visible = new System.Windows.Rect(xaxis, 0, group, yaxis);
                        }));

                    }*/
                    currentSecond++;
                    Console.WriteLine("--- dot " + sequence.ToString() + ": " + (dot as ADot).Rvalue);

                    //采样到达一定点数后，自动结束测试，计算并且显示测试结果。
                    if(sequence >= stop_test_position)
                    {
                        Dispatcher.Invoke(new Action(() =>
                        { 
                            switch (sampletab.SelectedIndex)
                            {
                                case 0:         //新样测试
                                    newcltindex = getNewCltIndex(NewTargetDgd.SelectedIndex);
                                    //newTestClt[newcltindex].ResponseValue1 =
                                    //    Utility.Integration(DotManager.GetDotManger().GetDots(), R1_start, R1_end,this.ratio).ToString();
                                    newTestClt[newcltindex].ResponseValue1 =
                                        maxResponse.ToString();
                                    break;
                                case 1:         //标样测试
                                    //standardSampleClt[getCltIndex(standardSampleDgd.SelectedIndex)].ResponseValue1 =
                                    //    Utility.Integration(DotManager.GetDotManger().GetDots(), R1_start, R1_end, this.ratio).ToString();
                                    standardSampleClt[getStandardCltIndex(standardSampleDgd.SelectedIndex)].ResponseValue1 =
                                        maxResponse.ToString();
                                    break;
                            }
                            startTestBtn.Content = "开始测试";
                            AnimatedColorButton.Visibility = Visibility.Hidden;
                        }));

                        try
                        {
                            //SerialDriver.GetDriver().RemoveHandler(Com_DataReceived);
                            System.Threading.Thread CloseDown =
                                new System.Threading.Thread(new System.Threading.ThreadStart(closeSerialAsc));
                            CloseDown.Start();
                            //SerialDriver.GetDriver().Close();
                        }
                        catch (Exception ex)
                        {
                            ;
                        }
                    }

                    break;
            }

        }

        private void CorrectedPacketReceived(DataFormater.ADot dot, int sequence)
        {
            Console.WriteLine("--- c dot " + sequence.ToString() + ": " + dot.Rvalue);
        }

        private void saveDotsMenu_Checked(object sender, RoutedEventArgs e)
        {
        }

        private void saveDotsMenu_Click(object sender, RoutedEventArgs e)
        {
            //string s = Utility.GetValueFrXml("descendant::R1", "pointstart");

            Collection<ADot> dots =  DotManager.GetDotManger().GetDots();
            ;
        }

        private void saveRawTextMenu_Click(object sender, RoutedEventArgs e)
        {
            DotManager.GetDotManger().DumpRawText("rawtext.txt");
        }

        private void sendTextMenu_Click(object sender, RoutedEventArgs e)
        {
            SerialDriver.GetDriver().OnReceived(Com_DataReceived);
            ProduceFakeData pfd = new ProduceFakeData("实际数据.txt");
            pfd.Send(1);
            ;

        }

        private void RBtn_Click(object sender, RoutedEventArgs e)
        {
            double[] x;
            double[] y;
            int len = 0;
            int index = 0;
            double a, b, R;
        
            if(standardSampleDgd.SelectedIndex == -1)
            {
                MessageBox.Show("请选择标样组");
                return;
            }
            string groupname = standardSampleClt[getStandardCltIndex( standardSampleDgd.SelectedIndex )].GroupName;
            foreach(StandardSample item in standardSampleClt)
            {
                if (item.GroupName == groupname)
                {
                    if(item.ResponseValue1 == "" || item.ResponseValue1 is null ||
                       item.AirG =="" || item.AirG is null
                        )
                    {
                        MessageBox.Show("测试未完成，无法计算");
                        return;
                    }
                    else
                    {
                        len++;
                    }
                }
            }
            x = new double[len];
            y = new double[len];
            foreach(StandardSample v in standardSampleClt)
            {
                if (v.GroupName == groupname)
                {
                    x[index] = double.Parse(v.AirG);
                    y[index] = double.Parse(v.ResponseValue1);
                    index++;
                }
                
            }
            Utility.ComputeAB(out a, out b, x, y);
            R = Utility.ComputeR(x, y);
            foreach(StandardSample v in standardSampleClt)
            {
                if (v.GroupName == groupname)
                {
                    v.A = Math.Round(a,2).ToString();
                    v.B = Math.Round(b,2).ToString();
                    v.R = Math.Round(R,3).ToString();
                }
            }
        }

        /*
         * 绘制相关系数点直线图
         * 
         */
        private void drawR(double[] x, double[] y, double a, double b, double r, string groupname)
        {
            double maxX = 0, maxY = 0,ratiox = 0,ratioy = 0;
            Line scaleline;
            TextBlock scaletext;
            TextBlock ytext = new TextBlock();
            TextBlock xtext = new TextBlock();
            Line arrowx = new Line();
            Line arrowy = new Line();
            double ScaleGapX;    //x轴单位粒度所占用的点数
            double ScaleGapY;    //y轴单位粒度说占用的点数
            double gapX;        //x轴一个大单位的粒度数
            int gapY;        //y轴一个大单位的粒度数
            double validwidth; //有效区宽度
            double validheight;  //有效区高度

            int topmargin = 20; //顶端的空白，为20个点
            int rightmargin = 200; //右边的空白，为20个点
            int leftmargin = 30;    //左边的空白
            int bottommargin = 20;  //底部的空白

            rCanvas.Children.Clear();

            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] > maxX) maxX = x[i];
                if (y[i] > maxY) maxY = y[i];
            }
            //ratiox = (rCanvas.Width - rightmargin - leftmargin) / (maxX);
            //ratioy = (rCanvas.Height - topmargin)/ (maxY);

            ScaleGapX = (rCanvas.Width - rightmargin - leftmargin) / maxX;
            ScaleGapY = (rCanvas.Height - topmargin - bottommargin) / maxY;
            gapX = (int)(maxX * 10 / x.Length); //maxX太小，比如18.81
            gapX = Math.Round(gapX / 10, 1);
            gapY = (int)(maxY / y.Length);
            gapY = (int)(gapY / 100);
            gapY *= 100;
            validwidth = rCanvas.Width - leftmargin - rightmargin;
            validheight = rCanvas.Height - topmargin - bottommargin;

            //绘制y轴
            Line yline = new Line();
            yline.Stroke = Brushes.Black;
            yline.StrokeThickness = 1; ;
            yline.X1 = leftmargin;
            yline.Y1 = 0;
            yline.X2 = leftmargin;
            yline.Y2 = rCanvas.Height - bottommargin;
            rCanvas.Children.Add(yline);
            ytext.Text = "Y 响应值";
            Canvas.SetTop(ytext, 15);
            Canvas.SetLeft(ytext, leftmargin + 36);
            rCanvas.Children.Add(ytext);
            arrowy.Stroke = Brushes.Black;
            arrowy.StrokeThickness = 1;
            arrowy.X1 = leftmargin;
            arrowy.Y1 = 0;
            arrowy.X2 = leftmargin + 5;
            arrowy.Y2 = 5;
            rCanvas.Children.Add(arrowy);
            //绘制y轴scale尺度
            for (int i = 0; i < y.Length; i++)
            {
                scaleline = new Line();
                scaleline.Stroke = Brushes.Black;
                scaleline.StrokeThickness = 1;
                scaleline.X1 = leftmargin;
                scaleline.Y1 = validheight - gapY * (i + 1)*ScaleGapY + topmargin;
                scaleline.X2 = leftmargin + 5;
                scaleline.Y2 = validheight - gapY * (i + 1) * ScaleGapY + topmargin;
                rCanvas.Children.Add(scaleline);
                scaletext = new TextBlock();
                //scaletext.Text = Math.Round((maxY/(y.Length))*(i+1),2).ToString();
                scaletext.Text = (gapY*(i+1)).ToString();
                Canvas.SetLeft(scaletext,leftmargin + 2);
                Canvas.SetTop(scaletext, (validheight - gapY*(i+1)*ScaleGapY) + topmargin);
                rCanvas.Children.Add(scaletext);
            }

            //绘制x轴
            Line xline = new Line();
            xline.Stroke = Brushes.Black;
            xline.StrokeThickness = 1; ;
            xline.X1 = leftmargin;
            xline.Y1 = rCanvas.Height - bottommargin;
            xline.X2 = rCanvas.Width;
            xline.Y2 = rCanvas.Height - bottommargin;
            rCanvas.Children.Add(xline);
            xtext.Text = "X 汞量(ng)";
            Canvas.SetTop(xtext, rCanvas.Height - 20 - bottommargin);
            Canvas.SetLeft(xtext, rCanvas.Width - 70 );
            rCanvas.Children.Add(xtext);
            arrowx.Stroke = Brushes.Black;
            arrowx.StrokeThickness = 1;
            arrowx.X1 = rCanvas.Width - 10;
            arrowx.Y1 = rCanvas.Height - 10 - bottommargin;
            arrowx.X2 = rCanvas.Width;
            arrowx.Y2 = rCanvas.Height - bottommargin;
            rCanvas.Children.Add(arrowx);
            //绘制x轴scale
            for (int i = 0; i < x.Length; i++)
            {
                scaleline = new Line();
                scaleline.Stroke = Brushes.Black;
                scaleline.StrokeThickness = 1;
                scaleline.X1 = gapX * (i + 1)* ScaleGapX + leftmargin;
                scaleline.Y1 = rCanvas.Height -  bottommargin;
                scaleline.X2 = gapX * (i + 1)* ScaleGapX + leftmargin;
                scaleline.Y2 = rCanvas.Height -5 - bottommargin;
                rCanvas.Children.Add(scaleline);
                scaletext = new TextBlock();
                scaletext.Text = (gapX * (i + 1)).ToString();
                Canvas.SetLeft(scaletext, gapX*(i+1)* ScaleGapX - 20 + leftmargin);
                Canvas.SetTop(scaletext, (rCanvas.Height - 20) - bottommargin);
                rCanvas.Children.Add(scaletext);
            }

            //画斜线
            /*
            double x1 = 0.01 * ratiox;
            double y1 = (0.01*a+b)*ratioy;
            double x2 = (maxX+10)*ratiox;
            double y2 = ((maxX + 10) * a + b) * ratioy;
            */
            double x1 = 0 + leftmargin;
            double y1 = validheight - b * ScaleGapY + topmargin;
            double x2 = maxX * ScaleGapX + leftmargin;
            double y2 = validheight - maxY * ScaleGapY + topmargin;
            Line mydrawline = new Line();
            mydrawline.Stroke = Brushes.Black;//mydrawline.Stroke = new SolidColorBrush(Color.FromArgb(0xFF, 0x5B, 0x9B, 0xD5));
            mydrawline.StrokeThickness = 1;
            mydrawline.X1 = x1;
            mydrawline.Y1 = y1;
            mydrawline.X2 = x2;
            mydrawline.Y2 = y2;
            rCanvas.Children.Add(mydrawline);

            //画点
            for (int i = 0; i < x.Length; i++)
            {
                //画中心点
                Ellipse centereli = new Ellipse();
                centereli.Stroke = System.Windows.Media.Brushes.Red;
                centereli.Fill = System.Windows.Media.Brushes.Red;
                centereli.Width = 3;
                centereli.Height = 3;
                Thickness mrg = new Thickness(x[i] * ScaleGapX + leftmargin -1.5, 
                    validheight - y[i] * ScaleGapY + topmargin -1.5, 0, 0);
                centereli.Margin = mrg;
                rCanvas.Children.Add(centereli);
                //画外圈
                Ellipse outeli = new Ellipse();
                outeli.Stroke = System.Windows.Media.Brushes.Blue;
                //outeli.Fill = System.Windows.Media.Brushes.Red;
                outeli.Width = 6;
                outeli.Height = 6;
                Thickness outmrg = new Thickness(x[i] * ScaleGapX + leftmargin -3, 
                    validheight - y[i] * ScaleGapY + topmargin -3, 0, 0);
                outeli.Margin = outmrg;
                rCanvas.Children.Add(outeli);
            }
            //在右侧显示显示信息
            TextBlock RparamSpTxbl = new TextBlock();
            RparamSpTxbl.Text = "";
            RparamSpTxbl.Text = "样品名称  响应值  汞量ng\r\n";
            int j = 0;
            for (int i=0; i < x.Length; i++)
            {
                for(int k=j; k < standardSampleClt.Count; k++)
                {
                    if(standardSampleClt[k].GroupName == groupname)
                    {
                        RparamSpTxbl.Text += standardSampleClt[k].SampleName +
                            "\t  " + standardSampleClt[k].ResponseValue1 + "\t " +
                            standardSampleClt[k].AirG + "\r\n";
                        j=k+1;
                        break;
                    }
                }
                
            }
            RparamSpTxbl.Text += "\r\n";
            RparamSpTxbl.Text += "斜率:  " + a.ToString() + "\r\n";
            RparamSpTxbl.Text += "截距:  " + b.ToString() + "\r\n";
            RparamSpTxbl.Text += "相关系数:  " + r.ToString();
            Canvas.SetTop(RparamSpTxbl, 20);
            Canvas.SetLeft(RparamSpTxbl, rCanvas.Width - rightmargin + 30);
            
            rCanvas.Children.Add(RparamSpTxbl);

        }

        //在标样选择中，将视图选中序号转变为数据源中序号
        private int getStandardCltIndex(int index)
        {
            string code = (standardSampleDgd.SelectedItem as StandardSample).Code;
            for(int i=0; i < standardSampleClt.Count; i++)
            {
                if (standardSampleClt[i].Code == code)
                    return i;
            }
            return 0;
        }

        private void standardSampleDgd_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            //计算相关系数及截率
            double[] x;
            double[] y;
            int len = 0;
            int index = 0;
            int cltindex = 0;
            double a, b, R;

            if (standardSampleDgd.SelectedIndex < 0) return;

            cltindex = getStandardCltIndex(standardSampleDgd.SelectedIndex);

            //数据未测试完成，则不计算相关系数
            string groupname = standardSampleClt[cltindex].GroupName;
            foreach (StandardSample item in standardSampleClt)
            {
                if (item.GroupName == groupname)
                {
                    if (item.ResponseValue1 == "" || item.ResponseValue1 is null ||
                        item.AirG == "" || item.AirG is null
                        )
                    {
                        //MessageBox.Show("测试未完成，无法计算R");
                        return;
                    }
                    else
                    {
                        len++;
                    }
                }
            }

            //收集新x,y数组
            x = new double[len];
            y = new double[len];
            foreach (StandardSample v in standardSampleClt)
            {
                if (v.GroupName == groupname)
                {
                    x[index] = double.Parse(v.AirG);
                    y[index] = double.Parse(v.ResponseValue1);
                    index++;
                }
            }

            Utility.ComputeAB(out a, out b, x, y);

            R = Utility.ComputeR(x, y);
            foreach (StandardSample v in standardSampleClt)
            {
                if (v.GroupName == groupname)
                {
                    v.A = Math.Round(a,2).ToString();
                    v.B = Math.Round(b,2).ToString();
                    v.R = Math.Round(R,3).ToString();
                }

            }
            //}
            //绘制R 线性回归图
            //
            if (a.ToString() == "NaN" || b.ToString() == "NaN") return;
            drawR(x, y, Math.Round(a, 2), Math.Round(b, 2), Math.Round(R, 3), groupname);
        }

        private void testliquidMenu_Click(object sender, RoutedEventArgs e)
        {
            newAirSampTimeCol.Visibility = Visibility.Hidden;
            newAirFluentCol.Visibility = Visibility.Hidden;
            newLiquidBulkCol.Visibility = Visibility.Visible;
            standardAirSampleTimeCol.Visibility = Visibility.Hidden;
            standardAirFluentCol.Visibility = Visibility.Hidden;
            standardPlaceCol.Visibility = Visibility.Visible;
            standardProviderCol.Visibility = Visibility.Visible;
            sampleType = SampleType.LIQUID;
        }

        private void testairMenu_Click(object sender, RoutedEventArgs e)
        {
            newAirSampTimeCol.Visibility = Visibility.Visible;
            newAirFluentCol.Visibility = Visibility.Visible;
            newLiquidBulkCol.Visibility = Visibility.Hidden;
            standardAirSampleTimeCol.Visibility = Visibility.Visible;
            standardAirFluentCol.Visibility = Visibility.Visible;
            standardPlaceCol.Visibility = Visibility.Hidden;
            standardProviderCol.Visibility = Visibility.Hidden;
            sampleType = SampleType.AIR;
        }

        /*
         * 将表格中的数据保存到历史xml文件中
         */
        private void savehistory_Click(object sender, RoutedEventArgs e)
        {
            Utility.Save2XmlHistory(newTestClt,standardSampleClt);
            ;
        }

        private void loadhistory_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.DefaultExt = "xml";
            openDialog.Filter = "xml 文件|*.xml";
            openDialog.ShowDialog();
            string filename = openDialog.FileName;
            ;
            //截取日期
            string suffix = "";
            if (filename.Length < 16) return;
            for (int i = filename.Length - 16; i < filename.Length - 4; i++)
            {
                suffix += filename[i];
            }
            ;
            string newtestxml = "history\\样本测试表格" + suffix + ".xml";
            string standardtestxml = "history\\标样测试表格" + suffix + ".xml";

            newTestClt = Utility.getNewTestTargetDataFromXml(newtestxml);
            NewTargetDgd.DataContext = newTestClt;
            standardSampleClt = Utility.getStandardTargetDataFromXml(standardtestxml);
            standardSampleDgd.DataContext = standardSampleClt;
        }

        //更新新样数据源，保持数据同步
        private void updateNewClt(string v, int row, string head)
        {
            int index = getNewCltIndex(row);
            switch (head)
            {
                case "样品名":
                    newTestClt[index].NewName = v;
                    break;
                case "产地":
                    newTestClt[index].Place = v;
                    break;
                case "响应值":
                    newTestClt[index].ResponseValue1 = v;
                    break;
                case "取样时间m":
                    newTestClt[index].AirSampleTime = v;
                    break;
                case "流量L/m":
                    newTestClt[index].AirFluent = v;
                    break;
                case "汞浓度ng/mL":
                    newTestClt[index].Density = v;
                    break;
                case "样品质量":
                    newTestClt[index].Weight = v;
                    break;
                case "样品消化液总体积ml":

                    break;
                case "样品中汞含量":
                    
                    break;
            }
        }

        //更新标样数据源，保持数据同步
        private void updateStandardClt(string v, int row, string head)
        {
            int index = getStandardCltIndex(row);
            switch (head)
            {
                case "组名":
                    standardSampleClt[index].GroupName = v;
                    break;
                case "样品名":
                    standardSampleClt[index].SampleName = v;
                    break;
                case "汞浓度ng/mL":
                    standardSampleClt[index].Density = v;
                    break;
                case "响应值":
                    standardSampleClt[index].ResponseValue1 = v;
                    break;
                case "取样时间m":
                    standardSampleClt[index].AirSampleTime = v;
                    break;
                case "流量L/m":
                    standardSampleClt[index].AirFluent = v;
                    break;
                case "温度":
                    standardSampleClt[index].Temperature = v;
                    break;
                case "标样体积mL":
                    standardSampleClt[index].AirML = v;
                    break;
                case "汞量ng":
                    standardSampleClt[index].AirG = Math.Round(double.Parse(v),2).ToString();
                    break;
                case "样品质量":
                    standardSampleClt[index].Weight = v;
                    break;
                case "样品供应商":
                    standardSampleClt[index].ProviderCompany = v;
                    break;
                case "产地":
                    standardSampleClt[index].Place = v;
                    break;
                case "样品购买日期":
                    standardSampleClt[index].BuyDate = v;
                    break;
            }
        }

        private void NewTargetDgd_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditingElement is null) return;
            if (!(e.EditingElement is TextBox)) return;
            string vle = (e.EditingElement as TextBox).Text;
            string headname = e.Column.Header as string;
            //int cltindex = getNewCltIndex(NewTargetDgd.SelectedIndex);

            //将数据更新到数据源
            updateNewClt(vle, NewTargetDgd.SelectedIndex, headname);

        }

        private void standardSampleDgd_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditingElement is null) return;
            if (!(e.EditingElement is TextBox)) return;
            string vle = (e.EditingElement as TextBox).Text;
            string headname = e.Column.Header as string;
            int cltindex = getStandardCltIndex(standardSampleDgd.SelectedIndex);
            double den, bulk;
            XmlNode node;
            //将数据更新到数据源
            updateStandardClt(vle, standardSampleDgd.SelectedIndex, headname);
            if (vle == "" || vle is null)
                return;
            //计算汞量
            switch (headname)
            {
                case "温度":
                    if (standardSampleClt[cltindex].AirML is null || standardSampleClt[cltindex].AirML == "")
                        return;
                    try
                    {
                        node = AirDensityXml.SelectSingleNode("/air/density[@hot=" + vle + "]");
                    }
                    catch
                    {
                        //MessageBox.Show("");
                        return;
                    }
                    
                    if (node is null) return;

                    try
                    {
                        bulk = double.Parse(standardSampleClt[cltindex].AirML);
                        den = double.Parse(node.InnerText);
                    }
                    catch
                    {
                        return;
                    }

                    standardSampleClt[cltindex].AirG = Math.Round((den * bulk),2).ToString();
                    break;
                case "标样体积mL":
                    if (standardSampleClt[cltindex].Temperature is null || standardSampleClt[cltindex].Temperature == "")
                        return;
                    string tem = standardSampleClt[cltindex].Temperature;
                    node = AirDensityXml.SelectSingleNode("/air/density[@hot=" + tem + "]");
                    if (node is null) return;
                    try
                    {
                        bulk = double.Parse(vle);
                        den = double.Parse(node.InnerText);
                    }
                    catch
                    {
                        return;
                    }
                    standardSampleClt[cltindex].AirG = Math.Round((den * bulk),2).ToString();
                    break;

            }

        }

        private void printRmenu_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog dlg = new PrintDialog();
            if (dlg.ShowDialog() == true)
            {
                dlg.PrintVisual(rCanvas, "Print Receipt");
            }
        }

        private void exitMenu_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void saveallbtn_Click(object sender, RoutedEventArgs e)
        {
            Utility.SaveToNewXmlFileCls.SaveToNewXmlFile(newTestClt, "resources\\NewTestTarget_Table.xml");
            Utility.SaveToStandardXmlFileCls.SaveToStandardXmlFile(standardSampleClt, "resources\\StandardSamples_Table.xml");
            MessageBox.Show("数据已保存");
        }

        private void exportExcelBtn_Click(object sender, RoutedEventArgs e)
        {
            switch (sampletab.SelectedIndex)
            {
                case 0:  //新样
                    Utility.Save2excel(NewTargetDgd);
                    break;
                case 1: //标样
                    Utility.Save2excel(standardSampleDgd);
                    break;
            }
        }

        private void printRbtn_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog dlg = new PrintDialog();
            if (dlg.ShowDialog() == true)
            {
                dlg.PrintVisual(rCanvas, "Print Receipt");
            }
        }

        private void exportDotsBtn_Click(object sender, RoutedEventArgs e)
        {
            string filename = "";

            Collection<ADot> dots = DotManager.GetDotManger().GetDots();
            if (dots is null || dots.Count == 0)
            {
                MessageBox.Show("没有发现测试数据");
                return;
            }

            string now = DateTime.Now.ToString();
            string suffix = "";
            for (int i = 0; i < now.Length; i++)
            {
                if (now[i] != ':' && now[i] != '/' && now[i] != ' ')
                {
                    suffix += now[i];
                }
            }
            ;
            switch (sampletab.SelectedIndex)
            {
                case 0:  //新样测试
                    filename = "history\\样本测试原始数据" + suffix + ".raw";
                    break;
                case 1:  //标样测试
                    filename = "history\\标样测试原始数据" + suffix + ".raw";
                    break;
            }


            FileStream aFile = new FileStream(filename,FileMode.Create);
            StreamWriter sr = new StreamWriter(aFile);
            for (int i = 0; i < dots.Count; i++)
            {
                sr.Write(dots[i].Rvalue.ToString()+"\r\n");

            }
            sr.Close();
            aFile.Close();

            MessageBox.Show("文件已保存到history目录中");

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void standardCmb_Loaded(object sender, RoutedEventArgs e)
        {
            //去除重复的组名
            ComboBox cmb = sender as ComboBox;
            cmb.ItemsSource = standardSampleDgd.ItemsSource;
            Collection<StandardSample> items = cmb.ItemsSource as Collection<StandardSample>;
            if (items.Count == 0) return;

            Collection<StandardSample> tempitem = new Collection<StandardSample>();
            tempitem.Add(items[0]);
            bool isok = true;
            
            for(int i = 1; i< items.Count; i++)
            {
                for(int k = 0; k < tempitem.Count; k++)
                {
                    if(items[i].GroupName == tempitem[k].GroupName)
                    {
                        isok = false;
                        break;
                    }
                }
                if (isok)
                {
                    tempitem.Add(items[i]);
                }
                isok = true;
            }

            cmb.ItemsSource = tempitem;
        }

        private void checkerlogin_Click(object sender, RoutedEventArgs e)
        {
            Window w = new CheckerLoginWnd();
            w.Owner = this;
            w.ShowDialog();
        }

        private void checkerlogout_Click(object sender, RoutedEventArgs e)
        {
            checkerbtn.Visibility = Visibility.Hidden;
        }

        private void modifytestmenu_Click(object sender, RoutedEventArgs e)
        {
            switch (sampletab.SelectedIndex)
            {
                case 0:
                    if(NewTargetDgd.SelectedIndex == -1)
                    {
                        MessageBox.Show("请选择一条记录");
                        return;
                    }
                    break;
                case 1:
                    if(standardSampleDgd.SelectedIndex == -1)
                    {
                        MessageBox.Show("请选择一条记录");
                        return;
                    }
                    break;
            }
            ModifyDataWnd w = new ModifyDataWnd();
            w.Owner = this;
            w.ShowDialog();
        }

        private void exportexcelmenu_Click(object sender, RoutedEventArgs e)
        {
            switch (sampletab.SelectedIndex)
            {
                case 0:  //新样
                    Utility.Save2excel(NewTargetDgd);
                    break;
                case 1: //标样
                    Utility.Save2excel(standardSampleDgd);
                    break;
            }
        }
    }
}
