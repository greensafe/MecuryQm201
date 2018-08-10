using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
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

    public partial class MainWindow : Window
    {
        //演示代码
        int seconds;
        DispatcherTimer demoTimer = new DispatcherTimer();

        //使用DynamicDataDisplay控件显示波形
        private ObservableDataSource<Point> realCptDs = new ObservableDataSource<Point>();
        private DispatcherTimer realCptTimer = new DispatcherTimer();
        bool mode = true;
        Random rd = new Random();
        private int currentSecond = 0;
        int xaxis = 0;
        int yaxis = 0;
        int group = 200;//组距
        Queue<int> q = new Queue<int>();

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
            realCptTimer.Interval = TimeSpan.FromSeconds(1);
            realCptTimer.Tick += realTck;
            //realCptTimer.IsEnabled = true;

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
            demoTimer.Interval =  new TimeSpan(0, 0, 0, 1);  //1 seconds
            demoTimer.Tick += new EventHandler(timeCycle);
        }

        //演示代码
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

        private void realTck(object sender, EventArgs e)
        {
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
        }


        private void window_Loaded(object sender, RoutedEventArgs e)
        {

            //初始化RS232驱动，注册回调函数
            ComDevice = new SerialPort();
            ComDevice.DataReceived += new SerialDataReceivedEventHandler(Com_DataReceived);

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
            string re ="";
            for(int i = 0; i< ReDatas.Length; i++)
            {
                re += (char)ReDatas[i];
            }
            Console.WriteLine("received data: " + re);

            //DataFormater.getDataFormater().GetDot(ReDatas);
            
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ReDatas.Length; i++)
            {
                sb.AppendFormat("{0:x2}" + " ", ReDatas[i]);
            }
            


            Dispatcher.Invoke(new Action(() =>
            {
                rTxt.Text = sb.ToString();
            }));

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

        /*
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
            switch (sampletab.SelectedIndex)
            {
                //新样测试
                case 0:
                    newTestClt.Add(new NewTestTarget());
                    break;
                //标样测试
                case 1:
                    standardSampleClt.Add(new StandardSample());
                    break;
                default:

                    break;
            }

        }

        private void DelRowBtn_Click(object sender, RoutedEventArgs e)
        {

            switch (sampletab.SelectedIndex)
            {
                //新样测试
                case 0:
                    if (NewTargetDgd.SelectedIndex < 0)
                        return;
                    newTestClt.RemoveAt(NewTargetDgd.SelectedIndex);
                    break;
                //标样测试
                case 1:
                    if (standardSampleDgd.SelectedIndex < 0)
                        return;
                    standardSampleClt.RemoveAt(standardSampleDgd.SelectedIndex);
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

        private void standardCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            StandardSample asample = e.AddedItems[0] as StandardSample;
            aTxb.Text = asample.A;
            bTxb.Text = asample.B;

            //如果有平均值则计算汞浓度
            int rowNo = NewTargetDgd.SelectedIndex;
            if (rowNo < 0 || newTestClt[rowNo].AverageValue is null)
                return;
            if (newTestClt[rowNo].AverageValue != "")
            {

                //newTestClt[rowNow].Density = "";
                int t1 = int.Parse(newTestClt[rowNo].ResponseValue1);
                double t2 = double.Parse(asample.A);
                double t3 = double.Parse(asample.B);
                double d = (t1 * t2 * t3 / 1000);
                newTestClt[rowNo].Density = d.ToString();
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
            switch(startTestBtn.Content as string)
            {
                case "开始测试":
                    statusBtn.Visibility = Visibility.Visible;
                    AnimatedColorButton.Visibility = Visibility.Visible;
                    demoTimer.Start();
                    realCptTimer.Start();
                    startTestBtn.Content = "停止测试";
                    break;
                case "停止测试":
                    statusBtn.Visibility = Visibility.Hidden;
                    AnimatedColorButton.Visibility = Visibility.Hidden;
                    demoTimer.Stop();
                    realCptTimer.Stop();
                    startTestBtn.Content = "开始测试";
                    break;
                default:

                    break;
            }
            int selectedItem = NewTargetDgd.SelectedIndex;

            NewTargetDgd.IsEnabled = true;
            
        }

        private void exitBtn_Click(object sender, RoutedEventArgs e)
        {
            
            this.Close();

        }

        private void testBtn_Click(object sender, RoutedEventArgs e)
        {
            SerialDriver.GetDriver().OnReceived(Com_DataReceived);
            ProduceFakeData pfd = new ProduceFakeData("实际数据.txt");
            pfd.Send(1);
            ;
            /*
            if(DataFormater.getDataFormater().getStatus() == DataFormater.ErrorOccur.NONE)
            {
                DataFormater.getDataFormater().GetDot(new byte[] { 1,2,3,4,5});
            }
            */
        }

        private void debugBtn_Click(object sender, RoutedEventArgs e)
        {
            DataFormater.getDataFormater().GetDots();
            //byte[] r = DataFormater.getDataFormater().GetRawData();
            ;
            //DataFormater.getDataFormater().SaveToFile("raw.txt");
            
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
