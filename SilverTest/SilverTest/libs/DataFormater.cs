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
        //@Param
        // dot - dot值
        // sequence - 序号
        public delegate void PacketReceviedDelegate(object dot, int sequence, PacketType ptype);  //解析出了一个dot
        public delegate void PacketCorrectedDelegate(ADot dot, int sequence); //响应包数据校验成功
        public delegate void PacketCheckErrorDelegate(int sequence,PacketType ptype); //数据包校验失败
        public delegate void PacketStillErrorDelegate(int sequence);  // 响应包数据校验再次失败
        
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
        PacketReceviedDelegate PacketRecevied_Ev = null;
        PacketCorrectedDelegate PacketCorrected_Ev = null;
        PacketStillErrorDelegate PacketStillError_Ev = null;
        PacketCheckErrorDelegate PacketCheckError_Ev = null;

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
        public Collection<ADot> GetDots()
        {
            return dots;
        }


        public void onPacketRecevied(PacketReceviedDelegate hdlr)
        {
            PacketRecevied_Ev = hdlr;
        }

        public void onPacketCorrected(PacketCorrectedDelegate hdlr)
        {
            PacketCorrected_Ev = hdlr;
        }

        public void onPacketStillError(PacketStillErrorDelegate hdlr)
        {
            PacketStillError_Ev = hdlr;
        }

        public void onPacketCheckError(PacketCheckErrorDelegate hdlr)
        {
            PacketCheckError_Ev = hdlr;
        }


        //处理PhyCombine.PacketRecevied_Ev的函数, packet是一个纯包
        public void PacketReceviedHdlr(byte[] packet, PacketType ptype)
        {
            switch (ptype)
            {
                case PacketType.CORRECT_RESPONSE:
                    if (validateData(packet,PhyCombine.GetPhyCombine().GetMachineInfo().CrtPctDStart
                        ,PhyCombine.GetPhyCombine().GetMachineInfo().DataWidth,
                        twoint(packet,PhyCombine.GetPhyCombine().GetMachineInfo().CrtPctVStart)) == true)
                    {
                        int seq = Utility.ConvertStrToInt_Big(packet, 
                            PhyCombine.GetPhyCombine().GetMachineInfo().CrtPctSStart,
                            PhyCombine.GetPhyCombine().GetMachineInfo().SequenceLength);
                        dots[seq].Rvalue = Utility.ConvertStrToInt_Big(packet,
                            PhyCombine.GetPhyCombine().GetMachineInfo().CrtPctDStart,
                            PhyCombine.GetPhyCombine().GetMachineInfo().DataWidth);
                        
                        dots[seq].Status = DotStaus.CORRECTED;
                        if(PacketCorrected_Ev != null)
                        {
                            PacketCorrected_Ev(dots[seq],seq);
                        }
                        return;
                    }
                    else
                    {
                        if(PacketStillError_Ev != null)
                            PacketStillError_Ev(Utility.ConvertStrToInt_Big(packet,
                            PhyCombine.GetPhyCombine().GetMachineInfo().CrtPctSStart,
                            PhyCombine.GetPhyCombine().GetMachineInfo().SequenceLength));
                    }
                    break;
                case PacketType.DATA_VALUE:
                    if(validateData(packet,PhyCombine.GetPhyCombine().GetMachineInfo().DataPctDStart,
                        PhyCombine.GetPhyCombine().GetMachineInfo().DataWidth,twoint(packet,PhyCombine.GetPhyCombine().GetMachineInfo().DataPctVStart)) == true)
                    {
                        dots.Add(new ADot() {
                            Rvalue = Utility.ConvertStrToInt_Big(packet, PhyCombine.GetPhyCombine().GetMachineInfo().DataPctDStart,
                                                        PhyCombine.GetPhyCombine().GetMachineInfo().DataWidth),
                            Status = DotStaus.OK
                        });
                        if (PacketRecevied_Ev != null)
                        {
                            //通知收到一个包
                            PacketRecevied_Ev(dots[dots.Count-1],dots.Count -1,PacketType.DATA_VALUE );
                        }

                    }
                    else
                    {
                        //发生错误
                        dots.Add(new ADot() {
                            Status = DotStaus.CORRECTING,
                        });
                        if(PacketCheckError_Ev != null)
                        {
                            PacketCheckError_Ev(dots.Count - 1, PacketType.DATA_VALUE);
                        }

                    }
                    break;
                case PacketType.AIR_FLUENT:
                    if (validateData(packet, PhyCombine.GetPhyCombine().GetMachineInfo().AirFluPctDStart,
                         PhyCombine.GetPhyCombine().GetMachineInfo().AirFluPctDataWidth, 
                         twoint(packet, PhyCombine.GetPhyCombine().GetMachineInfo().AirFluPctVStart)) == true)
                    {
                        if (PacketRecevied_Ev != null)
                        {
                            //通知收到一个包
                            PacketRecevied_Ev(null, Utility.ConvertStrToInt_Big(packet, PhyCombine.GetPhyCombine().GetMachineInfo().AirFluPctDStart,
                                                        PhyCombine.GetPhyCombine().GetMachineInfo().AirFluPctDataWidth), PacketType.AIR_FLUENT);
                        }

                    }
                    else
                    {
                        //发生错误
                        if (PacketCheckError_Ev != null)
                        {
                            PacketCheckError_Ev(0,PacketType.AIR_FLUENT);
                        }

                    }
                    break;
                case PacketType.AIR_SAMPLE_TIME:
                    if (validateData(packet, PhyCombine.GetPhyCombine().GetMachineInfo().AirSTPctDStart,
                         PhyCombine.GetPhyCombine().GetMachineInfo().AirSTPctDataWidth,
                         twoint(packet, PhyCombine.GetPhyCombine().GetMachineInfo().AirSTPctVStart)) == true)
                    {
                        if (PacketRecevied_Ev != null)
                        {
                            //通知收到一个包
                            PacketRecevied_Ev(null, Utility.ConvertStrToInt_Big(packet, PhyCombine.GetPhyCombine().GetMachineInfo().AirSTPctDStart,
                                                        PhyCombine.GetPhyCombine().GetMachineInfo().AirSTPctDataWidth), PacketType.AIR_SAMPLE_TIME);
                        }

                    }
                    else
                    {
                        //发生错误
                        if (PacketCheckError_Ev != null)
                        {
                            PacketCheckError_Ev(0, PacketType.AIR_SAMPLE_TIME);
                        }

                    }
                    break;
                case PacketType.SEQUENCE:

                    break;
                default:
                    Console.WriteLine("DataFormater:未知包");
                    break;
            }
        }




        //校验拼接，将数字高低位拼接成一个完整的数字
        private int twoint(byte[] data,int start)
        {
            string cv = "";
            //int total = 0;

            cv += (char)data[start];
            cv += (char)data[start+1];
            return int.Parse(cv);
            //total += data[start];
            //total += data[start+1] * 256;
            //return total;
        }

        //校验数据
        //校验数据正确性
        //param
        // data - 待校验数据
        // cv - 校验值
        private bool validateData(byte[] data,int start, int len, int cv)
        {
            int total = 0;
            for(int i = 0; i< len; i++)
            {
                total += (data[start + i] - 0x30);
            }
            if(total == cv)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

}
