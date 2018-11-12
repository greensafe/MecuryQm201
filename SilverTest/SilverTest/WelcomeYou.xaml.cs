using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SilverTest
{
    /// <summary>
    /// WelcomeYou.xaml 的交互逻辑
    /// </summary>
    public partial class WelcomeYou : Window
    {
        bool isactive = false;
        public WelcomeYou()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow m = new MainWindow();
            m.Show();
            this.Close();
        }

        private void welecomeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0,0,2);
            timer.Tick += new EventHandler(ticker_handler);
            timer.Start();
        }
        void ticker_handler(object sender, EventArgs e)
        {
            if (!isactive)
            {
                isactive = true;
                DispatcherTimer t = sender as DispatcherTimer;
                t.Stop();
                MainWindow m = new MainWindow();
                m.Show();
                this.Close();

            }
        }

        private void PackIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}
