using SilverTest.libs;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        private SerialPort alarmComDevice = null;

        public object MessageBoxButtons { get; private set; }
        public object MessageBoxIcon { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void dataComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ComDevice = new SerialPort();
            alarmComDevice = new SerialPort();
            string[] a = SerialPort.GetPortNames();
            for (int i = 0; i < a.Length; i++)
            {
                datacomporCombo.Items.Add(a[i]);
            }
            datacomporCombo.SelectedIndex = 0;
            for(int i = 0; i < a.Length; i++)
            {
                alarmcomporCombo.Items.Add(a[i]);
            }
            alarmcomporCombo.SelectedIndex = 0;

            datarate.Items.Add("300");
            datarate.Items.Add("600");
            datarate.Items.Add("1200");
            datarate.Items.Add("2400");
            datarate.Items.Add("4800");
            datarate.Items.Add("9600");
            datarate.Items.Add("19200");
            datarate.Items.Add("38400");
            datarate.Items.Add("43000");
            datarate.Items.Add("56000");
            datarate.Items.Add("57600");
            datarate.Items.Add("115200");
            datarate.SelectedIndex = 5;


            datacbbParity.Items.Add("None");
            datacbbParity.SelectedIndex = 0;

            datacbbDataBits.Items.Add("8");
            datacbbDataBits.Items.Add("7");
            datacbbDataBits.Items.Add("6");
            datacbbDataBits.SelectedIndex = 0;

            datacbbStopBits.Items.Add("1");
            datacbbStopBits.Items.Add("2");
            datacbbStopBits.Items.Add("3");
            datacbbStopBits.SelectedIndex = 0;

            //alarm
            alarmrate.Items.Add("300");
            alarmrate.Items.Add("600");
            alarmrate.Items.Add("1200");
            alarmrate.Items.Add("2400");
            alarmrate.Items.Add("4800");
            alarmrate.Items.Add("9600");
            alarmrate.Items.Add("19200");
            alarmrate.Items.Add("38400");
            alarmrate.Items.Add("43000");
            alarmrate.Items.Add("56000");
            alarmrate.Items.Add("57600");
            alarmrate.Items.Add("115200");
            alarmrate.SelectedIndex = 5;


            alarmcbbParity.Items.Add("None");
            alarmcbbParity.SelectedIndex = 0;

            alarmcbbDataBits.Items.Add("8");
            alarmcbbDataBits.Items.Add("7");
            alarmcbbDataBits.Items.Add("6");
            alarmcbbDataBits.SelectedIndex = 0;

            alarmcbbStopBits.Items.Add("1");
            alarmcbbStopBits.Items.Add("2");
            alarmcbbStopBits.Items.Add("3");
            alarmcbbStopBits.SelectedIndex = 0;


            ComDevice.DataReceived += new SerialDataReceivedEventHandler(Com_DataReceived);//绑定事件
            alarmComDevice.DataReceived += new SerialDataReceivedEventHandler(Com_alarmDataReceived);
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
            Dispatcher.BeginInvoke(new Action(() =>
            {
                dataoutputData.Text += sb.ToString();
                datawordcount.Text = dataoutputData.Text.Length.ToString();
                //outputData.ScrollToEnd();
            }));
        
        }

        private void Com_alarmDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] ReDatas = new byte[alarmComDevice.BytesToRead];
            alarmComDevice.Read(ReDatas, 0, ReDatas.Length);//读取数据

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ReDatas.Length; i++)
            {
                sb.AppendFormat("{0:x2}" + " ", ReDatas[i]);
            }

            //outputData.Text = ReDatas.ToString();
            Dispatcher.BeginInvoke(new Action(() =>
            {
                alarmoutputData.Text += sb.ToString();
                datawordcount.Text = alarmoutputData.Text.Length.ToString();
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

        private void dataopenPort_Click(object sender, RoutedEventArgs e)
        {
            if (ComDevice.IsOpen == false)
            {
                ComDevice.PortName = datacomporCombo.SelectedItem.ToString();
                ComDevice.BaudRate = Convert.ToInt32(datarate.SelectedItem.ToString());
                ComDevice.Parity = (Parity)Convert.ToInt32(datacbbParity.SelectedIndex.ToString());
                ComDevice.DataBits = Convert.ToInt32(datacbbDataBits.SelectedItem.ToString());
                ComDevice.StopBits = (StopBits)Convert.ToInt32(datacbbStopBits.SelectedItem.ToString());
                try
                {
                    ComDevice.Open();
                    dataoutputData.Text += "  "+ComDevice.PortName + "  :";
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
        private void alarmopenPort_Click(object sender, RoutedEventArgs e)
        {
            if (SerialDriver.GetDriver().alarm_isOpen() == false)
            {
                SerialDriver.GetDriver().alarm_portname = alarmcomporCombo.SelectedItem.ToString();
                SerialDriver.GetDriver().alarm_rate = Convert.ToInt32(alarmrate.SelectedItem.ToString());
                SerialDriver.GetDriver().alarm_parity = Convert.ToInt32(alarmcbbParity.SelectedIndex.ToString());
                SerialDriver.GetDriver().alarm_databits = Convert.ToInt32(alarmcbbDataBits.SelectedItem.ToString());
                SerialDriver.GetDriver().alarm_stopbits = Convert.ToInt32(alarmcbbStopBits.SelectedItem.ToString());
                try
                {
                    SerialDriver.GetDriver().alarm_Open(SerialDriver.GetDriver().alarm_portname, SerialDriver.GetDriver().alarm_rate, SerialDriver.GetDriver().alarm_parity,
                        SerialDriver.GetDriver().alarm_databits, SerialDriver.GetDriver().alarm_stopbits);
                    
                    alarmoutputData.Text += "  " + alarmComDevice.PortName + "  :";
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
                    SerialDriver.GetDriver().alarm_Close();
                    MessageBox.Show("打开端口已经被关闭");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "无法关闭");
                };
            }
        }


        private void datasendCommand_Click_1(object sender, RoutedEventArgs e)
        {
            //构造数据
            byte[] sendData = null;
            sendData = Encoding.ASCII.GetBytes(datatxtSendData.Text.Trim());
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
        private void alarmsendCommand_Click_1(object sender, RoutedEventArgs e)
        {
            //构造数据
            byte[] sendData = null;
            sendData = Encoding.ASCII.GetBytes(alarmtxtSendData.Text.Trim());
       
            if (this.alarmSendData(sendData))//发送数据成功计数  
            {
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

        private bool alarmSendData(byte[] data)
        {
            if (alarmComDevice.IsOpen)
            {
                try
                {
                    alarmComDevice.Write(data, 0, data.Length);//发送数据  
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

        private void datacloseCom_Click(object sender, RoutedEventArgs e)
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
        private void alarmcloseCom_Click(object sender, RoutedEventArgs e)
        {
            if (alarmComDevice.IsOpen)
            {
                try
                {
                    alarmComDevice.Close();
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

        private void dataButton_Click(object sender, RoutedEventArgs e)
        {
            dataoutputData.Text = "";
        }
        private void alarmButton_Click(object sender, RoutedEventArgs e)
        {
            dataoutputData.Text = "";
        }


        private void datamakeDataBtn_Click(object sender, RoutedEventArgs e)
        {
            FileStream afile = new FileStream("clip.txt", FileMode.Append);
            StreamWriter writer = new StreamWriter(afile);

            FileStream rfile = new FileStream("realtestdata_fr2.txt", FileMode.Open);
            StreamReader reader = new StreamReader(rfile);
            string data = reader.ReadToEnd();
            string[] r = Regex.Split(data, "I");
            for (int i = 0; i < r.Length; i++)
            {

                writer.Write(r[i]);
                writer.Write("I");
                writer.Write("\r\n");
            }

            writer.Close();
            afile.Close();
            reader.Close();
            rfile.Close();


        }
        private void alarmmakeDataBtn_Click(object sender, RoutedEventArgs e)
        {
            FileStream afile = new FileStream("clip.txt", FileMode.Append);
            StreamWriter writer = new StreamWriter(afile);

            FileStream rfile = new FileStream("realtestdata_fr2.txt", FileMode.Open);
            StreamReader reader = new StreamReader(rfile);
            string data = reader.ReadToEnd();
            string[] r = Regex.Split(data, "I");
            for (int i = 0; i < r.Length; i++)
            {

                writer.Write(r[i]);
                writer.Write("I");
                writer.Write("\r\n");
            }

            writer.Close();
            afile.Close();
            reader.Close();
            rfile.Close();

        }


        private void OnDataTabSelected(object sender, RoutedEventArgs e)
        {

        }

        private void OnAlarmTabSelected(object sender, RoutedEventArgs e)
        {

        }

        private void Testalarmobject_Click(object sender, RoutedEventArgs e)
        {
            AlarmRedObject.GetAlarmObject().NeedAlarm();
        }

        private void OntestbSelected(object sender, RoutedEventArgs e)
        {

        }

        private void Makevicedatabtn_Click(object sender, RoutedEventArgs e)
        {
            FileStream afile = new FileStream("clip.txt", FileMode.Append);
            StreamWriter writer = new StreamWriter(afile);
            string str;
            int iend = 0;
            char[] desstr = new char[11];
            FileStream rfile = new FileStream("realtestdata_fr2.txt", FileMode.Open);
            StreamReader reader = new StreamReader(rfile);
            
            while(reader.EndOfStream == false)
            {
                str = reader.ReadLine();
                if (str.Length != 11) continue;
                for(int i = 0; i < 11; i++)
                {
                    desstr[i] = '\0';
                }
                for(int i=0; i < desstr.Length; i++)
                {
                    desstr[i] = str[i];
                }
                writer.Write(desstr);
                writer.Write("\r\n");
                desstr[1] = 'U';
                switch(iend % 4)
                {
                    case 0:
                        writer.Write("FU00310H04I\r\n");
                        break;
                    case 1:
                        writer.Write("FU00311H05I\r\n");
                        break;
                    case 2:
                        writer.Write("FU00312H06I\r\n");
                        break;
                    case 3:
                        writer.Write("FU00313H07I\r\n");
                        break;
                }
                iend++;
            }

            writer.Close();
            afile.Close();
            reader.Close();
            rfile.Close();
        }

        private void DatamakeZeroDataBtn_Click(object sender, RoutedEventArgs e)
        {
            FileStream afile = new FileStream("clip.txt", FileMode.Append);
            StreamWriter writer = new StreamWriter(afile);

            FileStream rfile = new FileStream("realtestdata_fr3.txt", FileMode.Open);
            StreamReader reader = new StreamReader(rfile);
            string data = reader.ReadToEnd();
            string[] r = Regex.Split(data, "I");
            for (int i = 0; i < 7506; i++)
            {
                writer.Write("FG00000H00I");
                //writer.Write(r[i]);
                //writer.Write("I");
                writer.Write("\r\n");
            }

            writer.Close();
            afile.Close();
            reader.Close();
            rfile.Close();

        }
    }
}