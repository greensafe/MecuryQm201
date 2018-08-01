using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace SilverTest.libs
{
    public class Utility
    {
        //从资源中读取数据新样xml数据，转换为ObservableCollection
        //param: provider - 数据源
        static public ObservableCollection<NewTestTarget> getNewTestTargetDataFromXDP(DataSourceProvider provider)
        {
            ObservableCollection<NewTestTarget> newTargets = new ObservableCollection<NewTestTarget>();
            var data = provider.Data as IEnumerable;
            foreach (XmlElement item in data)
            {
                NewTestTarget atarget = new NewTestTarget();
                item.ToString();
                foreach (XmlElement ee in item)
                {
                    switch (ee.Name)
                    {
                        case "NewName":
                            atarget.NewName = ee.InnerText;
                            break;
                        case "Code":
                            atarget.Code = ee.InnerText;
                            break;
                        case "Density":
                            atarget.Density = ee.InnerText;
                            break;
                        case "Weight":
                            atarget.Weight = ee.InnerText;
                            break;
                        case "Place":
                            atarget.Place = ee.InnerText;
                            break;
                        case "LiquidSize":
                            atarget.LiquidSize = ee.InnerText;
                            break;
                        case "ResponseValue1":
                            atarget.ResponseValue1 = ee.InnerText;
                            break;
                        case "ResponseValue2":
                            atarget.ResponseValue2 = ee.InnerText;
                            break;
                        case "ResponseValue3":
                            atarget.ResponseValue3 = ee.InnerText;
                            break;
                        case "AverageValue":
                            atarget.AverageValue = ee.InnerText;
                            break;
                        default:
                            break;
                    }
                }
                newTargets.Add(atarget);
                ;
            }
            return newTargets;

        }

        //从资源中读取标样xml数据，转换为ObservableCollection
        //param: provider - 数据源
        static public ObservableCollection<StandardSample> getStandardTargetDataFromXDP(DataSourceProvider provider)
        {
            ObservableCollection<StandardSample> standardSamples = new ObservableCollection<StandardSample>();
            var data = provider.Data as IEnumerable;
            foreach (XmlElement item in data)
            {
                StandardSample astandard = new StandardSample();
                foreach (XmlElement ee in item)
                {
                    switch (ee.Name)
                    {
                        case "SampleName":
                            astandard.SampleName = ee.InnerText;
                            break;
                        case "Code":
                            astandard.Code = ee.InnerText;
                            break;
                        case "Density":
                            astandard.Density = ee.InnerText;
                            break;
                        case "Weight":
                            astandard.Weight = ee.InnerText;
                            break;
                        case "ProviderCompany":
                            astandard.ProviderCompany = ee.InnerText;
                            break;
                        case "Place":
                            astandard.Place = ee.InnerText;
                            break;
                        case "BuyDate":
                            astandard.BuyDate = ee.InnerText;
                            break;
                        case "A":
                            astandard.A = ee.InnerText;
                            break;
                        case "B":
                            astandard.B = ee.InnerText;
                            break;
                        default:
                            break;
                    }
                }
                standardSamples.Add(astandard);
                ;
            }
            return standardSamples;

        }

        /* 从新样xml文件中读取数据，转换为ObservableCollection
         * 
         */
        static public ObservableCollection<NewTestTarget> getNewTestTargetDataFromXml(string filename)
        {
            ObservableCollection<NewTestTarget> newTestTargetData = new ObservableCollection<NewTestTarget>();

            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<NewTestTarget>));
            using (FileStream stream = new FileStream(filename, FileMode.Open))
            {
                IEnumerable<NewTestTarget> xmldata = (IEnumerable<NewTestTarget>)serializer.Deserialize(stream);
                foreach (NewTestTarget item in xmldata)
                {
                    newTestTargetData.Add(item);
                }
            }
            return newTestTargetData;
        }

        /* 从标样xml文件中读取数据，转换为ObservableCollection
         * 
         */
        static public ObservableCollection<StandardSample> getStandardTargetDataFromXml(string filename)
        {
            ObservableCollection<StandardSample> standData = new ObservableCollection<StandardSample>();

            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<StandardSample>));
            using (FileStream stream = new FileStream(filename, FileMode.Open))
            {
                IEnumerable<StandardSample> xmldata = (IEnumerable<StandardSample>)serializer.Deserialize(stream);
                foreach (StandardSample item in xmldata)
                {
                    standData.Add(item);
                }
            }
            return standData;
        }

        /*将新样标签保存到新样xml文件之中
         *param: collections - 待保存数据 
         *       filename - 文件名
         */
        static public class SaveToNewXmlFileCls
        {
            static public void SaveToNewXmlFile(ICollection collections, string filename)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<NewTestTarget>));

                using (FileStream stream = new FileStream(filename, FileMode.Create))
                {
                    serializer.Serialize(stream, collections);
                }

                /*
                foreach (object item in collections)
                {
                    switch (type)
                    {
                        case 1: //表示 NewTestTarget
                            NewTestTarget a = (item as NewTestTarget);
                            break;
                        case 2: //表示 StandardSample
                            StandardSample b = (item as StandardSample);
                            break;
                        default:
                            return;

                    }
                }
                */
            }
        }

        /*将标样标签数据保存到标样xml文件之中
         *param: collections - 待保存数据 
         *       filename - 文件名
         */
        static public class SaveToStandardXmlFileCls
        {
            static public void SaveToStandardXmlFile(ICollection collections, string filename)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<StandardSample>));

                using (FileStream stream = new FileStream(filename, FileMode.Create))
                {
                    serializer.Serialize(stream, collections);
                }
            }
        }

        static public void Save2excel(DataGrid dataGrid)
        {
            string fileName = "";
            string saveFileName = "";
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.DefaultExt = "xlsx";
            saveDialog.Filter = "Excel 文件|*.xlsx";
            saveDialog.FileName = fileName;
            saveDialog.ShowDialog();
            saveFileName = saveDialog.FileName;
            if (saveFileName.IndexOf(":") < 0) return;  //被点了取消
            Microsoft.Office.Interop.Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();
            if (xlApp == null)
            {
                System.Windows.MessageBox.Show("无法创建Excel对象，您可能未安装Excel");
                return;
            }
            Microsoft.Office.Interop.Excel.Workbooks workbooks = xlApp.Workbooks;
            Microsoft.Office.Interop.Excel.Workbook workbook = workbooks.Add(Microsoft.Office.Interop.Excel.XlWBATemplate.xlWBATWorksheet);
            Microsoft.Office.Interop.Excel.Worksheet worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Worksheets[1]; //取得sheet1

            //写入行
            for (int i = 0; i < dataGrid.Columns.Count; i++)
            {
                worksheet.Cells[1, i + 1] = dataGrid.Columns[i].Header;
            }
            for (int r = 0; r < dataGrid.Items.Count; r++)
            {

                for (int i = 0; i < dataGrid.Columns.Count - 1; i++) //暂时不处理新样选择标样列，所以标样最后一列也不保存
                {
                    worksheet.Cells[r + 2, i + 1] = (dataGrid.Columns[i].GetCellContent(dataGrid.Items[r]) as TextBlock).Text;
                }

            }
            worksheet.Columns.EntireColumn.AutoFit();
            MessageBox.Show(fileName + "保存成功");
            if (saveFileName != "")
            {
                try
                {
                    workbook.Saved = true;
                    workbook.SaveCopyAs(saveFileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("导出文件可能正在被打断!" + ex.Message);
                }

            }
            xlApp.Quit();
            GC.Collect();

        }


        /*
        static public void GenerateExcel(DataTable DtIN)
        {
            try
            {
                excel = new Microsoft.Office.Interop.Excel.Application();
                excel.DisplayAlerts = false;
                excel.Visible = false;
                workBook = excel.Workbooks.Add(Type.Missing);
                workSheet = (Microsoft.Office.Interop.Excel.Worksheet)workBook.ActiveSheet;
                workSheet.Name = "LearningExcel";
                System.Data.DataTable tempDt = DtIN;
                dgExcel.ItemsSource = tempDt.DefaultView;
                workSheet.Cells.Font.Size = 11;
                int rowcount = 1;
                for (int i = 1; i <= tempDt.Columns.Count; i++) //taking care of Headers.  
                {
                    workSheet.Cells[1, i] = tempDt.Columns[i - 1].ColumnName;
                }
                foreach (System.Data.DataRow row in tempDt.Rows) //taking care of each Row  
                {
                    rowcount += 1;
                    for (int i = 0; i < tempDt.Columns.Count; i++) //taking care of each column  
                    {
                        workSheet.Cells[rowcount, i + 1] = row[i].ToString();
                    }
                }
                cellRange = workSheet.Range[workSheet.Cells[1, 1], workSheet.Cells[rowcount, tempDt.Columns.Count]];
                cellRange.EntireColumn.AutoFit();
            }
            catch (Exception)
            {
                throw;
            }
        }
        */
    }

    /*
     * 串口驱动类，
     * 仅仅能生成一个对象
     * 
     * 使用顺序
     * GetDriver -> open->send->close->destroy
     *   GetDriver->open ->send->close可反复使用，均是使用驱动对象
     *   调用destroy后，当前的驱动对象被销毁
     *    
     */
    public class SerialDriver
    {
        static private SerialPort ComDevice = null;
        static private SerialDriver onlyone = null;

        private SerialDriver()
        {
            ComDevice = new SerialPort();
            Console.WriteLine("SerialDriver object is created");
        }

        //获取SerialDriver对象
        static public SerialDriver GetDriver()
        {
            if(onlyone == null)
            {
                onlyone = new SerialDriver();
            }
            return onlyone;
        }

        //读取端口收到的数据
        public byte[] Read()
        {
            byte[] ReDatas = new byte[ComDevice.BytesToRead]; 
            ComDevice.Read(ReDatas, 0, ReDatas.Length);
            return ReDatas;
            /*
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ReDatas.Length; i++)
            {
                sb.AppendFormat("{0:x2}" + " ", ReDatas[i]);
            }
            */
        }

        //设置数据接收处理函数
        public SerialDriver OnReceived(SerialDataReceivedEventHandler dlr)
        {
            ComDevice.DataReceived += new SerialDataReceivedEventHandler(dlr);
            return onlyone;
        }

        //打开端口
        public SerialDriver Open(string portname, int rate, int parity, int databits, int stopBits)
        {
            if (ComDevice.IsOpen == false)
            {
                ComDevice.PortName = portname;
                ComDevice.BaudRate = rate;
                ComDevice.Parity = (Parity)parity;
                ComDevice.DataBits = databits;
                ComDevice.StopBits = (StopBits)stopBits;
                try
                {
                    ComDevice.Open();
                    Console.WriteLine("打开成功");
                }
                catch (Exception ex)
                {
                    //Console.WriteLine("打开发生错误");
                    MessageBox.Show("打开发生错误");
                    return onlyone;
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
                    MessageBox.Show(ex.Message, "无法关闭已打开的端口");
                    return onlyone;
                };
            }

            return onlyone;
        }

        //判断端口是否打开
        public bool isOpen()
        {
            if (ComDevice == null || ComDevice.IsOpen == false)
                return false;
            else
                return true;
        }

        //关闭端口
        public SerialDriver Close()
        {
            if (ComDevice.IsOpen)
            {
                try
                {
                    ComDevice.Close();
                    Console.WriteLine("serial is closed!");
                }
                catch
                {
                    MessageBox.Show("关闭端口失败");
                }
            }
            else
            {
                MessageBox.Show("没有打开的端口");
            }
            return onlyone;
        }

        //向端口发送数据
        public bool Send(byte[] data)
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
                    Console.WriteLine("发送数据错误");
                }
            }
            else
            {
                Console.WriteLine("串口未打开");
            }
            return false;
        }

        //销毁端口
        public void destroy()
        {
            if ( ComDevice!=null && ComDevice.IsOpen)
            {
                ComDevice.Close();

            }
            ComDevice = null;
            onlyone = null;
        }
    }

    /*
     * <>开放工具 模拟机器数据
     *  从文件中读取数据发送到端口
     */
    public class ProduceFakeData
    {
        //context
        private DispatcherTimer readDataTimer;
        FileStream aFile;
        StreamReader sr;
        public int i = 100;
        int tickcount = 1;

        public ProduceFakeData(string filename)
        {
            readDataTimer = new DispatcherTimer();
            readDataTimer.Tick += new EventHandler(timeCycle);
            aFile = new FileStream(filename, FileMode.Open);
            sr = new StreamReader(aFile);

            SerialDriver.GetDriver().Open("COM1", 9600, 0, 8, 1);
            if (!SerialDriver.GetDriver().isOpen())
            {
                Console.WriteLine("======打开COM1失败======");
            }
        }

        //向端口间隔发送数据
        public void Send(int s)
        {

            readDataTimer.Interval = new TimeSpan(0, 0, 0, 0,s);  //1 seconds
            readDataTimer.Start();
        }

        public void timeCycle(object sender, EventArgs e)
        {
            tickcount++;
            string line;
            Console.WriteLine("tick " + tickcount.ToString());
            try
            {
                line = sr.ReadLine();
                // Read data in line by line.
                if (line != null)
                {
                    Console.WriteLine(line);
                    line = line + "\r\n";
                    //byte[] b = { 1, 2, 3, 4, 5 };
                    if (SerialDriver.GetDriver().isOpen())
                    {
                        SerialDriver.GetDriver().Send(Encoding.Default.GetBytes(line));
                    }
                    ;
                    //line = sr.ReadLine();
                }
                else
                {
                    //clear context
                    aFile.Close();
                    aFile = null;
                    sr.Close();
                    sr = null;
                    readDataTimer.Stop();
                    readDataTimer.IsEnabled = false;
                    readDataTimer = null;
                    Console.WriteLine("env released !");
                }
            }
            catch (Exception error)
            {
                //clear context
                aFile.Close();
                aFile = null;
                sr.Close();
                sr = null;
                readDataTimer.Stop();
                readDataTimer.IsEnabled = false;
                readDataTimer = null;
                Console.WriteLine("error occure, env is released !");
            }
            ;
        }

    }


    /*
     * data format from machine through serial port
     */
    public class MachineDataFormat
    {
        public string TagStart { get; set; }
        public int Response { get; set; }
        public string TagEnd { get; set; }
        public MachineDataFormat(int resp)
        {
            TagStart = "G";
            TagEnd = "H";
            Response = resp;
        }
        public MachineDataFormat()
        {
            TagStart = "G";
            Response = 0;
            TagEnd = "H";
        }
    }
}
