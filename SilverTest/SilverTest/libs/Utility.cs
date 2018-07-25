using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Xml;
using System.Xml.Serialization;

namespace SilverTest.libs
{
    class Utility
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
}
