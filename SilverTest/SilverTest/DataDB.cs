using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilverTest
{
    class DataDB
    {
    }
    // 标样表结构定义
    public class StandardSample: INotifyPropertyChanged
    {
        //样品名称
        private string sampleName;
        public string SampleName
        {
            get {
                return sampleName;
            }
            set {
                sampleName = value;
                NotifyPropertyChanged("SampleName");
            }
        }

        //组名
        private string groupName;
        public string GroupName
        {
            get
            {
                return groupName;
            }
            set
            {
                groupName = value;
                NotifyPropertyChanged("GroupName");
            }
        }

        //样品编码
        private string code;
        public string Code {
            get { return code; }
            set {
                code = value;
                NotifyPropertyChanged("Code");
            }
        }
        //汞浓度
        private string density;
        public string Density {
            get { return density; }
            set
            {
                density = value;
                NotifyPropertyChanged("Density");
            }
        }
        //响应值
        private string responseValue1;
        public string ResponseValue1
        {
            get { return responseValue1; }
            set
            {
                responseValue1 = value;
                NotifyPropertyChanged("ResponseValue1");
            }
        }
        //样品重量
        private string weight;
        public string Weight {
            get { return weight; }
            set
            {
                weight = value;
                NotifyPropertyChanged("Weight");
            }
        }
        //样品出厂商
        private string providerCompany;
        public string ProviderCompany
        {
            get { return providerCompany; }
            set {
                providerCompany = value;
                NotifyPropertyChanged("ProviderCompany");
            }
        }
        //产地
        private string place;
        public string Place {
            get { return place; }
            set
            {
                place = value;
                NotifyPropertyChanged("Place");
            }
        }
        //样品购买日期
        private string buyDate;
        public string BuyDate {
            get { return buyDate; }
            set {
                buyDate = value;
                NotifyPropertyChanged("BuyDate");
            }
        }
        //斜率
        private string a;
        public string A {
            get { return a; }
            set {
                a = value;
                NotifyPropertyChanged("A");
            }
        }
        //截距
        private string b;
        public string B {
            get { return b; }
            set {
                b = value;
                NotifyPropertyChanged("B");
            }
        }
        //相关系数
        private string r;
        public string R
        {
            get { return r; }
            set
            {
                r = value;
                NotifyPropertyChanged("R");
            }
        }

        //构造函数
        public StandardSample(string sampleName, string code, string density,
            string weight, string providerCompany, string place, string buyDate, string a, string b,string grpname)
        {
            SampleName = sampleName;
            Code = code;
            Density = density;
            Weight = weight;
            ProviderCompany = providerCompany;
            Place = place;
            BuyDate = buyDate;
            A = a;
            B = b;
            GroupName = grpname;
        }
        public StandardSample() { }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    //新样表结构定义
    public class NewTestTarget: INotifyPropertyChanged
    {
        //新样名称
        private string newName;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        //新样名称
        public string NewName {
            get { return newName; }
            set
            {
                newName = value;
                NotifyPropertyChanged("NewName");
            }
        }
        //测试序号
        private string code;
        public string Code {
            get
            {
                return code;
            }
            set
            {
                code = value;
                NotifyPropertyChanged("Code");
            }
        }
        //重量
        private string weight;
        public string Weight {
            get { return weight; }
            set
            {
                weight = value;
                NotifyPropertyChanged("Weight");
            }
        }
        //产地
        private string place;
        public string Place {
            get { return place; }
            set
            {
                place = value;
                NotifyPropertyChanged("Place");
            }
        }
        //响应值1
        private string responseValue1;
        public string ResponseValue1 {
            get { return responseValue1; }
            set
            {
                responseValue1 = value;
                NotifyPropertyChanged("ResponseValue1");
            }
        }
        //响应值2
        private string responseValue2;
        public string ResponseValue2 {
            get { return responseValue2; }
            set
            {
                responseValue2 = value;
                NotifyPropertyChanged("ResponseValue2");
            }
        }
        //响应值3
        private string responseValue3;
        public string ResponseValue3 {
            get { return responseValue3; }
            set
            {
                responseValue3 = value;
                NotifyPropertyChanged("ResponseValue3");
            }
        }
        //平均值
        private string averageValue;
        public string AverageValue {
            get { return averageValue; }
            set
            {
                averageValue = value;
                NotifyPropertyChanged("AverageValue");
            }
        }
        //汞浓度
        private string density;
        public string Density {
            get { return density; }
            set
            {
                density = value;
                NotifyPropertyChanged("Density");
            }
        }
        //样品消化液总体积
        private string liquidSize;
        public string LiquidSize{
            get { return liquidSize; }
            set
            {
                liquidSize = value;
                NotifyPropertyChanged("LiquidSize");
            }
        }
        //构造函数
        public NewTestTarget(string newName, string code, string weight, string place, string responseValue1,
            string responseValue2, string responseValue3, string density, string liquidSize)
        {
            NewName = newName;
            Code = code;
            Weight = weight;
            ResponseValue1 = responseValue1;
            ResponseValue2 = responseValue2;
            ResponseValue3 = responseValue3;
            Density = density;
            LiquidSize = liquidSize;
            Place = place;
        }
        public NewTestTarget() { }
    }
}
