using System;
using System.Collections.Generic;
using System.IO;
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

namespace SerialPortLab
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private SerialPort ComDevice = null;

        public object MessageBoxButtons { get; private set; }
        public object MessageBoxIcon { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ComDevice = new SerialPort();
            string[] a = SerialPort.GetPortNames();
            comporCombo.Items.Add(a[0]);
            comporCombo.Items.Add(a[1]);
            comporCombo.SelectedIndex = 0;

            rate.Items.Add("300");
            rate.Items.Add("600");
            rate.Items.Add("1200");
            rate.Items.Add("2400");
            rate.Items.Add("4800");
            rate.Items.Add("9600");
            rate.Items.Add("19200");
            rate.Items.Add("38400");
            rate.Items.Add("43000");
            rate.Items.Add("56000");
            rate.Items.Add("57600");
            rate.Items.Add("115200");
            rate.SelectedIndex = 5;


            cbbParity.Items.Add("None");
            cbbParity.SelectedIndex = 0;

            cbbDataBits.Items.Add("8");
            cbbDataBits.Items.Add("7");
            cbbDataBits.Items.Add("6");
            cbbDataBits.SelectedIndex = 0;

            cbbStopBits.Items.Add("1");
            cbbStopBits.Items.Add("2");
            cbbStopBits.Items.Add("3");
            cbbStopBits.SelectedIndex = 0;

            ComDevice.DataReceived += new SerialDataReceivedEventHandler(Com_DataReceived);//绑定事件
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Com_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] ReDatas = new byte[ComDevice.BytesToRead];
            ComDevice.Read(ReDatas, 0, ReDatas.Length);//读取数据

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ReDatas.Length; i++)
            {
                sb.AppendFormat("{0:x2}" + " ", ReDatas[i]);
            }

            //outputData.Text = ReDatas.ToString();
            Dispatcher.Invoke(new Action(() =>
            {
                outputData.Text += sb.ToString();
                wordcount.Text = outputData.Text.Length.ToString();
                //outputData.ScrollToEnd();
            }));
        
        }

        /*
        private void sendCommand_Click(object sender, RoutedEventArgs e)
        {
            if (ComDevice.IsOpen == false)
            {
                ;
            }
            else
            {
                try
                {
                    ComDevice.Close();
               
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误");
                };
            }
        }
        */

        private void openPort_Click(object sender, RoutedEventArgs e)
        {
            if (ComDevice.IsOpen == false)
            {
                ComDevice.PortName = comporCombo.SelectedItem.ToString();
                ComDevice.BaudRate = Convert.ToInt32(rate.SelectedItem.ToString());
                ComDevice.Parity = (Parity)Convert.ToInt32(cbbParity.SelectedIndex.ToString());
                ComDevice.DataBits = Convert.ToInt32(cbbDataBits.SelectedItem.ToString());
                ComDevice.StopBits = (StopBits)Convert.ToInt32(cbbStopBits.SelectedItem.ToString());
                try
                {
                    ComDevice.Open();
                    MessageBox.Show("打开成功");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "打开发生错误");
                    return;
                }

            }
            else
            {
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

        private void sendCommand_Click_1(object sender, RoutedEventArgs e)
        {
            //构造数据
            byte[] sendData = null;
            sendData = Encoding.ASCII.GetBytes(txtSendData.Text.Trim());
            /*
            if (rbtnSendHex.Checked)
            {
                sendData = strToHexByte(txtSendData.Text.Trim());
            }
            else if (rbtnSendASCII.Checked)
            {
                sendData = Encoding.ASCII.GetBytes(txtSendData.Text.Trim());
            }
            else if (rbtnSendUTF8.Checked)
            {
                sendData = Encoding.UTF8.GetBytes(txtSendData.Text.Trim());
            }
            else if (rbtnSendUnicode.Checked)
            {
                sendData = Encoding.Unicode.GetBytes(txtSendData.Text.Trim());
            }
            else
            {
                sendData = Encoding.ASCII.GetBytes(txtSendData.Text.Trim());
            }
            */
            if (this.SendData(sendData))//发送数据成功计数  
            {
                /*
                lblSendCount.Invoke(new MethodInvoker(delegate
                {
                    lblSendCount.Text = (int.Parse(lblSendCount.Text) + txtSendData.Text.Length).ToString();
                }));
                */
            }
            else
            {

            }
        }
        private bool SendData(byte[] data)
        {
            if (ComDevice.IsOpen)
            {
                try
                {
                    ComDevice.Write(data, 0, data.Length);//发送数据  
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("发送数据错误");
                }
            }
            else
            {
                MessageBox.Show("串口未打开");
            }
            return false;
        }

        private void closeCom_Click(object sender, RoutedEventArgs e)
        {
            if (ComDevice.IsOpen)
            {
                try
                {
                    ComDevice.Close();
                    MessageBox.Show("关闭串口成功");
                }
                catch
                {
                    MessageBox.Show("无法关闭串口");
                }
            }
            else
            {
                MessageBox.Show("没有打开的串口");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            outputData.Text = "";
        }

        private void makeDataBtn_Click(object sender, RoutedEventArgs e)
        {
            FileStream afile = new FileStream("实际数据.txt", FileMode.Append);
            StreamWriter writer = new StreamWriter(afile);

            FileStream rfile = new FileStream("clip.txt", FileMode.Open);
            StreamReader reader = new StreamReader(rfile);
            string data = reader.ReadToEnd();

            for (int i = 0; i < 5000; i++)
            {

                writer.Write(data);
            }

            writer.Close();
            afile.Close();
            reader.Close();
            rfile.Close();


        }
    }
}