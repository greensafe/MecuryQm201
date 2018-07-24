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

            newTestClt =
            Utility.getNewTestTargetDataFromXDP((DataSourceProvider)this.FindResource("newTargetData"));
            NewTargetDgd.DataContext = newTestClt;

            standardSampleClt = 
            Utility.getStandardTargetDataFromXDP((DataSourceProvider)this.FindResource("standardSampleData"));
            standardSampleDgd.DataContext = standardSampleClt;
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {

            //初始化RS232驱动，注册回调函数
            ComDevice = new SerialPort();
            ComDevice.DataReceived += new SerialDataReceivedEventHandler(Com_DataReceived);

            drawWave_simulate(1);
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
        public void drawWave_simulate(int number)
        {
            
            var x = Enumerable.Range(0, 1001).Select(i => i / 10.0).ToArray();
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
                    newTestClt.RemoveAt(NewTargetDgd.SelectedIndex);
                    break;
                //标样测试
                case 1:
                    standardSampleClt.RemoveAt(standardSampleDgd.SelectedIndex);
                    break;
                default:

                    break;
            }

            
        }
    }
}
