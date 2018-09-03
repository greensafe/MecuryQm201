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

namespace SilverTest
{
    /// <summary>
    /// CheckerLoginWnd.xaml 的交互逻辑
    /// </summary>
    public partial class CheckerLoginWnd : Window
    {
        public bool isshort = false;
        public CheckerLoginWnd()
        {
            
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(checkerbtxt.Text == "checker" && passwordtxt.Text == "checker")
            {
                Window w = this.Owner;
                if (isshort)
                {
                    //do noting
                }
                else
                {
                    (w.FindName("checkerbtn") as Button).Visibility = Visibility.Visible;
                }

                this.Close();
            }
        }

        private void exitbtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
