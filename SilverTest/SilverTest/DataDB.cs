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
        //样品质量
        private string weight;
        public string Weight {
            get { return weight; }
            set
            {
                weight = value;
                NotifyPropertyChanged("Weight");
            }
        }
        //温度
        private string temperature;
        public string Temperature
        {
            get { return temperature; }
            set
            {
                temperature = value;
                NotifyPropertyChanged("Temperature");
            }
        }
        //气体标样体积
        private string airML;
        public string AirML
        {
            get { return airML; }
            set
            {
                airML = value;
                NotifyPropertyChanged("AirML");
            }
        }

        //气体汞量
        private string airG;
        public string AirG
        {
            get { return airG; }
            set
            {
                airG = value;
                NotifyPropertyChanged("AirG");
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

        //气体取样时间
        /*
        private string airSampleTime;
        public string AirSampleTime
        {
            get { return airSampleTime; }
            set
            {
                airSampleTime = value;
                NotifyPropertyChanged("airSampleTime");
            }
        }
        */
        //汞流量
        private string airFluent;
        public string AirFluent
        {
            get { return airFluent; }
            set
            {
                airFluent = value;
                NotifyPropertyChanged("airFluent");
            }
        }

        //全局唯一id
        private string globalID;
        public string GlobalID
        {
            get { return globalID; }
            set
            {
                globalID = value;
                NotifyPropertyChanged("globalID");
            }
        }

        //构造函数
        public StandardSample(string sampleName, string code, string density,
            string weight, string providerCompany, string place, string buyDate, string a, string b,string grpname, string gid)
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
            GlobalID = gid;
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
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        //新样名称
        private string newName;
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
        //样品总体积L
        private string airTotolBulk;
        public string AirTotolBulk
        {
            get { return airTotolBulk; }
            set
            {
                airTotolBulk = value;
                NotifyPropertyChanged("AirTotolBulk");
            }
        }

        //气体取样时间
        private string airSampleTime;
        public string AirSampleTime
        {
            get { return airSampleTime; }
            set
            {
                airSampleTime = value;
                NotifyPropertyChanged("AirSampleTime");
            }
        }

        //气体流量
        private string airFluent;
        public string AirFluent
        {
            get { return airFluent; }
            set
            {
                airFluent = value;
                NotifyPropertyChanged("AirFluent");
            }
        }

        //气体样品中汞含量
        private string airG;
        public string AirG
        {
            get { return airG; }
            set
            {
                airG = value;
                NotifyPropertyChanged("AirG");
            }
        }

        //样品气体总流量
        private string airTotalFluent;
        public string AirTotalFluent
        {
            get { return airTotalFluent; }
            set
            {
                airTotalFluent = value;
                NotifyPropertyChanged("AirTotalFluent");
            }
        }


        //汞含量
        private string thingInSamle;
        public string ThingInSamle
        {
            get { return thingInSamle; }
            set
            {
                thingInSamle = value;
                NotifyPropertyChanged("ThingInSamle");
            }
        }

        //进样量
        private string inSampleQuality;
        public string InSampleQuality
        {
            get { return inSampleQuality; }
            set
            {
                inSampleQuality = value;
                NotifyPropertyChanged("InSampleQuality");
            }
        }


        //全局唯一id
        private string globalID;
        public string GlobalID
        {
            get { return globalID; }
            set
            {
                globalID = value;
                NotifyPropertyChanged("globalID");
            }
        }

        //构造函数
        public NewTestTarget(string newName, string code, string weight, string place, string responseValue1,
            string responseValue2, string responseValue3, string density, string liquidSize, string airTotolBulk,
            string airSampleTime, string airFluent, string airG, string gid)
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
            AirTotolBulk = airTotolBulk;
            AirSampleTime = airSampleTime;
            AirFluent = airFluent;
            AirG = airG;
            GlobalID = gid;
        }
        public NewTestTarget() { }
    }
}
