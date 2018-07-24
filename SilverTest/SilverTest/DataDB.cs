using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilverTest
{
    class DataDB
    {
    }
    // 标样表结构定义
    public class StandardSample
    {
        //样品名称
        public string SampleName
        {
            get;set;
        }
        //样品编码
        public string Code { get; set; }
        //汞浓度
        public string Density { get; set; }
        //样品重量
        public string Weight { get; set; }
        //样品出厂商
        public string ProviderCompany { get; set; }
        //产地
        public string Place { get; set; }
        //样品购买日期
        public string BuyDate { get; set; }
        //斜率
        public string A { get; set; }
        //截距
        public string B { get; set; }
        //构造函数
        public StandardSample(string sampleName, string code, string density,
            string weight, string providerCompany, string place, string buyDate, string a, string b)
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
        }
        public StandardSample() { }
    }

    //新样表结构定义
    public class NewTestTarget
    {
        //新样名称
        public string NewName { get; set; }
        //测试序号
        public string Code { get; set; }
        //重量
        public string Weight { get; set; }
        //产地
        public string Place { get; set; }
        //响应值1
        public string ResponseValue1 { get; set; }
        //响应值2
        public string ResponseValue2 { get; set; }
        //响应值3
        public string ResponseValue3 { get; set; }
        //平均值
        public string AverageValue { get; set; }
        //汞浓度
        public string Density { get; set; }
        //样品消化液总体积
        public string LiquidSize{ get; set; }
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
