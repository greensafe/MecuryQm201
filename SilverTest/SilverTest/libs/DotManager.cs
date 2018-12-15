using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using static SilverTest.libs.DataFormater;
using static SilverTest.libs.PhyCombine;

namespace SilverTest.libs
{
    /*
     * 串口数据分析对外交互类。管理DataFormater, PhyCombine。 
     * 制定错误处理策略
     * singleton对象
     */
    public class DotManager
    {
        #region I_not_want_see
        private static DotManager onlyme = null;
        public delegate void PacketReceviedDelegate(object dot, int sequence, PacketType ptype);  //收到一个包，包括数据包，气体流量包，气体采样时间包
        public delegate void PacketCorrectedDelegate(ADot dot, int sequence); //获得一个校验dot

        enum correctstatus
        {
            CORRECTING,
            CORRECTED,
            FAIL,

        }
        private class CorrectItem
        {
            public int retrycount { get; set; } //重试次数
            public correctstatus status { get; set; }  //纠正状态
            public int seq { get; set; }    //点值序号
            public DispatcherTimer mytimer{ get; set; } //定时器
        }
        

        PacketReceviedDelegate PacketRecevied_Ev = null;
        PacketCorrectedDelegate PacketCorrected_Ev = null;

        List<CorrectItem> correctitems;
        private readonly int correctstrategy_retry = 3;
        private readonly int correctstrategy_timespan = 100; //10 毫秒

        private DotManager()
        {
            correctitems = new List<CorrectItem>();

            PhyCombine.GetPhyCombine().onPacketReceived(DataFormater.getDataFormater().PacketReceviedHdlr);

            DataFormater.getDataFormater().onPacketCheckError(PacketCheckErrorHdlr);
            DataFormater.getDataFormater().onPacketCorrected(PacketCorrectedHdlr);
            DataFormater.getDataFormater().onPacketRecevied(PacketReceviedHdlr);

            PhyCombine.GetPhyCombine().onCombineError(CombineErrorHdlr);
        }
        public static DotManager GetDotManger()
        {
            if (onlyme == null)
            {
                onlyme = new DotManager();
            }
            return onlyme;
        }

        public void onPacketRecevied(PacketReceviedDelegate hdlr)
        {
            PacketRecevied_Ev = hdlr;
        }

        public void onPacketCorrected(PacketCorrectedDelegate hdlr)
        {
            PacketCorrected_Ev = hdlr;
        }

        public void Start()
        {
            //do nothing。 仅仅给用户一个感觉。
        }

        //获取一个dot，结果使用事件通知
        public void GetDot(byte[] raw)
        {
            PhyCombine.GetPhyCombine().CombineFragment(raw);
        }

        public Collection<ADot> GetDots()
        {
            return DataFormater.getDataFormater().GetDots();
        }

        //清空Dots，rawText所有数据
        public void ReleaseData()
        {
            DataFormater.getDataFormater().GetDots().Clear();
            PhyCombine.GetPhyCombine().Clear();
        }

        public void DumpRawText(string filename)
        {
            PhyCombine.GetPhyCombine().Dump(filename);
        }

        #endregion
        //
        private void CombineErrorHdlr(CombineErrorInfo err)
        {
            switch (err)
            {
                case CombineErrorInfo.CORRECT_PCT_DATA_FORMAT_ERROR:
                    Console.WriteLine("纠正包格式出错");
                    break;
                case CombineErrorInfo.INVALID_MACHINE_TYPE:
                    Console.WriteLine("无法识别机器格式");
                    break;
                case CombineErrorInfo.INVALID_PACKET_TYPE:
                    Console.WriteLine("无法识别包格式");
                    break;
                case CombineErrorInfo.NOT_FOUND_MACHINE_HEADER_LONG:
                    Console.WriteLine("长时间无法找到机器格式包");
                    //SerialDriver.GetDriver().Close();  //机器格式写死，不需要协商
                    break;
                case CombineErrorInfo.NOT_FOUND_START_TAG_LONG:
                    Console.WriteLine("长时间无法找到包起始标志");
                    break;
                case CombineErrorInfo.SEQUENCE_PCT_DATA_FORMAT_ERROR:
                    Console.WriteLine("序号包格式出错");
                    break;
                case CombineErrorInfo.VALUE_PCT_DATA_FORMAT_ERROR:
                    Console.WriteLine("数据包格式出错");
                    break;
                case CombineErrorInfo.RES_COMPUTE_VALUE_PCT_DATA_FORMAT_ERROR:
                    Console.WriteLine("回应计算包格式出错");
                    break;
                case CombineErrorInfo.AIR_FLUENT_PCT_DATA_FORMAT_ERROR:
                    Console.WriteLine("气体流量包格式出错");
                    break;
                case CombineErrorInfo.AIR_SAMPLE_TIME_PCT_DATA_FORMAT_ERROR:
                    Console.WriteLine("气体采样时间包格式出错");
                    break;
                case CombineErrorInfo.GETSTATUS_RESPONSE_FORMAT_ERROR:
                    Console.WriteLine("获取状态命令回应包格式出错");

                    break;
                case CombineErrorInfo.NORCMD_RESPONSE_FORMAT_ERROR:
                    Console.WriteLine("普通命令回应包格式出错");
                    break;
                case CombineErrorInfo.UNKNOWN:
                default:
                    Console.WriteLine("未知错误");
                    break;
            }
            
        }

        //收到包，
        private void PacketReceviedHdlr(object dot, int sequence,PacketType ptype)
        {
            PacketRecevied_Ev(dot, sequence,ptype);
            ;
        }

        //数据校验事件出错处理函数
        private void PacketCheckErrorHdlr(int sequence,PacketType ptype)
        {
            //目前忽略气体包的校验错，仅对数据包进行纠正
            switch (ptype)
            {
                case PacketType.AIR_FLUENT:
                    Console.WriteLine("气体流量包校验出错");
                    return;
                case PacketType.AIR_SAMPLE_TIME:
                    Console.WriteLine("气体采样时间包校验出错");
                    return;
                case PacketType.GETSTATUS_RESPONSE:
                    Console.WriteLine("状态获取命令回应包校验出错");
                    return;
                case PacketType.NORCMD_RESPONSE:
                    Console.WriteLine("普通命令回应包校验出错");
                    return;
            }

            //
            CorrectItem item = null;

            Console.WriteLine("数据包数据校验出错");
            foreach(CorrectItem im in correctitems)
            {
                if(im.seq == sequence)
                {
                    item = im;
                    break;
                }
            }
            switch (item)
            {
                case null:  //没有记录
                    item = new CorrectItem
                    {
                        seq = sequence,
                        retrycount = 0,
                        status = correctstatus.CORRECTING,
                    };
                    correctitems.Add(item);

                    item.mytimer = new DispatcherTimer();
                    //correcttimer.Interval = new TimeSpan(0, 0, 0, 0, correctstrategy_timespan);  //
                    item.mytimer.Interval = new TimeSpan(0, 0, 0, 0, 1);  //
                    item.mytimer.Tick += new EventHandler(correct_tick_hdlr);



                    item.mytimer.Tag = (object)(correctitems.Count - 1);

                    item.mytimer.Start();
                    break;
                default:  //正在纠错中
                    //do noting
                    Console.WriteLine("正在纠错中: wrong sequence no=" + item.seq+" ;correct time="+item.retrycount + 
                        " ;status="+item.status);
                    break;
            }
            ;
        }

        //纠正成功事件处理函数
        private void PacketCorrectedHdlr(ADot dot, int sequence)
        {
            foreach(CorrectItem item in correctitems)
            {
                if(item.seq == sequence)
                {
                    item.status = correctstatus.CORRECTED;
                    if(item.mytimer.IsEnabled == true)
                    {
                        item.mytimer.Stop();
                        Console.WriteLine("包"+ sequence.ToString() + ": 纠正成功");
                    }
                }
            }
            PacketCorrected_Ev(dot, sequence);
        }

        private void correct_tick_hdlr(object sender, EventArgs e)
        {
            Console.WriteLine("ticker start");
            int index = (int)(((DispatcherTimer)sender).Tag);
            if(correctitems[index].retrycount > correctstrategy_retry)  //超出重发次数，停止定时器
            {
                if (((DispatcherTimer)(sender)).IsEnabled)
                {
                    ((DispatcherTimer)(sender)).Stop();
                }
                correctitems[index].status = correctstatus.FAIL;
            }

            SerialDriver.GetDriver().Send(makeCommandPct(index));
            correctitems[index].retrycount++;
        }

        //数据校验再次出错
        private void PacketStillErrorHdlr(int sequence)
        {
            ;
        }

        //构建一个命令包
        //@param
        //  sequence 序号
        private byte[] makeCommandPct(int sequence)
        {
            byte[] data = new byte[12];
            data[0] = 0x43;
            data[1] = 0x01;                 //设备地址
            data[2] = 0xa0;                 //命令

            string s = sequence.ToString();
            data[3] = (byte)s[0];
            data[4] = (byte)s[1];
            data[5] = (byte)s[2];
            data[6] = (byte)s[3];
            data[7] = (byte)s[4];
            data[8] = (byte)s[5];

            //填充校验位
            int total = data[3] + data[4] + data[5] + data[6] + data[7] + data[8] - 0x30 * 6;
            string c = total.ToString();
            data[9] = (byte)c[0];
            data[10] = (byte)c[1];
            data[11] = 0x44;            //"D"
            return data;
        }
    }
}
