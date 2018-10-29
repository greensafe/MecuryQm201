using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace SilverTest
{
    /// <summary>
    /// WaveHistory.xaml 的交互逻辑
    /// </summary>
    public partial class WaveHistoryWnd : Window
    {
        private Collection<HistoryItem> listdata = new Collection<HistoryItem>();
        MainWindow parentwindow = null;

        public WaveHistoryWnd()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            parentwindow = this.Owner as MainWindow;

            Collection<string> item = new Collection<string>();
            item.Add("2018.9.10 星期一 13.00");
            item.Add("2018.9.10 星期一 12.00");
            //itemlist.ItemsSource = item;

            //加载列表数据
            loadListData();
        }

        private void loadListData()
        {
            //get the globalid
            string gid = "";
            
            switch (parentwindow.sampletab.SelectedIndex)
            {
                case 0: //new sample
                    gid = (parentwindow.NewTargetDgd.Items[parentwindow.NewTargetDgd.SelectedIndex] as NewTestTarget).GlobalID;
                    break;
                case 1: //standard sample
                    gid = (parentwindow.standardSampleDgd.Items[parentwindow.standardSampleDgd.SelectedIndex] as StandardSample).GlobalID;
                    break;
            }
            if(gid == null)
            {
                MessageBox.Show("没有数据");
                return;
            }

            //构建list
            List<HistoryItem> datas = new List<HistoryItem>();
            DirectoryInfo folder = new DirectoryInfo(@"history");
            foreach (FileInfo file in folder.GetFiles("*.bin"))
            {
                if(file.Name.IndexOf(gid)>=0)
                    datas.Add(new HistoryItem(file.Name,file.FullName));
            }
            filelsb.ItemsSource = datas;
        }

        private void refreshUI()
        {
    
        }

        private void itemlistbtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("hello");
        }

        private void filelsb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            historywaveuc.ClearData();
            //在波形控件中载入波形
            string filepath = (e.AddedItems[0] as HistoryItem).Fullpath;
            FileStream aFile = new FileStream(filepath, FileMode.Open);
            StreamReader sr = new StreamReader(aFile);
            int xscale = 0;
            int yscale = 0;
            while (!sr.EndOfStream)
            {
                yscale = int.Parse( sr.ReadLine() );
                historywaveuc.AddPoint(new Point(xscale,yscale));
                xscale++;
            }

            sr.Close();
            aFile.Close();
        }

        private void exitbtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class HistoryItem
    {
        public string Filename { get; set; }
        public string Fullpath { get; set; }
        public HistoryItem(string file, string full)
        {
            Filename = file;
            Fullpath = full;
        }
    }
}
