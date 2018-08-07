using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilverTest.libs
{
    /* 物理层，singleton, 将串口数据片段合并成一个完整的包
     * 术语
     *    完整包 - 一个完整的包
     *    新包 - 上次已经拼接成功一个完整包。这次收到的包叫新包。新包check后，可能是个完整包，也可能
     *              是个碎片
     *    碎片 - 完整包的一部分。如果上次收到是个碎片，本次收到的一定也是碎片
     *    拼接 - 将碎片拼接成完整包
     *    净包 - 将完整包中里面的无用的头尾去掉，得到净包
     */
    public class PhyCombine
    {

        public delegate void PacketReceviedDelegate(byte[] packet, PacketType ptype);
        public delegate void CombineErrorDelegate(CombineErrorInfo err);


        private delegate void CombineFragmentDelegate(byte[] rawitem); // 自抛代码原型


        public class MachineInfo
        {
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
        }

        //数据包类型
        public enum PacketType
        {
            MACHINE_TYPE,               //机器类型
            DATA_VALUE,                 //机器采样到的一个数据值
            SEQUENCE,                   //数据值的序号包
            CORRECT_RESPONSE,           //校验出错重发命令的响应包
            FRAGMENT,                   //碎片
            UNKNOWN                     //未知格式
        }

        //上一个包拼接的状态
        private enum PacketCombineStatus {
                                    OK,                         // OK - 包是完整的包。
                                    FRAGMENT_MACHINE_TYPE,      //包是个机器信息头碎片
                                    FRAGMENT_VALUE,             //包是一个数据值碎片
                                    FRAGMENT_SEQUENCE,          //包是一个序号碎片
                                    FRAGMENT_CORRECT,           //包是校验出错响应包碎片
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
            DATA_FORMAT_ERROR,          //包的格式有错，找不到包的结束位置,和中间标志位置
            INVALID_PACKET_TYPE,            //非法的包类型
        }

        private byte[] rawText;
        private int rawText_okprt = 0;  //指向碎片的首部
        private int rawText_length = 0; //rawText 的长度，
        private readonly int rawText_maxlength = 1000000;
        private PacketCombineStatus rawText_frag_status = PacketCombineStatus.OK; //上一个包的状态
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
        private PhyCombine()
        {
            rawText = new byte[rawText_maxlength];
            Array.Clear(rawText, 0, rawText_maxlength);
            machineinfo = new MachineInfo();

            CombineFragmentSg = delegate (byte[] rawitem)
            {
                ////phase 1: 获取机器格式头////
                int st = -1;

                append(rawitem);
                rawText_length += rawitem.Length;

                for (int i = rawText_okprt; i < rawText_length; i++)  //查找包起始标志
                {
                    if (rawText[i] == 0x46)  //找到'F'
                    {
                        st = i;
                        break;
                    }
                }
                if (st == -1 && rawText_length - rawText_okprt > 1000)  //长时间不能收到机器类型包
                {
                    Console.WriteLine("长时间不能收到机器类型包");
                    CombineError_Ev(CombineErrorInfo.NOT_FOUND_MACHINE_HEADER_LONG);

                    return;
                }
                if (st == -1 ||(st + 4 > rawText_length -1))  //找不到起始标志'F', 或者碎片太短，继续拼包
                {
                    return;
                }
                if(rawText[st+1] != 0x01)   //未能识别的机器类型
                {
                    Console.WriteLine("未能识别的机器类型");
                    CombineError_Ev(CombineErrorInfo.INVALID_MACHINE_TYPE);
                    return;
                }

                //找到机器类型包，准备自弃
                switch(rawText[st+1])
                {
                    case 0x01:
                        machineinfo.CrtPctEndTag = 16;
                        machineinfo.CrtPctLength = 17;
                        machineinfo.CrtPctMiddleTag = 7;


                        machineinfo.DataPctEndTag = 8;
                        machineinfo.DataPctLength = 9;
                        machineinfo.DataPctMiddleTag = 0;

                        machineinfo.SrlPctEndTag = 3;
                        machineinfo.SrlPctLength = 4;

                        machineinfo.DataWidth = 5;
                        machineinfo.SequenceLength = 6;
                        machineinfo.Type = 0x01;

                        appendCRLF();
                        rawText_length += "\r\n".Length; //为完整包加上回车换行符，便于阅读
                        rawText_okprt = rawText_length;
                        break;
                }

                ////phase 2: 分析数据////
                CombineFragmentSg = delegate (byte[] rawit)
                {
                    PacketType out_ptype; PacketCombineStatus out_ftype;int out_packstart;

                    append(rawit);
                    rawText_length += rawit.Length;

                    switch(check(rawit, rawText_frag_status,out out_packstart, out out_ftype, out out_ptype))
                    {
                        case CheckError.OK: //成功组包，拨动指针。如果尾部没有其它数据添加回车换行符合
                            //appendCRLF();
                            //rawText_length += 2;
                            switch (out_ptype)
                            {
                                case PacketType.CORRECT_RESPONSE:
                                    if(rawText_okprt + out_packstart +machineinfo.CrtPctLength == rawText_length){
                                        appendCRLF();
                                        rawText_length += "\r\n".Length;
                                        rawText_okprt = rawText_length;
                                    }
                                    else
                                    {
                                        rawText_okprt += (out_packstart + machineinfo.CrtPctLength);
                                    }

                                    break;
                                case PacketType.DATA_VALUE:
                                    if (rawText_okprt + out_packstart + machineinfo.DataPctLength == rawText_length)
                                    {
                                        appendCRLF();
                                        rawText_length += "\r\n".Length;
                                        rawText_okprt = rawText_length;
                                    }
                                    else
                                    {
                                        rawText_okprt += (out_packstart + machineinfo.DataPctLength);
                                    }

                                    break;
                                case PacketType.SEQUENCE:
                                    if (rawText_okprt + out_packstart + machineinfo.SrlPctLength == rawText_length)
                                    {
                                        appendCRLF();
                                        rawText_length += "\r\n".Length;
                                        rawText_okprt = rawText_length;
                                    }
                                    else
                                    {
                                        rawText_okprt += (out_packstart + machineinfo.SrlPctLength);
                                    }

                                    break;
                            }
                            
                            break;
                        case CheckError.CONTINUE: //碎片，不要拨动指针
                            break;
                        case CheckError.DATA_FORMAT_ERROR: //抛弃，拨动指针，跳过这个坏包，不通知DataFormater
                            switch (out_ftype)
                            {
                                case PacketCombineStatus.FRAGMENT_CORRECT:
                                    rawText_okprt += out_packstart + machineinfo.CrtPctLength;
                                    break;
                                case PacketCombineStatus.FRAGMENT_SEQUENCE:
                                    rawText_okprt += out_packstart + machineinfo.SrlPctLength;
                                    break;
                                case PacketCombineStatus.FRAGMENT_VALUE:
                                    rawText_okprt += out_packstart + machineinfo.DataPctLength;
                                    break;
                            }
                            
                            break;
                        case CheckError.PACKET_TPYE_ERROR: ////抛弃，拨动指针，不通知DataFormater
                            rawText_okprt += 1; //跳过"S"
                            break;

                    }
                }; return ;

            };
        }
        public MachineInfo GetMachineInfo()
        {
            return machineinfo;
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
        //算法提示- 不要移动rawtext_okpr。
        //@param
        //  返回值 - true，是个完整包。其它情况，fasle。
        //  data - 从包头开始包rawtext_length结尾之间的数据
        //  cmbstus - 上一个包状态
        //  fragtype - 碎片类型
        //  ptype - 包类型
        //  packetstart - 纯包开始位置，相对于rawtext_okprt
        private CheckError check(byte[] data,  PacketCombineStatus cmbstus, out int packetstart, out PacketCombineStatus fragtype , out PacketType ptype)
        {
            ptype = PacketType.UNKNOWN;
            fragtype = cmbstus;
            packetstart = -1;

            switch (cmbstus)
            {
                case PacketCombineStatus.OK:  //新包或格式未知道碎片
                case PacketCombineStatus.FRAGMENT_UNKONWN:
                    return check_unknown_type_fn(data, out packetstart, out fragtype, out ptype);
                    
                case PacketCombineStatus.FRAGMENT_CORRECT:
                case PacketCombineStatus.FRAGMENT_SEQUENCE:
                case PacketCombineStatus.FRAGMENT_VALUE:
                    return check_known_type_fn(data, out packetstart, cmbstus,out fragtype, out ptype);
                    
            }
            return CheckError.CONTINUE;
        }
        //检查格式未知包
        //返回值
        //   - 获得完整包
        //  false - 碎片。格式可能能够识别，也可能不能识别
        // @param
        //      packetstart - 净包起始位置，以rawtext_okprt为参考
        private CheckError check_unknown_type_fn(byte[] data, out int packetstart,out PacketCombineStatus fragtype, out PacketType ptype)
        {
            ptype = PacketType.UNKNOWN;
            fragtype = PacketCombineStatus.FRAGMENT_UNKONWN;
            packetstart = -1;

            //搜索包起始标志
            int start = -1;
            for (int i = rawText_okprt; i< rawText_length; i ++)
            {
                if(rawText[i] == 0x46 && i < rawText_length -1)  //找到起始标志F
                {
                    start = i;
                    break;
                }
            }

            //找不到F起始标志，继续拼包
            if (start == -1)
            {
                if(start > 1000)  //长时间无法找到起始标志, 发出错误事件
                {
                    CombineError_Ev(CombineErrorInfo.NOT_FOUND_START_TAG_LONG);
                    return CheckError.CONTINUE;
                }
                else
                {
                    return CheckError.CONTINUE;
                }
            }

            //F在rawtext最后，继续拼包
            if(rawText_okprt + start  == rawText_length - 1) //F标志在rawtext的最后一位，仍然是无法包格式
            {
                return CheckError.CONTINUE;
            }

            //找到起始标志，识别包格式
            switch (rawText[rawText_okprt + start])
            {
                case 0x47: //'G',数据包
                    if(rawText_okprt + start + machineinfo.DataPctLength  -1 > rawText_length - 1) //碎片，继续拼接
                    {
                        ptype = PacketType.DATA_VALUE;
                        fragtype = PacketCombineStatus.FRAGMENT_VALUE;
                        return CheckError.CONTINUE;
                    }
                    else
                    {
                        //找不到数据包的H, I标志，格式错误
                        if (rawText[rawText_okprt+ start +machineinfo.DataPctMiddleTag]!= 0x48 
                            || rawText[rawText_okprt+ start +machineinfo.DataPctEndTag] != 0x49)
                            
                        {
                            ptype = PacketType.DATA_VALUE;
                            fragtype = PacketCombineStatus.OK;
                            packetstart = start;
                            CombineError_Ev(CombineErrorInfo.DATA_FORMAT_ERROR);
                            return CheckError.DATA_FORMAT_ERROR;
                        }
                        //拼接出了一个完整的包
                        ptype = PacketType.DATA_VALUE;
                        fragtype = PacketCombineStatus.OK;
                        packetstart = start;
                        PacketRecevied_Ev(getBuffer(rawText_okprt+start, 
                            rawText_okprt+start+machineinfo.DataPctLength - 1),PacketType.DATA_VALUE);
                        return CheckError.OK;
                    }
                    break;
                case 0x53: //'S',序号包
                    if (rawText_okprt + start + machineinfo.SrlPctLength - 1 > rawText_length - 1) //碎片，继续拼接
                    {
                        ptype = PacketType.SEQUENCE;
                        fragtype = PacketCombineStatus.FRAGMENT_SEQUENCE;
                        return CheckError.CONTINUE;
                    }
                    else
                    {
                        //找不到序号包的结束标志E，格式错误
                        if (rawText[rawText_okprt + start + machineinfo.SrlPctEndTag] != 0x45)
                        {
                            ptype = PacketType.SEQUENCE;
                            fragtype = PacketCombineStatus.OK;
                            packetstart = start;
                            CombineError_Ev(CombineErrorInfo.DATA_FORMAT_ERROR);
                            return CheckError.DATA_FORMAT_ERROR;
                        }
                        //拼接出了一个完整的包
                        ptype = PacketType.SEQUENCE;
                        fragtype = PacketCombineStatus.OK;
                        packetstart = start;
                        PacketRecevied_Ev(getBuffer(rawText_okprt + start,
                            rawText_okprt + start +machineinfo.SrlPctLength - 1), PacketType.SEQUENCE);
                        return CheckError.OK;
                    }
                    break;
                case 0x43: //'C', 纠正包
                    if (rawText_okprt + start + machineinfo.CrtPctLength - 1 > rawText_length - 1) //碎片，继续拼接
                    {
                        ptype = PacketType.CORRECT_RESPONSE;
                        fragtype = PacketCombineStatus.FRAGMENT_CORRECT;
                        return CheckError.CONTINUE;
                    }
                    else
                    {
                        //找不到序号包的中间标志M 0x4d，结束标志T 0x54，格式错误
                        if (rawText[rawText_okprt + start + machineinfo.CrtPctMiddleTag] != 0x4d 
                                || rawText[rawText_okprt + start + machineinfo.CrtPctEndTag] != 0x54)
                        {
                            ptype = PacketType.CORRECT_RESPONSE;
                            fragtype = PacketCombineStatus.OK;
                            packetstart = start;
                            CombineError_Ev(CombineErrorInfo.DATA_FORMAT_ERROR);
                            return CheckError.DATA_FORMAT_ERROR;
                        }

                        //拼接出了一个完整的包
                        ptype = PacketType.CORRECT_RESPONSE;
                        fragtype = PacketCombineStatus.OK;
                        packetstart = start;
                        PacketRecevied_Ev(getBuffer(rawText_okprt + start,
                                rawText_okprt + start + machineinfo.CrtPctLength - 1), PacketType.CORRECT_RESPONSE);
                        return CheckError.OK;
                    }
                    break;
                default:  //无法识别包的类型
                    ptype = PacketType.UNKNOWN;
                    fragtype = PacketCombineStatus.FRAGMENT_UNKONWN;
                    CombineError_Ev(CombineErrorInfo.INVALID_PACKET_TYPE);
                    return CheckError.PACKET_TPYE_ERROR;
            }
            return CheckError.CONTINUE; //默认继续拼包
        }

        //返回一段数据
        private byte[] getBuffer(int start_index, int end_index)
        {
            byte[] t = new byte[end_index - start_index + 1];
            for(int i = start_index; i<end_index;i++)
            {
                t[i-start_index] = rawText[i];
            }
            return t;
        }

        //检查格式已知包
        //@param
        // - packetstart 净包开始的位置
        private CheckError check_known_type_fn(byte[] data, out int packetstart , PacketCombineStatus cmbstu, out PacketCombineStatus fragtype, out PacketType ptype)
        {
            ptype = PacketType.UNKNOWN;
            fragtype = PacketCombineStatus.FRAGMENT_UNKONWN;
            packetstart = -1;
            int start = -1;

            switch (cmbstu)
            {
                case PacketCombineStatus.FRAGMENT_VALUE: //'G',数据包
                    for(int i = 0;i < machineinfo.DataPctLength; i++ )
                    {
                        if(rawText[rawText_okprt + i] == 0x46)  //找到起始标识 F
                        {
                            start = i;
                        }
                    }
                    if (rawText_okprt + start + machineinfo.DataPctLength - 1 > rawText_length - 1) //碎片，继续拼接
                    {
                        ptype = PacketType.DATA_VALUE;
                        fragtype = PacketCombineStatus.FRAGMENT_VALUE;
                        return CheckError.CONTINUE;
                    }
                    else
                    {
                        //找不到数据包的H, I标志，格式错误
                        if (rawText[rawText_okprt + start + machineinfo.DataPctMiddleTag] != 0x48
                            || rawText[rawText_okprt + start + machineinfo.DataPctEndTag] != 0x49)

                        {
                            ptype = PacketType.DATA_VALUE;
                            fragtype = PacketCombineStatus.OK;
                            packetstart = start;
                            CombineError_Ev(CombineErrorInfo.DATA_FORMAT_ERROR);
                            return CheckError.DATA_FORMAT_ERROR;
                        }
                        //拼接出了一个完整的包
                        ptype = PacketType.DATA_VALUE;
                        fragtype = PacketCombineStatus.OK;
                        packetstart = start;
                        return CheckError.OK;
                    }
                    break;
                case PacketCombineStatus.FRAGMENT_SEQUENCE : //'S',序号包
                    for (int i = 0; i < machineinfo.SrlPctLength; i++)
                    {
                        if (rawText[rawText_okprt + i] == 0x46)  //找到起始标识 F
                        {
                            start = i;
                        }
                    }
                    if (rawText_okprt + start +  machineinfo.SrlPctLength - 1 > rawText_length - 1) //碎片，继续拼接
                    {
                        ptype = PacketType.SEQUENCE;
                        fragtype = PacketCombineStatus.FRAGMENT_SEQUENCE;
                        return CheckError.CONTINUE;
                    }
                    else
                    {
                        //找不到序号包的结束标志E，格式错误
                        if (rawText[rawText_okprt + start+ machineinfo.SrlPctEndTag] != 0x45)
                        {
                            ptype = PacketType.SEQUENCE;
                            fragtype = PacketCombineStatus.OK;
                            packetstart = start;
                            CombineError_Ev(CombineErrorInfo.DATA_FORMAT_ERROR);
                            return CheckError.DATA_FORMAT_ERROR;
                        }
                        //拼接出了一个完整的包
                        ptype = PacketType.SEQUENCE;
                        fragtype = PacketCombineStatus.OK;
                        packetstart = start;
                        return CheckError.OK;
                    }
                    break;
                case PacketCombineStatus.FRAGMENT_CORRECT: //'C', 纠正包
                    for (int i = 0; i < machineinfo.CrtPctLength; i++)
                    {
                        if (rawText[rawText_okprt + i] == 0x46)  //找到起始标识 F
                        {
                            start = i;
                        }
                    }
                    if (rawText_okprt +start+ machineinfo.CrtPctLength - 1 > rawText_length - 1) //碎片，继续拼接
                    {
                        ptype = PacketType.CORRECT_RESPONSE;
                        fragtype = PacketCombineStatus.FRAGMENT_CORRECT;
                        return CheckError.CONTINUE;
                    }
                    else
                    {
                        //找不到序号包的中间标志M 0x4d，结束标志T 0x54，格式错误
                        if (rawText[rawText_okprt +start+ machineinfo.CrtPctMiddleTag] != 0x4d
                                || rawText[rawText_okprt +start+ machineinfo.CrtPctEndTag] != 0x54)
                        {
                            ptype = PacketType.CORRECT_RESPONSE;
                            fragtype = PacketCombineStatus.OK;
                            packetstart = start;
                            CombineError_Ev(CombineErrorInfo.DATA_FORMAT_ERROR);
                            return CheckError.DATA_FORMAT_ERROR;
                        }

                        //拼接出了一个完整的包
                        ptype = PacketType.CORRECT_RESPONSE;
                        fragtype = PacketCombineStatus.OK;
                        packetstart = start;
                        return CheckError.OK;
                    }
                    break;
            }
            return CheckError.CONTINUE;
        }

        //将串口上收到的数据粘贴到rawtext上。
        private void append(byte[] rawitem)
        {
            if(rawText_okprt + rawitem.Length < rawText.Length)
            {
                for(int i = 0; i < rawitem.Length; i++)
                {
                    rawText[rawText_okprt + i] = rawitem[i];
                }
            }
        }

        // 找到一个完整包后向rawtext添加\r\n，便于阅读
        private void appendCRLF()
        {
            if (rawText_length + 2 < rawText_maxlength)
                return;
            rawText[rawText_length] = 0x0d; // \r
            rawText[rawText_length] = 0x0a; // \n
        }
    }
}
