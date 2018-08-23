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
                    if ((dataGrid.Columns[i].GetCellContent(dataGrid.Items[r]) is null)){
                        worksheet.Cells[r + 2, i + 1] = "NaN";
                    }
                    else {
                        worksheet.Cells[r + 2, i + 1] = (dataGrid.Columns[i].GetCellContent(dataGrid.Items[r]) as TextBlock).Text;
                    }
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
            for(int i =0; i< len; i++)
            {
                total += data[start+i] - 0x30;
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

            for(int i = start_abs; i< end_abs; i++)
            {
                if(dots[i].Rvalue > max)
                {
                    max = dots[i].Rvalue;
                }
            }

            return max - dots[0].Rvalue;
        }

        /*
         * 计算相关截距，斜率
         * @param
         *  x - x轴数据
         *  y - y轴数据
         */
         static public void ComputeAB(out double a, out double b, double[] x, double[] y )
        {
            //double[] ydata = new double[] { 42.0, 43.5, 45.0, 45.5, 45.0, 47.5, 49.0, 53.0, 50.0, 55.0, 55.0, 60.0 };
            //double[] xdata = new double[] { 0.10, 0.11, 0.12, 0.13, 0.14, 0.15, 0.16, 0.17, 0.18, 0.20, 0.21, 0.23 };
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
         * 从xml取值
         * @param
         *  xpath - 定位元素。 比如
         *              descendant::Ratio
         */
        static public string GetValueFrXml(string xpath, string propertyname)
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(@"config\config.xml");

            //XmlElement root = xdoc.DocumentElement;
            //XmlNode node = xdoc.SelectSingleNode(xpath);
            //XmlNode node = xdoc.SelectSingleNode("/config/QM201H/response/compute");
            //XmlNode node = xdoc.SelectSingleNode("/config/response/compute/R3");

            //string t = node.Attributes[propertyname].Value;
            
            return "";
            
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

            for (int i = start_abs; i<end_abs; i++)
            {
                t += dots[i].Rvalue * 1;
            }

            return ( t / (end_abs - start_abs) ) * ratio;
        }
    }
   
}
