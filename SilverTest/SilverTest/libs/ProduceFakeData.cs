using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SilverTest.libs
{
    /*
     * <>开放工具 模拟机器数据
     *  从文件中读取数据发送到端口
     *  用法
     *       SerialDriver.GetDriver().OnReceived(Com_DataReceived);
     *       ProduceFakeData pfd = new ProduceFakeData("realtestdata.txt");
      *      pfd.Send(1); 
      *      注意，如果realtestdata.txt模拟数据都在一行之中，解析将陷入巨循环之中，
      *      界面停止反应。
     */
    public class ProduceFakeData
    {
        //context
        private DispatcherTimer readDataTimer;
        FileStream aFile;
        StreamReader sr;
        public int i = 100;
        int tickcount = 1;

        public ProduceFakeData(string filename)
        {
            readDataTimer = new DispatcherTimer();
            readDataTimer.Tick += new EventHandler(timeCycle);
            aFile = new FileStream(filename, FileMode.Open);
            sr = new StreamReader(aFile);
            SerialDriver.GetDriver().portname = "COM1";
            SerialDriver.GetDriver().rate = 9600;
            SerialDriver.GetDriver().parity = 0;
            SerialDriver.GetDriver().databits = 8;
            SerialDriver.GetDriver().stopbits = 1;
            //SerialDriver.GetDriver().Open("COM1", 9600, 0, 8, 1);
            SerialDriver.GetDriver().Open(SerialDriver.GetDriver().portname,
                SerialDriver.GetDriver().rate,
                SerialDriver.GetDriver().parity,
                SerialDriver.GetDriver().databits,
                SerialDriver.GetDriver().stopbits);
            if (!SerialDriver.GetDriver().isOpen())
            {
                Console.WriteLine("======打开COM1失败======");
            }
        }

        //向端口间隔发送数据
        public void Send(int s)
        {

            //readDataTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);  //1 seconds
            readDataTimer.Interval = new TimeSpan(0, 0, 0, 0,1);  //1 seconds
            readDataTimer.Start();
        }

        private void timeCycle(object sender, EventArgs e)
        {
            tickcount++;
            string line = "";
            string ch;
            //Console.WriteLine("tick " + tickcount.ToString());
            try
            {
                //line = sr.ReadLine();
                if (sr.EndOfStream == false)
                {
                    ch = sr.ReadLine();
                    line += ch;
                }
                else
                {
                    //clear context
                    aFile.Close();
                    aFile = null;
                    sr.Close();
                    sr = null;
                    readDataTimer.Stop();
                    readDataTimer.IsEnabled = false;
                    readDataTimer = null;
                    Console.WriteLine("end of file. env released !");
                }
      
                // Read data in line by line.
                if (line != null)
                {
                    //Console.WriteLine("write to serial:" + line);
                    //line = line + "\r\n";
                    //byte[] b = { 1, 2, 3, 4, 5 };
                    if (SerialDriver.GetDriver().isOpen())
                    {
                        SerialDriver.GetDriver().Send(Encoding.Default.GetBytes(line));
                    }
                    ;
                    //line = sr.ReadLine();
                }
                else
                {
                    //clear context
                    aFile.Close();
                    aFile = null;
                    sr.Close();
                    sr = null;
                    readDataTimer.Stop();
                    readDataTimer.IsEnabled = false;
                    readDataTimer = null;
                    Console.WriteLine("env released !");
                    
                }
            }
            catch (Exception error)
            {
                //clear context
                aFile.Close();
                aFile = null;
                sr.Close();
                sr = null;
                readDataTimer.Stop();
                readDataTimer.IsEnabled = false;
                readDataTimer = null;
                Console.WriteLine("error occure, env is released !");
            }
            ;
        }

    }

}
