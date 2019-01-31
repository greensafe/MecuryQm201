using MathNet.Numerics;
using MathNet.Numerics.Statistics;
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
using static SilverTest.libs.DataFormater;
using static SilverTest.MainWindow;

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
            XmlSerializer serializer;
            try
            {
                serializer = new XmlSerializer(typeof(ObservableCollection<NewTestTarget>));


                using (FileStream stream = new FileStream(filename, FileMode.Open))
                {
                    IEnumerable<NewTestTarget> xmldata = (IEnumerable<NewTestTarget>)serializer.Deserialize(stream);
                    foreach (NewTestTarget item in xmldata)
                    {
                        newTestTargetData.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                return newTestTargetData;
            }
            return newTestTargetData;
        }

        /* 从标样xml文件中读取数据，转换为ObservableCollection
         * 
         */
        static public ObservableCollection<StandardSample> getStandardTargetDataFromXml(string filename)
        {
            ObservableCollection<StandardSample> standData = new ObservableCollection<StandardSample>();
            XmlSerializer serializer;
            try
            {
                serializer = new XmlSerializer(typeof(ObservableCollection<StandardSample>));

                using (FileStream stream = new FileStream(filename, FileMode.Open))
                {
                    IEnumerable<StandardSample> xmldata = (IEnumerable<StandardSample>)serializer.Deserialize(stream);
                    foreach (StandardSample item in xmldata)
                    {
                        standData.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                return standData;
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

        /*
         * 将datagrid数据保存到保存到xml文件中。为文件名附上时间以便于区分
         */
        static public void Save2XmlHistory(ICollection newclt, ICollection standardclt)
        {
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
            string standardfile = "history\\标样测试表格" + suffix + ".xml";
            string newfile = "history\\样本测试表格" + suffix + ".xml";

            SaveToNewXmlFileCls.SaveToNewXmlFile(newclt, newfile);
            SaveToStandardXmlFileCls.SaveToStandardXmlFile(standardclt, standardfile);
            MessageBox.Show("测试数据已保存到history目录中");
        }

        static public string ToDecimal(string a)
        {
            bool adot = false;
            if (a.Length == 0) return a;
            string stemp = null;

            for (int i = 0; i < a.Length; i++)
            {
                if ((a[i] < '0' || a[i] > '9') && (a[i] != '.')) //非数字
                {
                }
                else
                {
                    if (a[i] == '.')
                    {
                        if (adot == false)
                        {
                            stemp += a[i];
                            adot = true;
                        }
                    }
                    else
                    {
                        stemp += a[i];
                    }
                }
            }
            return stemp;
        }

        static public void Save2excel(DataGrid dataGrid)
        {
            int colcount = 0;
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


            //统计可见列的数目
            for (int i = 0; i < dataGrid.Columns.Count; i++)
            {
                if (dataGrid.Columns[i].Visibility == Visibility.Visible)
                    colcount++;
            }
            //写入列头
            int k = 0;
            for (int i = 0; i < dataGrid.Columns.Count; i++)
            {
                if (dataGrid.Columns[i].Visibility == Visibility.Visible)
                {
                    worksheet.Cells[1, k + 1] = dataGrid.Columns[i].Header;
                    k++;
                }
            }
            k = 0;
            for (int r = 0; r < dataGrid.Items.Count; r++)
            {

                for (int i = 0; i < dataGrid.Columns.Count - 1; i++) //暂时不处理新样选择标样列，所以标样最后一列也不保存
                {
                    if (dataGrid.Columns[i].Visibility == Visibility.Hidden ||
                        dataGrid.Columns[i].Visibility == Visibility.Collapsed)
                        continue;
                    if ((dataGrid.Columns[i].GetCellContent(dataGrid.Items[r]) is null)) {
                        worksheet.Cells[r + 2, k + 1] = "NaN";
                    }
                    else {
                        worksheet.Cells[r + 2, k + 1] = (dataGrid.Columns[i].GetCellContent(dataGrid.Items[r]) as TextBlock).Text;
                    }
                    k++;
                }
                k = 0;
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
         * 将byte流转化为数字。 每个byte中存放的是数字字符的二进制。
         * 比如 '2543', byte中存放的是0x32, 0x35, 0x34, 0x33
         * 大端字节序
         * @param
         *      data - 字节流
         *      start - 数据开始位置
         *      len - 数据长度
         *  
         */
        static public int ConvertStrToInt_Big(byte[] data, int start, int len)
        {
            int total = 0;

            if (len > data.Length)
                return -1;
            for (int i = 0; i < len; i++)
            {
                total += data[start + i] - 0x30;
                total *= 10;
            }
            return total /= 10;
        }

        /*
          * 将byte流转化为数字。 每个byte中存放的是数字字符的二进制。
          * 比如 '2543', byte中存放的是0x32, 0x35, 0x34, 0x33
          * 小端字节序
          * @param
          *      data - 字节流
          *      start - 数据开始位置
          *      len - 数据长度
          *  
          */
        static public int ConvertStrToInt_Little(byte[] data, int start, int len)
        {
            int total = 0;

            if (len > data.Length)
                return -1;
            for (int i = len - 1; i >= 0; i--)
            {
                total += data[start + i] - 0x30;
                total *= 10;
            }
            return total /= 10;
        }

        /*
         * 计算响应值
         * @param
         *    start_abs - dots中的起始位， 
         *    end_abs - dots中的结束位
         *    
         */
        static public int ComputeResponseValue(int start_abs, int end_abs)
        {
            //检查参数

            int max = 0;
            Collection<ADot> dots = DotManager.GetDotManger().GetDots();

            if (dots.Count <= 0)
                return -1;

            for (int i = start_abs; i < end_abs; i++)
            {
                if (dots[i].Rvalue > max)
                {
                    max = dots[i].Rvalue;
                }
            }

            return max;
        }

        /*
         * 计算相关截距，斜率
         * @param
         *  x - x轴数据
         *  y - y轴数据
         */
        static public void ComputeAB(out double a, out double b, double[] x, double[] y)
        {
            //double[] ydata = new double[] { 42.0, 43.5, 45.0, 45.5, 45.0, 47.5, 49.0, 53.0, 50.0, 55.0, 55.0, 60.0 };
            //double[] xdata = new double[] { 0.10, 0.11, 0.12, 0.13, 0.14, 0.15, 0.16, 0.17, 0.18, 0.20, 0.21, 0.23 };
            if (x.Length <= 1)
            {
                a = 0;
                b = 0;
                return;
            }
            Tuple<double, double> p = Fit.Line(x, y);
            a = p.Item2;
            b = p.Item1;
        }

        /*
         * 计算相关系数
         * 
         */
        static public double ComputeR(double[] x, double[] y)
        {
            return Correlation.Pearson(x, y);
        }

        /*
         * 从config.xml取值
         * @param
         *  xpath - 定位元素。 比如
         *              descendant::Ratio
         */
        static public string GetValueFrXml(string xpath, string propertyname)
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(@"config\config.xml");

            XmlElement root = xdoc.DocumentElement;
            XmlNode node = xdoc.SelectSingleNode(xpath);
            //XmlNode node = xdoc.SelectSingleNode("/config/QM201H/response/compute");
            //XmlNode node = xdoc.SelectSingleNode("/config/response/compute/R3");
            //xdoc.RemoveAll();
            xdoc = null;
            string t = node.Attributes[propertyname].Value;

            return t;

        }

        /*
         * 写config.xml
         * @param
         *  xpath - 定位元素。 比如
         *              descendant::Ratio
         */
        static public void SetValueToXml(string xpath, string propertyname, string v)
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(@"config\config.xml");

            XmlElement root = xdoc.DocumentElement;
            XmlNode node = xdoc.SelectSingleNode(xpath);
            //XmlNode node = xdoc.SelectSingleNode("/config/QM201H/response/compute");
            //XmlNode node = xdoc.SelectSingleNode("/config/response/compute/R3");
            node.Attributes[propertyname].Value = v;
            xdoc.Save(@"config\config.xml");
            xdoc = null;
        }


        /*
         * 面积积分
         * 初略算法，寻求更加高级的算法
         * @param
         *      start_abs 起始位置
         *      end_abs 结束位置
         *      
         */
        static public double Integration(Collection<ADot> dots, int start_abs, int end_abs, double ratio)
        {
            double t = 0;

            for (int i = start_abs; i < end_abs; i++)
            {
                t += dots[i].Rvalue * 1;
            }

            return (t / (end_abs - start_abs)) * ratio;
        }

        /*
         * CRC 16位校验
         */
        //CRC高位校验码checkCRCHigh
        private static byte[] ArrayCRCHigh =
        {
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
            0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
            0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
            0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40,
            0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
            0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40,
            0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
            0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40,
            0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
            0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40
        };

        //CRC地位校验码checkCRCLow
        private static byte[] checkCRCLow =
        {
            0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06,
            0x07, 0xC7, 0x05, 0xC5, 0xC4, 0x04, 0xCC, 0x0C, 0x0D, 0xCD,
            0x0F, 0xCF, 0xCE, 0x0E, 0x0A, 0xCA, 0xCB, 0x0B, 0xC9, 0x09,
            0x08, 0xC8, 0xD8, 0x18, 0x19, 0xD9, 0x1B, 0xDB, 0xDA, 0x1A,
            0x1E, 0xDE, 0xDF, 0x1F, 0xDD, 0x1D, 0x1C, 0xDC, 0x14, 0xD4,
            0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2, 0x12, 0x13, 0xD3,
            0x11, 0xD1, 0xD0, 0x10, 0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3,
            0xF2, 0x32, 0x36, 0xF6, 0xF7, 0x37, 0xF5, 0x35, 0x34, 0xF4,
            0x3C, 0xFC, 0xFD, 0x3D, 0xFF, 0x3F, 0x3E, 0xFE, 0xFA, 0x3A,
            0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38, 0x28, 0xE8, 0xE9, 0x29,
            0xEB, 0x2B, 0x2A, 0xEA, 0xEE, 0x2E, 0x2F, 0xEF, 0x2D, 0xED,
            0xEC, 0x2C, 0xE4, 0x24, 0x25, 0xE5, 0x27, 0xE7, 0xE6, 0x26,
            0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0, 0xA0, 0x60,
            0x61, 0xA1, 0x63, 0xA3, 0xA2, 0x62, 0x66, 0xA6, 0xA7, 0x67,
            0xA5, 0x65, 0x64, 0xA4, 0x6C, 0xAC, 0xAD, 0x6D, 0xAF, 0x6F,
            0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB, 0x69, 0xA9, 0xA8, 0x68,
            0x78, 0xB8, 0xB9, 0x79, 0xBB, 0x7B, 0x7A, 0xBA, 0xBE, 0x7E,
            0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C, 0xB4, 0x74, 0x75, 0xB5,
            0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71,
            0x70, 0xB0, 0x50, 0x90, 0x91, 0x51, 0x93, 0x53, 0x52, 0x92,
            0x96, 0x56, 0x57, 0x97, 0x55, 0x95, 0x94, 0x54, 0x9C, 0x5C,
            0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E, 0x5A, 0x9A, 0x9B, 0x5B,
            0x99, 0x59, 0x58, 0x98, 0x88, 0x48, 0x49, 0x89, 0x4B, 0x8B,
            0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D, 0x4D, 0x4C, 0x8C,
            0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42,
            0x43, 0x83, 0x41, 0x81, 0x80, 0x40
        };

        /// <summary>
        /// CRC校验
        /// </summary>
        /// <param name="data">校验的字节数组</param>
        /// <param name="length">校验的数组长度</param>
        /// <returns>该字节数组的奇偶校验字节</returns>
        public static UInt16 CRC16(byte[] data, int arrayLength)
        {
            byte CRCHigh = 0xFF;
            byte CRCLow = 0xFF;
            byte index;
            int i = 0;
            while (arrayLength-- > 0)
            {
                index = (System.Byte)(CRCHigh ^ data[i++]);
                CRCHigh = (System.Byte)(CRCLow ^ ArrayCRCHigh[index]);
                CRCLow = checkCRCLow[index];
            }

            return (UInt16)(CRCHigh << 8 | CRCLow);
        }

        //报警相关
        //开始报警
        public static void SendStartAlarm()
        {
            byte[] datas = new byte[] { 0x46, 0x41, 0x53, 0x31, 0x49 }; //FAS1I

            if (SerialDriver.GetDriver().alarm_isOpen())
            {
                SerialDriver.GetDriver().alarm_Send(datas);
            }
            else
            {
                MessageBox.Show("数值超标，报警失败。请检查报警串口配置是否正确");
            }
        }
        //停止报警
        public static void SendStopAlarm()
        {
            byte[] datas = new byte[] { 0x46, 0x41, 0x53, 0x30, 0x49 };   //FAS0I

            if (SerialDriver.GetDriver().alarm_isOpen())
            {
                SerialDriver.GetDriver().alarm_Send(datas);
            }
            else
            {
                MessageBox.Show("数值超标，报警失败。请检查报警串口配置是否正确");
            }
        }


        //下拉列表采样时间映射值 m
        public static double MapSampleTime(int id)
        {
            switch (id)
            {
                case 0:
                    return 0.25;
                case 1:
                    return 0.5;
                case 2:
                    return 1.0;
                case 3:
                    return 2.0;
                case 4:
                    return 3.0;
                case 5:
                    return 5.0;
                case 6:
                    return 8.0;
                case 7:
                    return 10.0;
                case 8:
                    return 13.0;
                case 9:
                    return 15.0;
                case 10:
                    return 17.0;
                case 11:
                    return 20.0;
                default:
                    return 0;
            }
        }

        //下啦列表取样流量映射表 L/m
        public static double MapSampleFluent(int id)
        {
            switch (id)
            {
                case 0:
                    return 0.2;
                case 1:
                    return 0.3;
                case 2:
                    return 0.4;
                case 3:
                    return 0.5;
                case 4:
                    return 0.6;
                case 5:
                    return 0.8;
                case 6:
                    return 1.0;
                default:
                    return 0;
            }
        }

        //下拉列表清洗时长映射表
        public static string MapWashTime(int id)
        {
            switch (id)
            {
                case 0:
                    return "2m";
                case 1:
                    return "3m";
                case 2:
                    return "4m";
                case 3:
                    return "5m";
                case 4:
                    return "6m";
                case 5:
                    return "8m";
                case 6:
                    return "10m";
                case 7:
                    return "12m";
                default:
                    return "0m";

            }
        }
    }

    //当汞浓度超出预期值时，报警
    //每次只能报警一个超标值。每次每个超标值报警最多三次，每次持续1分钟，间隔三分钟。
    public class AlarmRedObject
    {
        static AlarmRedObject onlyone = null;
        //报警次数
        int alarmtimes = 0;
        //报警状态
        AlarmStatus alarmstatus = AlarmStatus.NONE;
        //用户手动取消报警
        bool usercancel = false;
        //间隔定时器
        DispatcherTimer actiontimer = new DispatcherTimer();
        //持续定时器
        DispatcherTimer silenttimer = new DispatcherTimer();
        //主窗口
        MainWindow mw = null;
        private AlarmRedObject()
        {
            //actiontimer.Interval = new TimeSpan(0, 1, 0);
            //silenttimer.Interval = new TimeSpan(0, 3, 0);
            actiontimer.Interval = new TimeSpan(0, 0, 1);
            silenttimer.Interval = new TimeSpan(0, 0, 2);
            actiontimer.Tick += new EventHandler(actiontimer_tick_hdlr);
            silenttimer.Tick += new EventHandler(silenttimer_tick_hdlr);
            actiontimer.IsEnabled = false;
            silenttimer.IsEnabled = false;

        }
        private void silenttimer_tick_hdlr(object sender, EventArgs e)
        {
            //报警被用户中途取消
            if (usercancel == true)
            {
                usercancel = false;
                ClearAlarmObject();

                //发送停止停止报警请求
                Utility.SendStopAlarm();
                return;
            }

            //即将进入alarm状态
            alarmstatus = AlarmStatus.ALARM;
            actiontimer.IsEnabled = true;
            silenttimer.IsEnabled = false;
            //发送开始报警
            Utility.SendStartAlarm();
            Console.WriteLine("停止静默");
            Console.WriteLine("开始报警");
        }

        private void actiontimer_tick_hdlr(object sender, EventArgs e)
        {
            //报警被用户中途取消
            if (usercancel == true)
            {
                usercancel = false;
                ClearAlarmObject();

                //发送停止停止报警请求
                Utility.SendStopAlarm();
                AlarmActionFunc?.Invoke(false);
                return;
            }

            //报警超过三次，停止报警
            if (alarmtimes > 2)
            {
                ClearAlarmObject();
                Console.WriteLine("报警彻底结束");
                AlarmActionFunc?.Invoke(false);
                return;
            }

            alarmtimes++;
            //即将进入silent状态
            alarmstatus = AlarmStatus.SILENT;
            actiontimer.IsEnabled = false;
            silenttimer.IsEnabled = true;
            //发送停止报警
            Utility.SendStopAlarm();

            Console.WriteLine("已报警" + alarmtimes.ToString() + "次");
            Console.WriteLine("开始静默");
        }

        public static AlarmRedObject GetAlarmObject()
        {
            if (onlyone != null)
                return onlyone;
            else
            {
                onlyone = new AlarmRedObject();
                return onlyone;
            }
        }

        void ClearAlarmObject()
        {
            alarmstatus = AlarmStatus.NONE;
            alarmtimes = 0;
            silenttimer.Stop();
            silenttimer.IsEnabled = false;
            actiontimer.Stop();
            actiontimer.IsEnabled = false;
        }

        public delegate void AlarmActionDelegate(bool todo);
        AlarmActionDelegate AlarmActionFunc = null;

        public void RegisterAlarmAction(AlarmActionDelegate alr)
        {
            AlarmActionFunc = alr;
        }

        //公共接口
        public void NeedAlarm()
        {
            //mw = mainwindow;
            //在needalarm之前调用Cancel没有意义，直接忽略usercancel标志
            if (usercancel == true)
                usercancel = false;
            //
            if (GetAlarmObject().alarmstatus == AlarmStatus.NONE)
            {
                //即将进入alarm状态
                GetAlarmObject().alarmstatus = AlarmStatus.ALARM;

                actiontimer.Start();
                actiontimer.IsEnabled = true;

                silenttimer.Start();
                silenttimer.IsEnabled = false;

                //发送报警
                GetAlarmObject().alarmtimes = 0;
                Utility.SendStartAlarm();

                Console.WriteLine("发出报警指令");

                AlarmActionFunc?.Invoke(true);
            }
            else
            {
                //上次的报警还没结束，直接忽略本次的报警请求
                return;
                ;
            }
        }

        //用户手动关闭报警
        //只有在报警响起后cancel才有意义。当数据正常时，用户取消报警没有意义，直接忽略
        public void Cancel(MainWindow mainwindow)
        {
            this.usercancel = true;

            alarmstatus = AlarmStatus.NONE;
            alarmtimes = 0;
            silenttimer.Stop();
            actiontimer.Stop();
            //向报警器发送停止报警命令
            Utility.SendStopAlarm();
            AlarmActionFunc?.Invoke(false);
        }


    }

    //缓存测试dvalue，供在切换波形使用
    public class TableCache
    {
        private CacheItem[] datas;
        static private TableCache onlyone;
        private int maxcount;
        private int currentlength;
        private TableCache()
        {
            maxcount = 100;
            currentlength = 0;
            datas = new CacheItem[maxcount];
        }

        public int GetMaxCount()
        {
            return maxcount;
        }

        public static TableCache GetTableCache()
        {
            if (TableCache.onlyone == null)
            {
                onlyone = new TableCache();
            }
            return onlyone;
        }

        public bool isExist(TestingTabType type,string testing_gid)
        {
            for(int i = 0; i < currentlength; i++)
            {
                if (datas[i].type == type && datas[i].gid == testing_gid)
                    return true;
            }
            return false;
        }

        public void CacheWave(TestingTabType type,string testing_gid, Collection<ADot> main_tune_dvalues)
        {
            if (main_tune_dvalues == null)
                return;
            int index = -1;
            //已经存在
            for(int i = 0; i<currentlength; i++)
            {
                if(datas[i].type == type && datas[i].gid == testing_gid)
                {
                    index = i;
                    break;
                }
            }
            if(index != -1)
            {
                datas[index].wave = null;
                datas[index].wave = new Collection<ADot>(); 
                for(int i = 0; i < main_tune_dvalues.Count; i++)
                {
                    datas[index].wave.Add(new ADot(main_tune_dvalues[i].No,main_tune_dvalues[i].Rvalue,main_tune_dvalues[i].Status));
                }
                datas[index].refcount++;
                return;
            }
            //还有空槽
            if(currentlength < maxcount)
            {
                CacheItem newitm = new CacheItem();
                newitm.gid = testing_gid;
                newitm.type = type;
                //datas[index].wave = null;
                newitm.wave = new Collection<ADot>();
                for (int i = 0; i < main_tune_dvalues.Count; i++)
                {
                    newitm.wave.Add(new ADot(main_tune_dvalues[i].No, main_tune_dvalues[i].Rvalue, main_tune_dvalues[i].Status));
                }
                //newitm.wave = main_tune_dvalues;
                newitm.refcount = 1;
                datas[currentlength] = newitm;
                currentlength++;
                return;
            }
            //已满，寻找不常用项，替换它
            int oldmanindex = 0;
            int maxref = 0;
            for(int i = 0; i < currentlength; i++)
            {
                if (datas[i].refcount > maxref)
                {
                    maxref = datas[i].refcount;
                    oldmanindex = i;
                }
            }
            datas[oldmanindex].refcount = 1;
            datas[oldmanindex].wave = null;
            datas[oldmanindex].wave = new Collection<ADot>();
            for (int i = 0; i < main_tune_dvalues.Count; i++)
            {
                datas[oldmanindex].wave.Add(new ADot(main_tune_dvalues[i].No, main_tune_dvalues[i].Rvalue, main_tune_dvalues[i].Status));
            }
            //datas[oldmanindex].wave = main_tune_dvalues;
            datas[oldmanindex].gid = testing_gid;
            datas[oldmanindex].type = type;
        }

        public Collection<ADot> FindWave( TestingTabType type,string gid)
        {
            for(int i=0; i<currentlength; i++)
            {
                if (datas[i].gid == gid && datas[i].type == type)
                {
                    datas[i].refcount++;    
                    return datas[i].wave;
                }
            }
            return null;
        }
    }

    public class CacheItem
    {
        public int refcount { get; set; }
        public TestingTabType type { get; set; }
        public Collection<ADot> wave { get; set; }
        public Collection<ADot> vicetune_wave { get; set; }
        public string gid { get; set; }
    }


    public enum AlarmStatus
    {
        //报警
        ALARM,
        //间隔安静
        SILENT,
        //报警结束
        NONE
    }

        
}
