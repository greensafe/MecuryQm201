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
using System.Xml;

/*
 * 变量尾缀约定
 * Clt = Collection
 * XDP = XmlDataProvider
 * Dgd = DataGrid 
 * Bbx = GroupBox
 * Cmb = ComboBox
 * Txb = TextBox
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
        private SerialPort ComDevice = null;
        private ObservableCollection<NewTestTarget> newTestClt;
        private ObservableCollection<StandardSample> standardSampleClt;

        public MainWindow()
        {
            InitializeComponent();

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
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {

            //初始化RS232驱动，注册回调函数
            ComDevice = new SerialPort();
            ComDevice.DataReceived += new SerialDataReceivedEventHandler(Com_DataReceived);

            drawWave_simulate(1001);
        }



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

            }));

        }

        //模拟数据，画波形
        //number = 1001 
        private void drawWave_simulate(int number)
        {

            var x = Enumerable.Range(0, number).Select(i => i / 10.0).ToArray();
            var y = x.Select(v => Math.Abs(v) < 1e-10 ? 1 : Math.Sin(v) / v).ToArray();
            linegraph.Plot(x, y);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
            StandardSample asample = e.AddedItems[0] as StandardSample;
            aTxb.Text = asample.A;
            bTxb.Text = asample.B;

            //如果有平均值则计算汞浓度
            int rowNo = NewTargetDgd.SelectedIndex;
            if (newTestClt[rowNo].AverageValue is null)
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
            NewTargetDgd.DataContext = null;
            NewTargetDgd.DataContext = newTestClt;

            //演示数据，模拟波形
            switch (asample.Code)
            {
                case "s1001":
                    drawWave_simulate(1001);
                    break;
                case "s1002":
                    drawWave_simulate(150);
                    break;
                case "s1003":
                    drawWave_simulate(2000);
                    break;
                case "s1004":
                    drawWave_simulate(1500);
                    break;
                default:

                    break;
            }
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
                    startTestBtn.Content = "停止测试";
                    break;
                case "停止测试":
                    statusBtn.Visibility = Visibility.Hidden;
                    AnimatedColorButton.Visibility = Visibility.Hidden;
                    startTestBtn.Content = "开始测试";
                    break;
                default:

                    break;
            }
            int selectedItem = NewTargetDgd.SelectedIndex;

            NewTargetDgd.IsEnabled = true;

            
        }
    }
}
