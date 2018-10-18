using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace test
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Random random;
        int ticker = 0;
        FileStream readFile;
        StreamReader readstream;

        public MainWindow()
        {
            random = new Random();
            InitializeComponent();

            readFile = new FileStream("realdata.bin", FileMode.Open);
            readstream = new StreamReader(readFile);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //wc.NumberOfDValue = 1187;
            wc.SetScale(200, 4000, 300, 30000);
            //wc.RatioS = "1:10";
            //wc.Height = 100;
            //wc.Width = 400;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            /*
            wc.AddPoint(new Point(0, 0));
            wc.AddPoint(new Point(100, 100));
            wc.AddPoint(new Point(200, 50));
            wc.AddPoint(new Point(300, 150));
            wc.AddPoint(new Point(400, 200));
            wc.AddPoint(new Point(500, 200));
            
            wc.AddPoint(new Point(0, 104));
            wc.AddPoint(new Point(21, 100));
            wc.AddPoint(new Point(22,145));
            wc.AddPoint(new Point(23, 259));
            */
            //wc.AddPoint(new Point(24, 190));
            //wc.AddPoint(new Point(25, 158));
            //wc.AddPoint(new Point(26, 129));
            
            //wc.AddPoint(new Point(0, 0));
            wc.NumberOfDValue = 1187;
            //wc.SetScale(200, 4000, 300, 30000);
            DispatcherTimer drawtimer = new DispatcherTimer();
            drawtimer.Tick += new EventHandler(timer_hdlr);
            drawtimer.Interval = new TimeSpan(0, 0, 0,0, 1);
            drawtimer.Start();
            //wc.Height = 400;
            //wc.Width = 400;
        }

        private void drawxaxis_Click(object sender, RoutedEventArgs e)
        {
            wc.SetScale(200, 4000, 300, 9000);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void sendtestdata()
        {
            string data;
            if (readstream.EndOfStream == false)
            {
                data = readstream.ReadLine();
            }
            else
            {
                PointCollection dvalus = wc.GetDValues();
                PointCollection datas = wc.GetDatas();
                return;
            }
            Point p = new Point(
                ticker,
                int.Parse(data)
                );
            wc.AddPoint(p);
            Console.WriteLine(p.ToString());
        }

        private void timer_hdlr(object sender, EventArgs e)
        {
            DispatcherTimer selft = sender as DispatcherTimer;
            sendtestdata();
            ticker++;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            dynamic p = new Point();
            p.X = 100;
            p.Y = 200;
            
            
        }
    }
}
