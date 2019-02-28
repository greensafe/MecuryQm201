using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilverTest.libs
{
    /* 物理层，singleton, 将串口数据片段合并成一个完整的包。拼接完成后通知DataFormater。
     * 术语
     *    完整包 - 一个完整的包
     *    新包 - 上次已经拼接成功一个完整包。本次收到的包叫新包。新包check后，可能是个完整包，也可能
     *              是个碎片
     *    碎片 - 完整包的一部分。如果上次收到是个碎片，本次收到的一定也是碎片
     *    拼接 - 将碎片拼接成完整包
     *    净包 - 将完整包中里面的无用的头尾去掉，得到净包
     *    rawtext - 原始文本，存放串口发过来的原始数据
     *    rawText_bigpct_prt -  rawtext中完整包的起始位置
     *    rawText_purepct_prt - rawtext中纯包的起始位置
     *    rawText_length - rawtext的实际长度
     *    rawText_maxlength - rawtext的最大长度
     *    rawText_frag_status - rawtext包的拼接状态。如果为OK，表示上一个包拼接成功，本次是一个新包。
     */
    public class PhyCombine
    {
        public delegate void PacketReceviedDelegate(byte[] packet, PacketType ptype);  //收到数据包
        public delegate void PacketCorrectingDelegate(byte[] packet, PacketType ptype); //收到纠正包
        public delegate void CombineErrorDelegate(CombineErrorInfo err);   //错误发生

        private delegate void CombineFragmentDelegate(byte[] rawitem); // 自抛代码原型

        public class MachineInfo
        {
            public int MachineTypeHeaderLength { get; set; }  //机器类型头包长度

            public int SequenceLength { get; set; } //序号长度
            public int DataWidth { get; set; } //数据位宽
            public int Type { get; set; } //机器类型

            public int DataPctLength { get; set; } //数据包长度
            public int DataPctEndTag { get; set; } //数据包结束标志位置
            public int DataPctMiddleTag { get; set; } //数据中间标志位置
            public int DataPctVStart { get; set; }       //数据包校验位开始位置
            public int DataPctDStart { get; set; }       //数据包数据开始位置

            public int SrlPctLength { get; set; } //序号包长度
            public int SrlPctEndTag { get; set; } //序号包结束标志位置
            public int SrlPctDStart { get; set; } //序号包数据开始位置


            public int CrtPctLength { get; set; } //纠正包长度
            public int CrtPctMiddleTag { get; set; } //纠正包中间标志位置
            public int CrtPctEndTag { get; set; }   //纠正包结束标志位置
            public int CrtPctDStart { get; set; }   //纠正包数据开始位置
            public int CrtPctSStart { get; set; }   //纠正包序号开始位置
            public int CrtPctVStart { get; internal set; }  //纠正包校验开始位置

            public int AirSTPctLength { get; set; } //气体取样时间包长度
            public int AirSTPctMiddleTag { get; set; }  //气体取样包中间标志
            public int AirSTPctMiddleStart { get; set; }    //气体取样包中间标志位置
            public int AirSTPctEndTag { get; set; } //气体取样包结束标志
            public int AirSTPctEndStart { get; set; } //气体取样包结束标志位置
            public int AirSTPctDStart { get; set; } //气体取样包数据开始位置
            public int AirSTPctDataWidth { get; set; } //气体取样包数据长度
            public int AirSTPctVStart { get; set; } //气体取样包纠正包开始位置

            public int AirFluPctLength { get; set; } //气体流量包长度
            public int AirFluPctMiddleTag { get; set; }  //气体流量包中间标志
            public int AirFluPctMiddleStart { get; set; }    //气体流量包中间标志位置
            public int AirFluPctEndTag { get; set; } //气体流量包结束标志
            public int AirFluPctEndStart { get; set; } //气体流量包结束标志位置
            public int AirFluPctDStart { get; set; } //气体流量包数据开始位置
            public int AirFluPctDataWidth { get; set; } //气体流量包数据长度
            public int AirFluPctVStart { get; set; } //气体流量包纠正包开始位置

            public int ResComputePctLength { get; set; } //计算包长度
            public int ResComputePctDStart { get; set; }       //计算包数据开始位置
            public int ResComputePctMiddleTag { get; set; } //计算包数据中间标志位置
            public int ResComputePctVStart { get; set; }       //计算包校验位开始位置
            public int ResComputePctEndTag { get; set; } //计算包结束标志位置

            //状态获取命令回应包 GetStatusResPct
            public int GetStatusResPctLength { get; set; } //状态获取回应包长度
            public int GetStatusResPctMiddleTag { get; set; }  //状态获取回应包中间标志
            public int GetStatusResPctMiddleStart { get; set; }    //状态获取回应包中间标志位置
            public int GetStatusResPctEndTag { get; set; } //状态获取回应包结束标志
            public int GetStatusResPctEndStart { get; set; } //状态获取回应包结束标志位置
            public int GetStatusResPctDStart { get; set; } //状态获取回应包数据开始位置
            public int GetStatusResPctDataWidth { get; set; } //状态获取回应包数据长度
            public int GetStatusResPctVStart { get; set; } //状态获取回应包纠正开始位置

            //普通命令回应包 
            public int NorCmdResPctLength { get; set; } //普通命令回应包长度
            public int NorCmdResPctMiddleTag { get; set; }  //普通命令回应包中间标志
            public int NorCmdResPctMiddleStart { get; set; }    //普通命令回应包中间标志位置
            public int NorCmdResPctEndTag { get; set; } //普通命令回应包结束标志
            public int NorCmdResPctEndStart { get; set; } //普通命令回应包结束标志位置
            public int NorCmdResPctDStart { get; set; } //普通命令回应包数据开始位置
            public int NorCmdResPctDataWidth { get; set; } //普通命令回应包数据长度
            public int NorCmdResPctVStart { get; set; } //普通命令回应包纠正包开始位置

            //辅道数据包
            public int ViceDataPctLength { get; set; } //辅道数据包长度
            public int ViceDataPctEndTag { get; set; } //辅道数据包结束标志位置
            public int ViceDataPctMiddleTag { get; set; } //辅道数据中间标志位置
            public int ViceDataPctVStart { get; set; }       //辅道数据包校验位开始位置
            public int ViceDataPctDStart { get; set; }       //辅道数据包数据开始位置
        }

        //数据包类型
        public enum PacketType
        {
            MACHINE_TYPE,               //机器类型
            DATA_VALUE,                 //机器采样到的一个数据值
            VICE_DATA_VALUE,                 //机器采样到的一个辅道数据值
            RES_COMPUTE_VALUE,          //计算包
            SEQUENCE,                   //数据值的序号包
            CORRECT_RESPONSE,           //校验出错重发命令的响应包
            AIR_SAMPLE_TIME,            //气体取样时间包
            AIR_FLUENT,                 //气体流量包
            GETSTATUS_RESPONSE,         //状态获取命令回应包
            NORCMD_RESPONSE,           //普通命令回应包
            FRAGMENT,                   //碎片
            UNKNOWN                     //未知格式
        }

        //上一个包拼接的状态
        private enum PacketCombineStatus {
                                    OK,                         // OK - 包是完整的包。
                                    FRAGMENT_MACHINE_TYPE,      //包是个机器信息头碎片
                                    FRAGMENT_VALUE,             //包是一个数据值碎片
                                    FRAGMENT_VICE_VALUE,             //包是一个辅道数据值碎片
                                    FRAGMENT_RES_COMPUTE,       //包是一个计算包碎片
                                    FRAGMENT_SEQUENCE,          //包是一个序号碎片
                                    FRAGMENT_CORRECT,           //包是校验出错响应包碎片
                                    FRAGMENT_AIR_SAMPLE_TIME,   //包是气体取样时间包碎片
                                    FRAGMENT_AIR_FLUENT,        //包是气体流量包碎片
                                    FRAGMENT_GETSTATUS_RESPONSE,//包是状态获取命令回应包碎片
                                    FRAGMENT_NORCMD_RESPONSE,   //包是普通命令回应包碎片
                                    FRAGMENT_UNKONWN            //碎片格式未知
        };

        //check结果
        private enum CheckError
        {
            OK,                         //正常
            CONTINUE,                   //包不完整，继续拼接
            PACKET_TPYE_ERROR,            //F标志后跟着一个无法识别的包类型，可能是数据失真
            DATA_FORMAT_ERROR,          //包的格式有错，找不到包的结束位置,和中间标志位置
        }

        //拼包错误类型，用于错误事件
        public enum CombineErrorInfo
        {
            INVALID_MACHINE_TYPE,           //非法的机器类型
            NOT_FOUND_MACHINE_HEADER_LONG,  //长时间不能收到机器类型包
            NOT_FOUND_START_TAG_LONG,       //长时间不能找到包的其始标志
            VALUE_PCT_DATA_FORMAT_ERROR,          //数据包的格式有错，找不到包的结束位置,和中间标志位置
            VICE_VALUE_PCT_DATA_FORMAT_ERROR,          //辅道数据包的格式有错，找不到包的结束位置,和中间标志位置
            RES_COMPUTE_VALUE_PCT_DATA_FORMAT_ERROR, //计算包格式有错误，找不到包的结束位置,和中间标志位置
            SEQUENCE_PCT_DATA_FORMAT_ERROR,          //序号包的格式有错，找不到包的结束位置,和中间标志位置
            CORRECT_PCT_DATA_FORMAT_ERROR,          //纠正包的格式有错，找不到数据包的结束位置,和中间标志位置
            AIR_SAMPLE_TIME_PCT_DATA_FORMAT_ERROR,          //气体取样时间包的格式有错，找不到数据包的结束位置,和中间标志位置
            AIR_FLUENT_PCT_DATA_FORMAT_ERROR,          //气体流量包的格式有错，找不到数据包的结束位置,和中间标志位置
            GETSTATUS_RESPONSE_FORMAT_ERROR,           //状态获取命令回应包的格式有错，找不到数据包的结束位置,和中间标志位置
            NORCMD_RESPONSE_FORMAT_ERROR,           //普通命令回应包的格式有错，找不到数据包的结束位置,和中间标志位置
            INVALID_PACKET_TYPE,            //非法的包类型
            UNKNOWN,                        //程序算法出错 :<
        }

        private byte[] rawText;
        private int rawText_bigpct_prt = 0;  //指向完整的首部
        private int rawText_purepct_prt = 0;  //指向纯包的首部
        private int rawText_length = 0; //rawText 的实际长度，
        private readonly int rawText_maxlength = 1000000;
        private readonly int MachineTypeHeaderLength = 4;  //机器类型包长度
        private PacketCombineStatus rawText_frag_status = PacketCombineStatus.OK; //包的状态
        private MachineInfo machineinfo;

        static private PhyCombine onlyme = null;

        private CombineFragmentDelegate CombineFragmentSg = null;


        private PacketReceviedDelegate PacketRecevied_Ev = null;  //收到一个完整的包

        private CombineErrorDelegate CombineError_Ev = null;  //拼包发生一个错误

        static public PhyCombine GetPhyCombine()
        {
            if(onlyme == null)
            {
                onlyme = new PhyCombine();
            }
            return onlyme;
        }

        private void initmachineinfo()
        {
            machineinfo.CrtPctEndTag = 16;
            machineinfo.CrtPctLength = 17;
            machineinfo.CrtPctMiddleTag = 7;
            machineinfo.CrtPctDStart = 2;
            machineinfo.CrtPctSStart = 10;        //序号起始位置
            machineinfo.CrtPctVStart = 8;

            machineinfo.DataPctEndTag = 10;
            machineinfo.DataPctLength = 11;
            machineinfo.DataPctMiddleTag = 7;
            machineinfo.DataPctDStart = 2;
            machineinfo.DataPctVStart = 8;

            machineinfo.ViceDataPctEndTag = 10;     //辅道数据包
            machineinfo.ViceDataPctLength = 11;
            machineinfo.ViceDataPctMiddleTag = 7;
            machineinfo.ViceDataPctDStart = 2;
            machineinfo.ViceDataPctVStart = 8;


            machineinfo.ResComputePctEndTag = 10;        //回应计算包
            machineinfo.ResComputePctLength = 11;
            machineinfo.ResComputePctMiddleTag = 7;
            machineinfo.ResComputePctDStart = 2;
            machineinfo.ResComputePctVStart = 8;

            machineinfo.SrlPctEndTag = 8;
            machineinfo.SrlPctLength = 9;

            //气体流量包
            machineinfo.AirFluPctDataWidth = 1;
            machineinfo.AirFluPctDStart = 2;
            machineinfo.AirFluPctEndStart = 6;
            machineinfo.AirFluPctEndTag = 0x49;
            machineinfo.AirFluPctLength = 7;
            machineinfo.AirFluPctMiddleStart = 3;
            machineinfo.AirFluPctMiddleTag = 0x45;
            machineinfo.AirFluPctVStart = 4;

            //气体取样时间包
            machineinfo.AirSTPctDataWidth = 2;
            machineinfo.AirSTPctDStart = 2;
            machineinfo.AirSTPctEndStart = 7;
            machineinfo.AirSTPctEndTag = 0x49;
            machineinfo.AirSTPctLength = 8;
            machineinfo.AirSTPctMiddleStart = 4;
            machineinfo.AirSTPctMiddleTag = 0x42;
            machineinfo.AirSTPctVStart = 5;

            //状态获取命令回应包
            machineinfo.GetStatusResPctDataWidth = 8;
            machineinfo.GetStatusResPctDStart = 5;
            machineinfo.GetStatusResPctEndStart = 16;
            machineinfo.GetStatusResPctEndTag = 0x49;
            machineinfo.GetStatusResPctLength = 17;
            machineinfo.GetStatusResPctMiddleStart = 13;
            machineinfo.GetStatusResPctMiddleTag = 0x48;
            machineinfo.GetStatusResPctVStart = 14;

            //普通命令回应包
            machineinfo.NorCmdResPctDataWidth = 3;
            machineinfo.NorCmdResPctDStart = 2;
            machineinfo.NorCmdResPctEndStart = 8;
            machineinfo.NorCmdResPctEndTag = 0x49;
            machineinfo.NorCmdResPctLength = 9;
            machineinfo.NorCmdResPctMiddleStart = 5;
            machineinfo.NorCmdResPctMiddleTag = 0x48;
            machineinfo.NorCmdResPctVStart = 6;

            machineinfo.DataWidth = 5;
            machineinfo.SequenceLength = 6;
            machineinfo.Type = 0x01;
        }

        private PhyCombine()
        {
            rawText = new byte[rawText_maxlength];
            Array.Clear(rawText, 0, rawText_maxlength);
            machineinfo = new MachineInfo();
            //初始化机器类型
            initmachineinfo();

            CombineFragmentSg = delegate (byte[] rawitem)
            {
                #region phase1: get machine header
                /*
                ////phase 1: 获取机器格式头////
                int st = -1;

                if (!append(rawitem))
                {
                    Console.WriteLine("到达尾部，无法append");
                    return;
                }
                rawText_length += rawitem.Length;

                for (int i = rawText_bigpct_prt; i < rawText_length -1; i++)  //查找包起始标志
                {
                    if (rawText[i] == 0x46 && rawText[i+1] == 0x4C )  //找到'FL'
                    {
                        st = i;
                        break;
                    }
                }
                if (st == -1 && rawText_length > rawText_maxlength - 100)  //长时间不能收到机器类型包
                {
                    Console.WriteLine("长时间不能收到机器类型包");
                    if(CombineError_Ev != null) CombineError_Ev(CombineErrorInfo.NOT_FOUND_MACHINE_HEADER_LONG);

                    return;
                }
                if (st == -1 ||(st + MachineTypeHeaderLength > rawText_length ))  //找不到起始标志'F', 或者碎片太短，继续拼包
                {
                    return;
                }
                if(rawText[st+2] != 0x01)   //未能识别的机器类型
                {
                    Console.WriteLine("未能识别的机器类型");
                    if (CombineError_Ev != null) { CombineError_Ev(CombineErrorInfo.INVALID_MACHINE_TYPE); }
                    return;
                }

                //找到机器类型包，准备自弃
                switch (rawText[st+2])
                {
                    case 0x01:
                        machineinfo.CrtPctEndTag = 16;
                        machineinfo.CrtPctLength = 17;
                        machineinfo.CrtPctMiddleTag = 7;
                        machineinfo.CrtPctDStart = 2;
                        machineinfo.CrtPctSStart = 10;        //序号起始位置
                        machineinfo.CrtPctVStart = 8;       

                        machineinfo.DataPctEndTag = 10;
                        machineinfo.DataPctLength = 11;
                        machineinfo.DataPctMiddleTag = 7;
                        machineinfo.DataPctDStart = 2;
                        machineinfo.DataPctVStart = 8;

                        machineinfo.SrlPctEndTag = 8;
                        machineinfo.SrlPctLength = 9;

                        //气体流量包
                        machineinfo.AirFluPctDataWidth = 1;
                        machineinfo.AirFluPctDStart = 2;
                        machineinfo.AirFluPctEndStart = 6;
                        machineinfo.AirFluPctEndTag = 0x49;
                        machineinfo.AirFluPctLength = 7;
                        machineinfo.AirFluPctMiddleStart = 3;
                        machineinfo.AirFluPctMiddleTag = 0x45;
                        machineinfo.AirFluPctVStart = 4;

                        //气体取样时间包
                        machineinfo.AirSTPctDataWidth = 2;
                        machineinfo.AirSTPctDStart = 2;
                        machineinfo.AirSTPctEndStart = 7;
                        machineinfo.AirSTPctEndTag = 0x49;
                        machineinfo.AirSTPctLength = 8;
                        machineinfo.AirSTPctMiddleStart = 4;
                        machineinfo.AirSTPctMiddleTag = 0x42;
                        machineinfo.AirSTPctVStart = 5;

                        //状态获取命令回应包
                        machineinfo.GetStatusResPctDataWidth = 8;
                        machineinfo.GetStatusResPctDStart = 4;
                        machineinfo.GetStatusResPctEndStart = 16;
                        machineinfo.GetStatusResPctEndTag = 0x49;
                        machineinfo.GetStatusResPctLength = 17;
                        machineinfo.GetStatusResPctMiddleStart = 13;
                        machineinfo.GetStatusResPctMiddleTag = 0x48;
                        machineinfo.GetStatusResPctVStart = 14;

                        //普通命令回应包
                        machineinfo.NorCmdResPctDataWidth =3 ;
                        machineinfo.NorCmdResPctDStart = 2;
                        machineinfo.NorCmdResPctEndStart = 8;
                        machineinfo.NorCmdResPctEndTag = 0x49;
                        machineinfo.NorCmdResPctLength = 9;
                        machineinfo.NorCmdResPctMiddleStart = 5;
                        machineinfo.NorCmdResPctMiddleTag = 0x48;
                        machineinfo.NorCmdResPctVStart = 6;

                        machineinfo.DataWidth = 5;
                        machineinfo.SequenceLength = 6;
                        machineinfo.Type = 0x01;

                        rawText_bigpct_prt = st + MachineTypeHeaderLength;
                        break;
                }
                */
                #endregion

                #region phase2: analyze
                ////phase 2: 分析数据////
                CombineFragmentSg = delegate (byte[] rawit)
                {
                    PacketType out_ptype; PacketCombineStatus out_ftype;

                    if (!append(rawit)) return;
                    rawText_length += rawit.Length;

                    //串口上传过来的一片数据可能包含多个包，循环直到处理完该片中的所有包
                    CheckError errslt = CheckError.OK;
                    while(errslt == CheckError.OK || errslt == CheckError.DATA_FORMAT_ERROR || 
                            errslt == CheckError.PACKET_TPYE_ERROR)
                    {
                        errslt = check(rawText_frag_status, out out_ftype, out out_ptype);
                        switch (errslt)
                        {
                            case CheckError.OK: //成功组包，拨动指针。如果尾部没有其它数据添加回车换行符合
                                switch (out_ptype)
                                {
                                    case PacketType.CORRECT_RESPONSE:
                                        if (rawText_purepct_prt + machineinfo.CrtPctLength == rawText_length)
                                        {
                                            if (appendCRLF())
                                            {
                                                rawText_length += "\r\n".Length;
                                                rawText_bigpct_prt = rawText_length;
                                            }
                                        }
                                        else
                                        {
                                            rawText_bigpct_prt = (rawText_purepct_prt + machineinfo.CrtPctLength);
                                        }

                                        break;
                                    case PacketType.DATA_VALUE:
                                        if (rawText_purepct_prt + machineinfo.DataPctLength == rawText_length)
                                        {
                                            if (appendCRLF())
                                            {
                                                rawText_length += "\r\n".Length;
                                                rawText_bigpct_prt = rawText_length;
                                            }
                                        }
                                        else
                                        {
                                            rawText_bigpct_prt = (rawText_purepct_prt + machineinfo.DataPctLength);
                                        }

                                        break;
                                    case PacketType.VICE_DATA_VALUE:
                                        if (rawText_purepct_prt + machineinfo.ViceDataPctLength == rawText_length)
                                        {
                                            if (appendCRLF())
                                            {
                                                rawText_length += "\r\n".Length;
                                                rawText_bigpct_prt = rawText_length;
                                            }
                                        }
                                        else
                                        {
                                            rawText_bigpct_prt = (rawText_purepct_prt + machineinfo.ViceDataPctLength);
                                        }

                                        break;

                                    case PacketType.RES_COMPUTE_VALUE:
                                        if (rawText_purepct_prt + machineinfo.ResComputePctLength == rawText_length)
                                        {
                                            if (appendCRLF())
                                            {
                                                rawText_length += "\r\n".Length;
                                                rawText_bigpct_prt = rawText_length;
                                            }
                                        }
                                        else
                                        {
                                            rawText_bigpct_prt = (rawText_purepct_prt + machineinfo.ResComputePctLength);
                                        }
                                        break;
                                    case PacketType.SEQUENCE:
                                        if (rawText_purepct_prt + machineinfo.SrlPctLength == rawText_length)
                                        {
                                            if (appendCRLF())
                                            {
                                                rawText_length += "\r\n".Length;
                                                rawText_bigpct_prt = rawText_length;
                                            }
                                        }
                                        else
                                        {
                                            rawText_bigpct_prt = (rawText_purepct_prt + machineinfo.SrlPctLength);
                                        }

                                        break;
                                    case PacketType.AIR_FLUENT:
                                        if (rawText_purepct_prt + machineinfo.AirFluPctLength == rawText_length)
                                        {
                                            if (appendCRLF())
                                            {
                                                rawText_length += "\r\n".Length;
                                                rawText_bigpct_prt = rawText_length;
                                            }
                                        }
                                        else
                                        {
                                            rawText_bigpct_prt = (rawText_purepct_prt + machineinfo.AirFluPctLength);
                                        }
                                        break;
                                    case PacketType.AIR_SAMPLE_TIME:
                                        if (rawText_purepct_prt + machineinfo.AirSTPctLength == rawText_length)
                                        {
                                            if (appendCRLF())
                                            {
                                                rawText_length += "\r\n".Length;
                                                rawText_bigpct_prt = rawText_length;
                                            }
                                        }
                                        else
                                        {
                                            rawText_bigpct_prt = (rawText_purepct_prt + machineinfo.AirSTPctLength);
                                        }
                                        break;
                                    case PacketType.GETSTATUS_RESPONSE:
                                        if (rawText_purepct_prt + machineinfo.GetStatusResPctLength == rawText_length)
                                        {
                                            if (appendCRLF())
                                            {
                                                rawText_length += "\r\n".Length;
                                                rawText_bigpct_prt = rawText_length;
                                            }
                                        }
                                        else
                                        {
                                            rawText_bigpct_prt = (rawText_purepct_prt + machineinfo.GetStatusResPctLength);
                                        }
                                        break;

                                    case PacketType.NORCMD_RESPONSE:
                                        if (rawText_purepct_prt + machineinfo.NorCmdResPctLength == rawText_length)
                                        {
                                            if (appendCRLF())
                                            {
                                                rawText_length += "\r\n".Length;
                                                rawText_bigpct_prt = rawText_length;
                                            }
                                        }
                                        else
                                        {
                                            rawText_bigpct_prt = (rawText_purepct_prt + machineinfo.NorCmdResPctLength);
                                        }
                                        break;
                                }
                                break;
                            case CheckError.CONTINUE: //碎片，不要拨动指针
                                break;
                            case CheckError.DATA_FORMAT_ERROR: //抛弃，拨动指针，跳过这个坏包，发出事件
                                switch (out_ftype)
                                {
                                    case PacketCombineStatus.FRAGMENT_CORRECT:
                                        rawText_bigpct_prt = rawText_purepct_prt + machineinfo.CrtPctLength;
                                        if (CombineError_Ev != null) CombineError_Ev(CombineErrorInfo.CORRECT_PCT_DATA_FORMAT_ERROR);
                                        break;
                                    case PacketCombineStatus.FRAGMENT_SEQUENCE:
                                        rawText_bigpct_prt = rawText_purepct_prt + machineinfo.SrlPctLength;
                                        if (CombineError_Ev != null) CombineError_Ev(CombineErrorInfo.SEQUENCE_PCT_DATA_FORMAT_ERROR);
                                        break;
                                    case PacketCombineStatus.FRAGMENT_VALUE:
                                        rawText_bigpct_prt = rawText_purepct_prt + machineinfo.DataPctLength;
                                        if (CombineError_Ev != null) CombineError_Ev(CombineErrorInfo.VALUE_PCT_DATA_FORMAT_ERROR);
                                        break;
                                    case PacketCombineStatus.FRAGMENT_VICE_VALUE:
                                        rawText_bigpct_prt = rawText_purepct_prt + machineinfo.ViceDataPctLength;
                                        if (CombineError_Ev != null) CombineError_Ev(CombineErrorInfo.VICE_VALUE_PCT_DATA_FORMAT_ERROR);
                                        break;
                                    case PacketCombineStatus.FRAGMENT_RES_COMPUTE:
                                        rawText_bigpct_prt = rawText_purepct_prt + machineinfo.ResComputePctLength;
                                        if (CombineError_Ev != null) CombineError_Ev(CombineErrorInfo.RES_COMPUTE_VALUE_PCT_DATA_FORMAT_ERROR);
                                        break;
                                    case PacketCombineStatus.FRAGMENT_AIR_FLUENT:
                                        rawText_bigpct_prt = rawText_purepct_prt + machineinfo.AirFluPctLength;
                                        if (CombineError_Ev != null) CombineError_Ev(CombineErrorInfo.AIR_FLUENT_PCT_DATA_FORMAT_ERROR);
                                        break;
                                    case PacketCombineStatus.FRAGMENT_AIR_SAMPLE_TIME:
                                        rawText_bigpct_prt = rawText_purepct_prt + machineinfo.AirSTPctLength;
                                        if (CombineError_Ev != null) CombineError_Ev(CombineErrorInfo.AIR_SAMPLE_TIME_PCT_DATA_FORMAT_ERROR);
                                        break;
                                    case PacketCombineStatus.FRAGMENT_GETSTATUS_RESPONSE:
                                        rawText_bigpct_prt = rawText_purepct_prt + machineinfo.GetStatusResPctLength;
                                        if (CombineError_Ev != null) CombineError_Ev(CombineErrorInfo.GETSTATUS_RESPONSE_FORMAT_ERROR);
                                        break;
                                    case PacketCombineStatus.FRAGMENT_NORCMD_RESPONSE:
                                        rawText_bigpct_prt = rawText_purepct_prt + machineinfo.NorCmdResPctLength;
                                        if (CombineError_Ev != null) CombineError_Ev(CombineErrorInfo.NORCMD_RESPONSE_FORMAT_ERROR);
                                        break;
                                }
                                rawText_frag_status = PacketCombineStatus.OK; //抛弃格式错误包
                                break;
                            case CheckError.PACKET_TPYE_ERROR: ////抛弃，拨动指针，发出事件
                                rawText_bigpct_prt += 1; //跳过"S"
                                if (CombineError_Ev != null) CombineError_Ev(CombineErrorInfo.INVALID_PACKET_TYPE);
                                break;
                        }
                    }

                }; return ;
                #endregion
            };
        }
        public MachineInfo GetMachineInfo()
        {
            return machineinfo;
        }
        
        //清空rawtext数据
        public void Clear()
        {
            Array.Clear(rawText, 0, rawText_maxlength);
            rawText_bigpct_prt = 0;
            rawText_length = 0;
            rawText_purepct_prt = 0;
        }
        //将rawtext数据保存到文本之中
        public void Dump(string filename)
        {
            FileStream aFile = new FileStream(filename, FileMode.Create);
            StreamWriter sr = new StreamWriter(aFile);

            //sr.Write(rawText);
            for (int i = 0; i < rawText_length; i++)
            {
                sr.Write((char)rawText[i]);
            }

            sr.Close();
            sr = null;
            aFile.Close();
            aFile = null;
        }
        public void onPacketReceived(PacketReceviedDelegate handler)
        {
            PacketRecevied_Ev = handler;
        }
        public void onCombineError(CombineErrorDelegate handler)
        {
            CombineError_Ev = handler;
        }
        public void CombineFragment(byte[] rawitem)
        {
            CombineFragmentSg(rawitem);
        }

        //检查包的完整性,寻找包的起始位置和结束位置。
        //算法提示- rawText_bigpct_prt。
        //@param
        //  返回值 - true，是个完整包。其它情况，fasle。
        //  
        //  cmbstus - 上一个包状态
        //  fragtype - 碎片类型
        //  ptype - 包类型
        //  packetstart - 纯包开始位置，相对于rawText_bigpct_prt
        private CheckError check(PacketCombineStatus cmbstus, out PacketCombineStatus fragtype , out PacketType ptype)
        {
            ptype = PacketType.UNKNOWN;
            fragtype = cmbstus;
            
            switch (cmbstus)
            {
                case PacketCombineStatus.OK:  //新包或格式未知道碎片
                case PacketCombineStatus.FRAGMENT_UNKONWN:
                    return check_unknown_type_fn(out fragtype, out ptype);
                    
                case PacketCombineStatus.FRAGMENT_CORRECT:
                case PacketCombineStatus.FRAGMENT_SEQUENCE:
                case PacketCombineStatus.FRAGMENT_VALUE:
                case PacketCombineStatus.FRAGMENT_VICE_VALUE:
                case PacketCombineStatus.FRAGMENT_RES_COMPUTE:
                case PacketCombineStatus.FRAGMENT_AIR_FLUENT:
                case PacketCombineStatus.FRAGMENT_AIR_SAMPLE_TIME:
                case PacketCombineStatus.FRAGMENT_GETSTATUS_RESPONSE:
                case PacketCombineStatus.FRAGMENT_NORCMD_RESPONSE:
                    return check_known_type_fn(cmbstus,out fragtype, out ptype);
                    
            }
            return CheckError.CONTINUE;
        }


        //检查格式未知包
        //
        //
        //  
        // @param
        // 
        private CheckError check_unknown_type_fn(out PacketCombineStatus fragtype, out PacketType ptype)
        {
            ptype = PacketType.UNKNOWN;
            fragtype = PacketCombineStatus.FRAGMENT_UNKONWN;

            //搜索包起始标志, abs表示相对于rawText的起点，
            int start_abs = -1;
            for (int i = rawText_bigpct_prt; i< rawText_length; i ++)
            {
                if(rawText[i] == 0x46)  //找到起始标志F
                {
                    start_abs = i;
                    break;
                }
            }

            //找不到F起始标志，继续拼包
            if (start_abs == -1)
            {
                if(start_abs > 1000)  //长时间无法找到起始标志, 碎片过大， 发出错误事件
                {
                    if(CombineError_Ev != null) CombineError_Ev(CombineErrorInfo.NOT_FOUND_START_TAG_LONG);
                    return CheckError.CONTINUE;
                }
                else
                {
                    return CheckError.CONTINUE;
                }
            }

            //F在rawtext最后，继续拼包
            if(start_abs == rawText_length - 1) //F标志在rawtext的最后一位，仍然是无法识别包格式
            {
                return CheckError.CONTINUE;
            }

            //找到起始标志，识别包格式
            rawText_purepct_prt = start_abs;
            switch (rawText[rawText_purepct_prt + 1])
            {
                case 0x47: //'G',数据包, H, I
                    return cut(out ptype, out fragtype,PacketType.DATA_VALUE,true,0x48,
                        machineinfo.DataPctMiddleTag,0x49,machineinfo.DataPctEndTag,machineinfo.DataPctLength);
                case 0x55: //'U',数据包, H, I
                    return cut(out ptype, out fragtype, PacketType.VICE_DATA_VALUE, true, 0x48,
                        machineinfo.ViceDataPctMiddleTag, 0x49, machineinfo.ViceDataPctEndTag, machineinfo.ViceDataPctLength);
                case 0x58   : //'X' 回应计算包, H, I
                    return cut(out ptype, out fragtype, PacketType.RES_COMPUTE_VALUE, true, 0x48,
                        machineinfo.ResComputePctMiddleTag, 0x49, machineinfo.ResComputePctEndTag, machineinfo.ResComputePctLength);
                case 0x53: //'S',序号包 ,E
                    return cut(out ptype, out fragtype, PacketType.SEQUENCE, false, 0x0,
                        0, 0x45, machineinfo.SrlPctEndTag, machineinfo.SrlPctLength);
                case 0x43: //'C', 纠正包,0x4d, 0x54
                    return cut(out ptype, out fragtype, PacketType.CORRECT_RESPONSE, true, 0x4d,
                        machineinfo.CrtPctMiddleTag, 0x54, machineinfo.CrtPctEndTag, machineinfo.CrtPctLength);
                case 0x41: //'A' 气体取样时间包
                    return cut(out ptype, out fragtype, PacketType.AIR_SAMPLE_TIME, true, (byte)machineinfo.AirSTPctMiddleTag,
                        machineinfo.AirSTPctMiddleStart, (byte)machineinfo.AirSTPctEndTag, machineinfo.AirSTPctEndStart, machineinfo.AirSTPctLength);
                case 0x44: //'D' 气体流量包
                    return cut(out ptype, out fragtype, PacketType.AIR_FLUENT, true, (byte)machineinfo.AirFluPctMiddleTag,  
                        machineinfo.AirFluPctMiddleStart, (byte)machineinfo.AirFluPctEndTag, machineinfo.AirFluPctEndStart, machineinfo.AirFluPctLength);
                case 0x4A: //'J' 状态获取命令回应包
                    return cut(out ptype, out fragtype, PacketType.GETSTATUS_RESPONSE, true, (byte)machineinfo.GetStatusResPctMiddleTag,
                        machineinfo.GetStatusResPctMiddleStart, (byte)machineinfo.GetStatusResPctEndTag, machineinfo.GetStatusResPctEndStart, machineinfo.GetStatusResPctLength);
                case 0x5A:  //'Z'普通命令回应包
                    return cut(out ptype, out fragtype, PacketType.NORCMD_RESPONSE, true, (byte)machineinfo.NorCmdResPctMiddleTag,
                        machineinfo.NorCmdResPctMiddleStart, (byte)machineinfo.NorCmdResPctEndTag, machineinfo.NorCmdResPctEndStart, machineinfo.NorCmdResPctLength);
                default:  //无法识别包的类型
                    ptype = PacketType.UNKNOWN;
                    fragtype = PacketCombineStatus.FRAGMENT_UNKONWN;
                    return CheckError.PACKET_TPYE_ERROR;
            }
        }

        //检查格式已知包
        //@param
        // 
        private CheckError check_known_type_fn(PacketCombineStatus cmbstu, out PacketCombineStatus fragtype, out PacketType ptype)
        {
            ptype = PacketType.UNKNOWN;
            fragtype = PacketCombineStatus.FRAGMENT_UNKONWN;

            switch (cmbstu)
            {
                case PacketCombineStatus.FRAGMENT_VALUE: //'G',数据包
                    return cut(out ptype, out fragtype, PacketType.DATA_VALUE, true, 0x48,
                        machineinfo.DataPctMiddleTag, 0x49, machineinfo.DataPctEndTag, machineinfo.DataPctLength);
                case PacketCombineStatus.FRAGMENT_VICE_VALUE: //'U',数据包
                    return cut(out ptype, out fragtype, PacketType.VICE_DATA_VALUE, true, 0x48,
                        machineinfo.ViceDataPctMiddleTag, 0x49, machineinfo.ViceDataPctEndTag, machineinfo.ViceDataPctLength);
                case PacketCombineStatus.FRAGMENT_RES_COMPUTE: //'X' 回应计算包
                    return cut(out ptype, out fragtype, PacketType.RES_COMPUTE_VALUE, true, 0x48,
                        machineinfo.ResComputePctMiddleTag, 0x49, machineinfo.ResComputePctEndTag, machineinfo.ResComputePctLength);
                case PacketCombineStatus.FRAGMENT_SEQUENCE : //'S',序号包
                    return cut(out ptype, out fragtype, PacketType.SEQUENCE, false, 0x0,
                          0, 0x45, machineinfo.SrlPctEndTag, machineinfo.SrlPctLength);
                case PacketCombineStatus.FRAGMENT_CORRECT: //'C', 纠正包
                    return cut(out ptype, out fragtype, PacketType.CORRECT_RESPONSE, true, 0x4d,
                        machineinfo.CrtPctMiddleTag, 0x54, machineinfo.CrtPctEndTag, machineinfo.CrtPctLength);
                case PacketCombineStatus.FRAGMENT_AIR_FLUENT:
                    return cut(out ptype, out fragtype, PacketType.AIR_FLUENT, true, (byte)machineinfo.AirFluPctMiddleTag,
                        machineinfo.AirFluPctMiddleStart, (byte)machineinfo.AirFluPctEndTag, machineinfo.AirFluPctEndStart, machineinfo.AirFluPctLength);
                case PacketCombineStatus.FRAGMENT_AIR_SAMPLE_TIME:
                    return cut(out ptype, out fragtype, PacketType.AIR_SAMPLE_TIME, true, (byte)machineinfo.AirSTPctMiddleTag,
                        machineinfo.AirSTPctMiddleStart, (byte)machineinfo.AirSTPctEndTag, machineinfo.AirSTPctEndStart, machineinfo.AirSTPctLength);
                case PacketCombineStatus.FRAGMENT_GETSTATUS_RESPONSE:
                    return cut(out ptype, out fragtype, PacketType.GETSTATUS_RESPONSE, true, (byte)machineinfo.GetStatusResPctMiddleTag,
                        machineinfo.GetStatusResPctMiddleStart, (byte)machineinfo.GetStatusResPctEndTag, machineinfo.GetStatusResPctEndStart, machineinfo.GetStatusResPctLength);
                case PacketCombineStatus.FRAGMENT_NORCMD_RESPONSE:
                    return cut(out ptype, out fragtype, PacketType.NORCMD_RESPONSE, true, (byte)machineinfo.NorCmdResPctMiddleTag,
                        machineinfo.NorCmdResPctMiddleStart, (byte)machineinfo.NorCmdResPctEndTag, machineinfo.NorCmdResPctEndStart, machineinfo.NorCmdResPctLength);
            }
            return CheckError.CONTINUE;
        }

        /*
         * 截取纯包出来
         */
        private CheckError cut(out PacketType out_ptye, out PacketCombineStatus out_fragtype, PacketType in_ptype,
            bool hasMiddleTag, byte middleTag, int middletag_rel_position,
            byte eTag, int endtag_rel_position, int purepctlength)
        {
            out_fragtype = getFragType(in_ptype);
            if (rawText_purepct_prt + purepctlength - 1 > rawText_length - 1) //碎片，继续拼接
            {
                out_ptye = in_ptype;
                out_fragtype = getFragType(in_ptype);
                return CheckError.CONTINUE;
            }
            //验证标志中间标志
            if (hasMiddleTag == true)
            {
                if (rawText[rawText_purepct_prt + middletag_rel_position] != middleTag)
                {
                    out_ptye = in_ptype;
                    //out_fragtype = PacketCombineStatus.OK;      //格式错误，抛弃数据
                    if(CombineError_Ev != null) CombineError_Ev(getDataFormatErrorType(in_ptype));
                    return CheckError.DATA_FORMAT_ERROR;
                }
            }
            //验证结束标志
            if (rawText[rawText_purepct_prt + endtag_rel_position] != eTag)
            {
                out_ptye = in_ptype;
                //out_fragtype = PacketCombineStatus.OK;      //格式错误，抛弃数据
                if(CombineError_Ev != null) CombineError_Ev(getDataFormatErrorType(in_ptype));
                return CheckError.DATA_FORMAT_ERROR;
            }
            //祝贺，拼接除了一个完整的包!
            out_ptye = in_ptype;
            out_fragtype = PacketCombineStatus.OK;
            PacketRecevied_Ev(getBuffer(rawText_purepct_prt,
                rawText_purepct_prt + purepctlength - 1), in_ptype);
            return CheckError.OK;
        }

        private CombineErrorInfo getDataFormatErrorType(PacketType ptype)
        {
            switch (ptype)
            {
                case PacketType.DATA_VALUE:
                    return CombineErrorInfo.VALUE_PCT_DATA_FORMAT_ERROR;
                case PacketType.VICE_DATA_VALUE:
                    return CombineErrorInfo.VICE_VALUE_PCT_DATA_FORMAT_ERROR;
                case PacketType.RES_COMPUTE_VALUE:
                    return CombineErrorInfo.RES_COMPUTE_VALUE_PCT_DATA_FORMAT_ERROR;
                case PacketType.CORRECT_RESPONSE:
                    return CombineErrorInfo.CORRECT_PCT_DATA_FORMAT_ERROR;
                case PacketType.SEQUENCE:
                    return CombineErrorInfo.SEQUENCE_PCT_DATA_FORMAT_ERROR;
                case PacketType.AIR_FLUENT:
                    return CombineErrorInfo.AIR_FLUENT_PCT_DATA_FORMAT_ERROR;
                case PacketType.AIR_SAMPLE_TIME:
                    return CombineErrorInfo.AIR_SAMPLE_TIME_PCT_DATA_FORMAT_ERROR;
                case PacketType.GETSTATUS_RESPONSE:
                    return CombineErrorInfo.GETSTATUS_RESPONSE_FORMAT_ERROR;
                case PacketType.NORCMD_RESPONSE:
                    return CombineErrorInfo.NORCMD_RESPONSE_FORMAT_ERROR;
                default:
                    return CombineErrorInfo.UNKNOWN;
            }
        }

        private PacketCombineStatus getFragType(PacketType ptype)
        {
            switch (ptype)
            {
                case PacketType.DATA_VALUE:
                    return PacketCombineStatus.FRAGMENT_VALUE;
                case PacketType.VICE_DATA_VALUE:
                    return PacketCombineStatus.FRAGMENT_VICE_VALUE;
                case PacketType.RES_COMPUTE_VALUE:
                    return PacketCombineStatus.FRAGMENT_RES_COMPUTE;
                case PacketType.CORRECT_RESPONSE:
                    return PacketCombineStatus.FRAGMENT_CORRECT;
                case PacketType.SEQUENCE:
                    return PacketCombineStatus.FRAGMENT_SEQUENCE;
                case PacketType.AIR_FLUENT:
                    return PacketCombineStatus.FRAGMENT_AIR_FLUENT;
                case PacketType.AIR_SAMPLE_TIME:
                    return PacketCombineStatus.FRAGMENT_AIR_SAMPLE_TIME;
                case PacketType.GETSTATUS_RESPONSE:
                    return PacketCombineStatus.FRAGMENT_GETSTATUS_RESPONSE;
                case PacketType.NORCMD_RESPONSE:
                    return PacketCombineStatus.FRAGMENT_NORCMD_RESPONSE;
                default:
                    return PacketCombineStatus.FRAGMENT_UNKONWN;
            }
        }


        //返回一段数据
        private byte[] getBuffer(int start_index, int end_index)
        {
            byte[] t = new byte[end_index - start_index + 1];
            for (int i = start_index; i <= end_index; i++)
            {
                t[i - start_index] = rawText[i];
            }
            return t;
        }


        //将串口上收到的数据粘贴到rawtext上。
        private bool append(byte[] rawitem)
        {
            if(rawText_length -1 + rawitem.Length < rawText_maxlength)
            {
                for(int i = 0; i < rawitem.Length; i++)
                {
                    rawText[rawText_length + i] = rawitem[i];
                }
                return true;
            }
            {
                return false;
            }
       
        }

        // 找到一个完整包后向rawtext添加\r\n，便于阅读
        private bool appendCRLF()
        {
            if (rawText_length + 2 > rawText_maxlength)
                return false;
            rawText[rawText_length] = 0x0d; // \r
            rawText[rawText_length] = 0x0a; // \n
            return true;
        }
    }
}
