using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using static SilverTest.libs.DataFormater;

namespace SilverTest.libs
{
    /*
     * 串口数据分析对外交互类。管理DataFormater, PhyCombine。 
     * 制定错误处理策略
     * singleton对象
     */
    public class DotManager
    {
        private static DotManager onlyme = null;

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
        }
        public static DotManager GetDotManger()
        {
            if (onlyme == null)
            {
                onlyme = new DotManager();
            }
            return onlyme;
        }

        public void Start()
        {
            //do nothing。 仅仅给用户一个感觉。
        }

        public Collection<ADot> GetDots()
        {
            return DataFormater.getDataFormater().GetDots();
        }

        //收到纠正包，刷新波形图
        /*
        private void PacketCorrectedHdlr( int sequence)
        {
            Console.WriteLine("C dots["+sequence.ToString()+"]: "+dot.ToString());
            ;
        }*/
        //收到包，
        private void PacketReceviedHdlr(ADot dot, int sequence)
        {
            Console.WriteLine("dots[" + sequence.ToString() + "]: " + dot.ToString());
            ;
        }

        //数据校验事件出错处理函数
        private void PacketCheckErrorHdlr(int sequence)
        {
            //
            CorrectItem item = null;
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
                    DispatcherTimer correcttimer = new DispatcherTimer();
                    correcttimer.Interval = new TimeSpan(0, 0, 0, 0, correctstrategy_timespan);  //1 seconds
                    correcttimer.Tick += new EventHandler(correct_tick_hdlr);
                    
                    item = new CorrectItem
                    {
                        seq = sequence,
                        retrycount = 0,
                        status = correctstatus.CORRECTING,
                        mytimer = correcttimer,
                    };
                    correctitems.Add(item);

                    correcttimer.Tag = (object)(correctitems.Count - 1);
                    correcttimer.Start();
                    break;
                default:  //正在纠错中
                    //do noting
                    break;
            }
            ;
        }

        //纠正成功事件处理函数, 刷新波形图
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
        }

        private void correct_tick_hdlr(object sender, EventArgs e)
        {
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
