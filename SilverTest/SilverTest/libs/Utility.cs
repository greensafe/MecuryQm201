using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xml;

namespace SilverTest.libs
{
    class Utility
    {
        //从资源中读取数据新样xml数据，转换为ObservableCollection
        static public ObservableCollection<NewTestTarget> getNewTestTargetDataFromXDP(DataSourceProvider provider )
        {
            ObservableCollection<NewTestTarget>  newTargets = new ObservableCollection<NewTestTarget>();
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
