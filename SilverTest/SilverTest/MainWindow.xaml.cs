using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Win32;
using SilverTest.libs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
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
using System.Windows.Navigation;
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
            /*
            XmlNode sn = AirDensityXml.SelectSingleNode("/air/density[@hot=70.0]");
            double vl = double.Parse(sn.InnerText);
            */
            /*
            newTestClt =
            Utility.getNewTestTargetDataFromXDP((DataSourceProvider)this.FindResource("newTargetData"));
            NewTargetDgd.DataContext = newTestClt;

            standardSampleClt = 
            Utility.getStandardTargetDataFromXDP((DataSourceProvider)this.FindResource("standardSampleData"));
            standardSampleDgd.DataContext = standardSampleClt;
            */
            newTestClt =
                    Utility.getNewTestTargetDataFromXml("resources\\NewTestTarget_Table.xml");
            NewTargetDgd.DataContext = newTestClt;
            standardSampleClt =
                Utility.getStandardTargetDataFromXml("resources\\StandardSamples_Table.xml");
            standardSampleDgd.DataContext = standardSampleClt;
            ;
            StandardCvw = CollectionViewSource.GetDefaultView(standardSampleClt);
            StandardCvw.GroupDescriptions.Add(new PropertyGroupDescription("GroupName"));

            //演示代码
            //demoTimer.Interval =  new TimeSpan(0, 0, 0, 1);  //1 seconds
            //demoTimer.Tick += new EventHandler(timeCycle);
        }

        //演示代码
        /*
        public void timeCycle(object sender, EventArgs e)
        {
            seconds++;
            if (seconds == 3)
            {
                newTestClt[2].ResponseValue1 = "30";
                NewTargetDgd.DataContext = null;
                NewTargetDgd.DataContext = newTestClt;
            }
            if(seconds == 5)
            {
                newTestClt[2].ResponseValue2 = "20";
                NewTargetDgd.DataContext = null;
                NewTargetDgd.DataContext = newTestClt;
            }
            if(seconds == 7)
            {
                newTestClt[2].ResponseValue3 = "25";
                NewTargetDgd.DataContext = null;
                NewTargetDgd.DataContext = newTestClt;
            }
            ;
        }
        */

        private void realTck(object sender, EventArgs e)
        {
            /*
            //演示代码
            double x = currentSecond;
            double y = rd.Next(5, 30);
            Point point = new Point(x, y);
            realCptDs.AppendAsync(base.Dispatcher, point);
            if (true)
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
                realCpt.Viewport.Visible = new System.Windows.Rect(xaxis, 0, group, yaxis);
            }
            currentSecond++;
            */
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
            string re ="";
            for(int i = 0; i< ReDatas.Length; i++)
            {
                re += (char)ReDatas[i];
            }
            //Console.WriteLine("serial . received data: " + re);
            DotManager.GetDotManger().GetDot(ReDatas);

            //DataFormater.getDataFormater().GetDot(ReDatas);
            
            /*            
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ReDatas.Length; i++)
            {
                sb.AppendFormat("{0:x2}" + " ", ReDatas[i]);
            }
            Dispatcher.Invoke(new Action(() =>
            {
                rTxt.Text = sb.ToString();
            }));
            */
        }

        /*
        public void Com_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            //接受数据
            byte[] ReDatas = new byte[ComDevice.BytesToRead];
            ComDevice.Read(ReDatas, 0, ReDatas.Length);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ReDatas.Length; i++)
            {
                sb.AppendFormat("{0:x2}" + " ", ReDatas[i]);
            }


            Dispatcher.Invoke(new Action(() =>
            {
                ;
            }));

        }
        */

        //演示
        //画波形
        //number = 1001 
        /*
        private void drawWave_simulate(int number)
        {

            var x = Enumerable.Range(0, number).Select(i => i / 10.0).ToArray();
            var y = x.Select(v => Math.Abs(v) < 1e-10 ? 1 : Math.Sin(v) / v).ToArray();
            linegraph.Plot(x, y);
        }
        */


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
                    for(int i = 0; i < newTestClt.Count; i++)
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
                paramGbx.Visibility = Visibility.Hidden;
                RBtn.Visibility = Visibility.Visible;
                rCanvas.Visibility = Visibility.Visible;
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
                rCanvas.Visibility = Visibility.Collapsed;
            }
        }

        private void sampletab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            /*
            if (window.IsLoaded)
            {
                if (newtabitem.IsSelected)
                {

                }
                else if (standardtabitem.IsSelected)
                {
                }
                else
                {
                    //do nothing
                    ;
                }
            }
            */
        }

        private void newItemHead_Click(object sender, RoutedEventArgs e)
        {

        }

        private int getNewCltIndex(int index)
        {
            string code = (NewTargetDgd.SelectedItem as NewTestTarget).Code;
            for(int i = 0; i< newTestClt.Count; i++)
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
            if (rowNo < 0 )
                return;
            if (newTestClt[cltindex].ResponseValue1 == "" ||
                newTestClt[cltindex].ResponseValue1 == null 
                //newTestClt[rowNo].ResponseValue2 == "" ||
                //newTestClt[rowNo].ResponseValue2 == null ||
                //newTestClt[rowNo].ResponseValue3 == "" ||
                //newTestClt[rowNo].ResponseValue3 == null
                )
                return;
            {

                //newTestClt[rowNow].Density = "";
                double avr = double.Parse(newTestClt[cltindex].ResponseValue1);
                   // int.Parse(newTestClt[rowNo].ResponseValue2) +
                   // int.Parse(newTestClt[rowNo].ResponseValue3);
                //avr /= 3;
                newTestClt[cltindex].AverageValue = avr.ToString();

                double a = double.Parse(asample.A);
                double b = double.Parse(asample.B);
                //double d = (avr * t2 * t3 / 1000);
                double den = (avr - b) / a;
                newTestClt[cltindex].Density = den.ToString();
            }

            //NewTargetDgd.DataContext = null;
            //NewTargetDgd.DataContext = newTestClt;
     
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
                                MessageBox.Show("数据已经满，请去掉网格中数据重新开始测试");
                                return;
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
                            
                            //计算响应值，填入datagrid之中
                            /*
                            if (newTestClt[NewTargetDgd.SelectedIndex].ResponseValue1 == "" ||
                                newTestClt[NewTargetDgd.SelectedIndex].ResponseValue1 == null)
                                newTestClt[NewTargetDgd.SelectedIndex].ResponseValue1 = Utility.ComputeResponseValue(
                                    dots_start_abs, DotManager.GetDotManger().GetDots().Count).ToString();
                            else if (newTestClt[NewTargetDgd.SelectedIndex].ResponseValue2 == "" ||
                                newTestClt[NewTargetDgd.SelectedIndex].ResponseValue2 == null)
                                newTestClt[NewTargetDgd.SelectedIndex].ResponseValue2 = Utility.ComputeResponseValue(
                                    dots_start_abs, DotManager.GetDotManger().GetDots().Count).ToString();
                            else if (newTestClt[NewTargetDgd.SelectedIndex].ResponseValue3 == "" ||
                                newTestClt[NewTargetDgd.SelectedIndex].ResponseValue3 == null)
                            {
                                newTestClt[NewTargetDgd.SelectedIndex].ResponseValue3 = Utility.ComputeResponseValue(
                                    dots_start_abs, DotManager.GetDotManger().GetDots().Count).ToString();
                            }
                            */

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
                                MessageBox.Show("数据已经满，请去掉网格中数据重新开始测试");
                                return;
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
            
            
            //int selectedItem = NewTargetDgd.SelectedIndex;

            //NewTargetDgd.IsEnabled = true;
            
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
            /*
            SerialDriver.GetDriver().OnReceived(Com_DataReceived);
            ProduceFakeData pfd = new ProduceFakeData("实际数据.txt");
            pfd.Send(1);
            ;
            */
            /*
            if(DataFormater.getDataFormater().getStatus() == DataFormater.ErrorOccur.NONE)
            {
                DataFormater.getDataFormater().GetDot(new byte[] { 1,2,3,4,5});
            }
            */
        }

        private void debugBtn_Click(object sender, RoutedEventArgs e)
        {
            //DataFormater.getDataFormater().GetDots();
            //byte[] r = DataFormater.getDataFormater().GetRawData();
            ;
            //DataFormater.getDataFormater().SaveToFile("raw.txt");
            
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
            //
            

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
                    if(item.ResponseValue1 == "" || item.ResponseValue1 is null)
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
                    x[index] = double.Parse(v.Density);
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
                    v.A = a.ToString();
                    v.B = b.ToString();
                    v.R = R.ToString();
                }
                
            }
        }

        /*
         * 绘制相关系数点直线图
         */
        private void drawR(double[] x, double[] y, double a, double b)
        {
            double maxX = 0, maxY = 0,ratiox = 0,ratioy = 0;

            rCanvas.Children.Clear();

            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] > maxX) maxX = x[i];
                if (y[i] > maxY) maxY = y[i];
            }
            ratiox = rCanvas.Width / (maxX + 10);
            ratioy = rCanvas.Height / (maxY + 10);

            double x1 = 0.01 * ratiox;
            double y1 = (0.01*a+b)*ratioy;
            double x2 = (maxX+10)*ratiox;
            double y2 = ((maxX + 10) * a + b) * ratioy;
            

            Line mydrawline = new Line();
            mydrawline.Stroke = Brushes.Black;//mydrawline.Stroke = new SolidColorBrush(Color.FromArgb(0xFF, 0x5B, 0x9B, 0xD5));
            mydrawline.StrokeThickness = 3;
            mydrawline.X1 = x1;
            mydrawline.Y1 = rCanvas.Height - y1;
            mydrawline.X2 = x2;
            mydrawline.Y2 = rCanvas.Height - y2;
            rCanvas.Children.Add(mydrawline);


            //画点
            for (int i = 0; i < x.Length; i++)
            {
                Ellipse eli = new Ellipse();
                eli.Stroke = System.Windows.Media.Brushes.Black;
                eli.Fill = System.Windows.Media.Brushes.DarkBlue;
                eli.Width = 5;
                eli.Height = 5;
                Thickness mrg = new Thickness(x[i] * ratiox, rCanvas.Height - y[i] * ratioy, 0, 0);
                eli.Margin = mrg;
                rCanvas.Children.Add(eli);
            }
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

            /*
            if (standardSampleClt[cltindex].R == null ||
                standardSampleClt[cltindex].R == "") 
            {*/
            R = Utility.ComputeR(x, y);
            foreach (StandardSample v in standardSampleClt)
            {
                if (v.GroupName == groupname)
                {
                    v.A = a.ToString();
                    v.B = b.ToString();
                    v.R = R.ToString();
                }

            }
            //}
            //绘制R 线性回归图
            //
            if (a.ToString() == "NaN" || b.ToString() == "NaN") return;
            drawR(x, y, a, b);
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
            if (filename.Length < 17) return;
            for (int i = filename.Length - 17; i < filename.Length - 4; i++)
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
                case "响应值1":
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
                    standardSampleClt[index].AirG = v;
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
            string vle = (e.EditingElement as TextBox).Text;
            string headname = e.Column.Header as string;
            //int cltindex = getNewCltIndex(NewTargetDgd.SelectedIndex);

            //将数据更新到数据源
            updateNewClt(vle, NewTargetDgd.SelectedIndex, headname);

        }

        private void standardSampleDgd_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
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
                    node = AirDensityXml.SelectSingleNode("/air/density[@hot=" + vle + "]");

                    bulk = double.Parse(standardSampleClt[cltindex].AirML);
                    den = double.Parse(node.InnerText);
                    standardSampleClt[cltindex].AirG = (den * bulk).ToString();
                    break;
                case "标样体积mL":
                    if (standardSampleClt[cltindex].Temperature is null || standardSampleClt[cltindex].Temperature == "")
                        return;
                    string tem = standardSampleClt[cltindex].Temperature;
                    node = AirDensityXml.SelectSingleNode("/air/density[@hot=" + tem + "]");

                    bulk = double.Parse(vle);
                    den = double.Parse(node.InnerText);
                    standardSampleClt[cltindex].AirG = (den * bulk).ToString();
                    break;

            }

        }



        /*
        private void SomeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            var selectedItem = this.NewTargetDgd.CurrentItem;
            MessageBox.Show(selectedItem.ToString());
        }
        */
    }
}
