using SilverTest.libs;
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
    /// SetPortWnd.xaml 的交互逻辑
    /// </summary>
    public partial class SetPortWnd : Window
    {
        public SetPortWnd()
        {
            InitializeComponent();
        }

        private void exitbtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            string[] ports = SerialDriver.GetDriver().GetPortList();
            foreach(string item in ports)
            {
                comportCombo.Items.Add(item);
            }
        }

        private void applybtn_Click(object sender, RoutedEventArgs e)
        {
            if (comportCombo.SelectedValue == null)
                SerialDriver.GetDriver().portname = "COM1";
            else
                SerialDriver.GetDriver().portname = comportCombo.SelectedValue as string;
            SerialDriver.GetDriver().databits = int.Parse(datacombo.SelectedValue as string);
            SerialDriver.GetDriver().parity = 0;        //paritycombo.SelectedValue as string;
            SerialDriver.GetDriver().rate = int.Parse(speedcombo.SelectedValue as string);
            SerialDriver.GetDriver().stopbits = int.Parse(stopcombo.SelectedValue as string);
            this.Close();
        }
    }
}
