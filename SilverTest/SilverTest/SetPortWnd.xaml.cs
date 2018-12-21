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

        private void dataExitbtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            string[] ports = SerialDriver.GetDriver().GetPortList();
            foreach(string item in ports)
            {
                dataComportCombo.Items.Add(item);
            }
        }

        private void dataApplybtn_Click(object sender, RoutedEventArgs e)
        {
            SerialDriver.GetDriver().portname = dataComportCombo.SelectedValue as string;
            SerialDriver.GetDriver().databits = int.Parse(dataDatacombo.SelectedValue as string);
            SerialDriver.GetDriver().parity = 0;        //paritycombo.SelectedValue as string;
            SerialDriver.GetDriver().rate = int.Parse(dataSpeedcombo.SelectedValue as string);
            SerialDriver.GetDriver().stopbits = int.Parse(dataStopcombo.SelectedValue as string);
            this.Close();
        }

        private void OnAlarmSelected(object sender, RoutedEventArgs e)
        {

        }

        private void OnTestDataSelected(object sender, RoutedEventArgs e)
        {

        }

        private void AlarmApplybtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AlarmExitbtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
