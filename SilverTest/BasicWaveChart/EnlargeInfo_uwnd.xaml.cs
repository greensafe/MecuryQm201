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

namespace BasicWaveChart
{
    /// <summary>
    /// EnlargeInfo_uwnd.xaml 的交互逻辑
    /// </summary>
    public partial class EnlargeInfo_uwnd : Window
    {
        public int minvalue{get;set;}
        public int maxvalue { get; set; }
        public BasicWaveChartUC myproxy { get; set; }

        public EnlargeInfo_uwnd()
        {
            InitializeComponent();
            
        }

        private void Ok_btn_Click(object sender, RoutedEventArgs e)
        {
            int temp;
            if (int.TryParse(minvalue_txt.Text, out temp))
            {
                minvalue = temp;
            }
            else
            {
                minvalue = 0;
            }

            if(int.TryParse(maxvalue_txt.Text,out temp))
            {
                maxvalue = temp;
            }
            else
            {
                maxvalue = 0;
            }
            //this.DialogResult = true;
            myproxy.EnlargeWave(minvalue, maxvalue);
        }

        private void No_btn_Click(object sender, RoutedEventArgs e)
        {
            myproxy.HideEnlargeHoritalLine();
            this.DialogResult = false;
        }

        private void Ok_btn_Loaded(object sender, RoutedEventArgs e)
        {
            this.maxvalue_txt.Text = maxvalue.ToString();
            this.minvalue_txt.Text = minvalue.ToString();
            
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Maxvalue_txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            int temp;
            
            if (int.TryParse((sender as TextBox).Text, out temp))
            {
                maxvalue = temp;
                myproxy.DrawEnlargeHoritalLineUp(maxvalue);
            }
            else
            {
                maxvalue = 0;
            }
        }

        private void Minvalue_txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            int temp;

            if (int.TryParse((sender as TextBox).Text, out temp))
            {
                minvalue = temp;
                myproxy.DrawEnlargeHoritalLineDown(minvalue);
            }
            else
            {
                minvalue = 0;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            myproxy.ShowEnlargeHoritalLine();
        }
    }
}
