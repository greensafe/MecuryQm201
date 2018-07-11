using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilverTest
{
    public class Requirement
    {
        //校正软件峰值和仪器读数
        static public void ajust() {
            ;
        }

        //计算平均值
        static public void countAverage()
        {
            ;
        }

        //计算汞浓度
        static public void countMercuryThickness()
        {
            ;
        }

        //计算高斯曲线，输出统计值
        static public double countGaussCurve(GaussOutputPoint type)
        {
            return 0;
        }

        public enum GaussOutputPoint {
            RESPONSE,

        }
    }
}
