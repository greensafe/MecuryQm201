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
    }

    //新样表结构定义
    public class NewTestTarget
    {
        //新样名称
        public string NewName { get; set; }
        //测试序号
        public string Number { get; set; }
        //重量
        public string Weight { get; set; }
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
    }
}
