using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SilverTest.libs.PhyCombine;

namespace SilverTest.libs
{
    /*
     * 将PhyCombine返回的包解析成数据供波形控件显示
     */
    public class DataFormater
    {
        //点的格式
        public class ADot
        {
            public int No { get; set; }                 //数据序号
            public int Rvalue { get; set; }             //下维机返回值
            public DotStaus Status { get; set; }        //点值状态
        }

        public enum DotStaus
        {
            OK,                         //正常
            CORRECTING,                 //纠正中
            CORRECTED,                  //纠正成功
            ERROR,                      //纠正失败，错误数据
        }

        private Collection<ADot> dots = null;

        static private DataFormater onlyone = null;
        static public DataFormater getDataFormater()
        {
            if (onlyone == null)
            {
                onlyone = new DataFormater();
            }
            return onlyone;
        }
        private DataFormater()
        {
            dots = new Collection<ADot>();
        }

        //获取所有点组
        public ICollection GetDots()
        {
            return dots;
        }

        //PacketRecevied_Ev回调函数, packet是一个纯包
        void PacketReceviedDelegate(byte[] packet, PacketType ptype)
        {
            switch (ptype)
            {
                case PacketType.CORRECT_RESPONSE:
                    if (validateData(packet +1,PhyCombine.GetPhyCombine().GetMachineInfo().CrtPctLength,
                        twoint(packet)) == true)
                    {

                    }
                    else
                    {
                        
                    }
                    break;
                case PacketType.DATA_VALUE:
                    
                    break;
                case PacketType.SEQUENCE:

                    break;
                default:
                    Console.WriteLine("DataFormater:未知包");
                    break;
            }
        }

        private bool validateData(object p, object g)
        {
            throw new NotImplementedException();
        }

        //将字符串拼接数字
        private int cToNum(byte[] data, int len)
        {
            string a = "";
            for(int i = 0; i< len; i++)
            {
                a += (char)data[i];
            }
            return int.Parse(a) ;
        }

        //将数字高低位拼接成一个完整的数字
        private int twoint(byte[] data)
        {
            int total = 0;
            total += data[0];
            total += data[1] * 256;
            return total;
        }

        //校验数据
        //校验数据正确性
        //param
        // data - 待校验数据
        // cv - 校验值
        private bool validateData(byte[] data, int len, int cv)
        {
            return true;
        }
    }

}
