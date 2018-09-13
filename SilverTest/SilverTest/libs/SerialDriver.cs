using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SilverTest.libs
{
    /*
     * 串口驱动类，
     * 仅仅能生成一个对象
     * 
     * 使用顺序
     * GetDriver -> open->send->close->destroy
     *   GetDriver->open ->send->close可反复使用，均是使用驱动对象
     *   调用destroy后，当前的驱动对象被销毁
     *    
     */
    public class SerialDriver
    {
        static private SerialPort ComDevice = null;
        static private SerialDriver onlyone = null;

        public string portname = "COM5";
        public int rate = 38400;
        public int parity = 0 ;
        public int databits = 8;
        public int stopbits = 1;

        private int  hdrcount = 0;

        private SerialDriver()
        {
            ComDevice = new SerialPort();
            Console.WriteLine("SerialDriver object is created");
        }

        //获取SerialDriver对象
        static public SerialDriver GetDriver()
        {
            if (onlyone == null)
            {
                onlyone = new SerialDriver();
            }
            return onlyone;
        }

        //读取端口收到的数据
        public byte[] Read()
        {

            if (ComDevice.IsOpen == (true))
            {
                byte[] ReDatas = new byte[ComDevice.BytesToRead];
                ComDevice.Read(ReDatas, 0, ReDatas.Length);
                return ReDatas;
            }
            else
            {
                Console.WriteLine("SerialDriver:端口关闭，无法读取数据");
                return null;
            }
            
            /*
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ReDatas.Length; i++)
            {
                sb.AppendFormat("{0:x2}" + " ", ReDatas[i]);
            }
            */
        }

        //设置数据接收处理函数
        //注意：仅仅调用一次，避免注册多次处理函数
        public SerialDriver OnReceived(SerialDataReceivedEventHandler dlr)
        {
            if (hdrcount == 0)
            {
                ComDevice.DataReceived += new SerialDataReceivedEventHandler(dlr);
                hdrcount++;
            }
            return onlyone;
        }

        //打开端口
        public SerialDriver Open(string portname, int rate, int parity, int databits, int stopBits)
        {
            if (ComDevice.IsOpen == false)
            {
                ComDevice.PortName = portname;
                ComDevice.BaudRate = rate;
                ComDevice.Parity = (Parity)parity;
                ComDevice.DataBits = databits;
                ComDevice.StopBits = (StopBits)stopBits;
                try
                {
                    ComDevice.Open();
                    Console.WriteLine("打开成功");
                }
                catch (Exception ex)
                {
                    //Console.WriteLine("打开发生错误");
                    MessageBox.Show("打开发生错误");
                    return onlyone;
                }

            }
            else
            {
                try
                {
                    ComDevice.Close();
                    MessageBox.Show("打开端口已经被关闭");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "无法关闭已打开的端口");
                    return onlyone;
                };
            }

            return onlyone;
        }

        //判断端口是否打开
        public bool isOpen()
        {
            if (ComDevice == null || ComDevice.IsOpen == false)
                return false;
            else
                return true;
        }

        //关闭端口
        public SerialDriver Close()
        {
            if (ComDevice.IsOpen)
            {
                try
                {

                    //ComDevice.DiscardOutBuffer();
                    //ComDevice.DiscardInBuffer();
                    ComDevice.Close();
                    Console.WriteLine("serial is closed!");
                }
                catch
                {
                    MessageBox.Show("关闭端口失败");
                }
            }
            else
            {
                //MessageBox.Show("没有打开的端口");
                Console.WriteLine("SerialDriver.close : 没有打开的端口");
            }
            return onlyone;
        }

        //向端口发送数据
        public bool Send(byte[] data)
        {
            if (ComDevice.IsOpen)
            {
                try
                {
                    ComDevice.Write(data, 0, data.Length);//发送数据  
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("发送数据错误");
                }
            }
            else
            {
                Console.WriteLine("串口未打开");
            }
            return false;
        }

        //销毁端口
        public void destroy()
        {
            if (ComDevice != null && ComDevice.IsOpen)
            {
                ComDevice.Close();

            }
            ComDevice = null;
            onlyone = null;
        }

        //获取端口列表
        public string[] GetPortList()
        {
            return SerialPort.GetPortNames();
        }
    }

}
