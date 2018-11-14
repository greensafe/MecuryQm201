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

namespace SilverTest
{
    /// <summary>
    /// ModifyDataWnd.xaml 的交互逻辑
    /// </summary>
    public partial class ModifyDataWnd : Window
    {
        private Window parentwindow;
        public ModifyDataWnd()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            parentwindow = this.Owner;

            TabControl tabctl = parentwindow.FindName("sampletab") as TabControl;
            DataGrid dg;

            switch (tabctl.SelectedIndex)
            {
                case 0:  //新样测试
                    dg = parentwindow.FindName("NewTargetDgd") as DataGrid;
                    NewTestTarget newitem = dg.Items[dg.SelectedIndex] as NewTestTarget;
                    responsevaluetxt.Text = newitem.ResponseValue1;
                    sampletimetxt.Text = newitem.AirSampleTime;
                    fluenttxt.Text = newitem.AirFluent;
                    break;
                case 1:  //标样测试
                    dg = parentwindow.FindName("standardSampleDgd") as DataGrid;
                    StandardSample standarditem = dg.Items[dg.SelectedIndex] as StandardSample;
                    responsevaluetxt.Text = standarditem.ResponseValue1;
                    sampletimetxt.IsEnabled = false;
                    sampletimelable.IsEnabled = false;
                    fluentlabel.IsEnabled = false;
                    break; 
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void checkbtn_Click(object sender, RoutedEventArgs e)
        {
            Window w = this.Owner;
            Button man = w.FindName("checkerbtn") as Button;
            DataGrid dg;
            TabControl tab = parentwindow.FindName("sampletab") as TabControl;
            string text = "";

            switch (tab.SelectedIndex)
            {
                case 1:     //标样
                    dg = parentwindow.FindName("standardSampleDgd") as DataGrid;
                    break;
                case 0:  //新样
                default:
                    dg = parentwindow.FindName("NewTargetDgd") as DataGrid;
                    break;
            }

            if (man.Visibility == Visibility.Visible)
            {
                //审核已经登陆
                switch (tab.SelectedIndex)
                {
                    case 0:
                        text = "样本修改 " + DateTime.Now.ToString()+"\r\n";
                        text += (dg.Items[dg.SelectedIndex] as NewTestTarget).ResponseValue1 + " --> " + responsevaluetxt.Text + "\r\n";
                        text += (dg.Items[dg.SelectedIndex] as NewTestTarget).AirSampleTime + " --> " + sampletimetxt.Text + "\r\n";
                        text += (dg.Items[dg.SelectedIndex] as NewTestTarget).AirFluent + " --> " + fluenttxt.Text + "\r\n";
                        (dg.Items[dg.SelectedIndex] as NewTestTarget).ResponseValue1 = responsevaluetxt.Text;
                        (dg.Items[dg.SelectedIndex] as NewTestTarget).AirSampleTime = sampletimetxt.Text;
                        (dg.Items[dg.SelectedIndex] as NewTestTarget).AirFluent = fluenttxt.Text;
                        
                        break;
                    case 1:
                        text = "标样修改 " + DateTime.Now.ToString() + "\r\n";
                        text += (dg.Items[dg.SelectedIndex] as StandardSample).ResponseValue1 + " --> " + responsevaluetxt.Text + "\r\n";
                        (dg.Items[dg.SelectedIndex] as StandardSample).ResponseValue1 = responsevaluetxt.Text ;
                        break;
                }
                //save to change log 
                string md5 = EasyEncryption.MD5.ComputeMD5Hash(text);
                string filename = "changelog\\"+ md5 + ".change";

                FileStream aFile = new FileStream(filename, FileMode.Create);
                StreamWriter sr = new StreamWriter(aFile);
                sr.Write(text);
                sr.Close();
                aFile.Close();
                this.Close();
            }
            else
            {
                //审核者未登陆
                Window login = new CheckerLoginWnd();
                login.Owner = this.Owner;
                //Application.Current.Properties["isshort"] = true;
                login.ShowDialog();
            }
        }
    }
}
