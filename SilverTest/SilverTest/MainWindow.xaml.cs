﻿using MahApps.Metro.IconPacks;
using Microsoft.Win32;
using SilverTest.libs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;
using static SilverTest.libs.DataFormater;
using static SilverTest.libs.PhyCombine;

/*
 * 变量尾缀约定
 * Clt = Collection
 * XDP = XmlDataProvider
 * Dgd = DataGrid 
 * Bbx = GroupBox
 * Cmb = ComboBox
 * Txb = TextBox
 * Viw = ICollectionView
 * 
 * 函数尾缀约定
 * Hdlr = handler 事件处理函数
 * 
 * 函数前缀约定
 * On = 事件注册函数
*/

namespace SilverTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>

    //模拟数据
    public struct RowItem
    {
        public string no { get; set; }
        public string sampleName { get; set; }
        public string responseTime { get; set; }
    }

    //波形图绘制现场信息
    public class WaveDrawSite
    {
        //绘制粒度。一次绘制2个点
        public static int grain = 2;
        //dots中待绘制点的位置
        public static int to_pos_index_rel = 0; 
    }

    //样本类型
    public enum SampleType
    {
        LIQUID,
        AIR,
        SOLID
    }

    //测试模块
    public enum TestModule
    {
        HOT_AIR_BOX,    //热释气体炉测量
        AIR_ATOM_IN_AHEAD,  //气体测量原子吸收法前进样
        AIR_GOLD_ATOM_IN_BACK    ,//气体金属膜原子吸收法后进样
        AIR_ADJUST_ZERO_ATOM_IN_AHEAD,//自校零原子吸收法前进样
        LIQUID_MULTI_BULK,//液体多量程测量
        LIQUID_STANDARD_BULK//标准液体量程测量
    }

    public partial class MainWindow : Window
    {

        bool mode = true;
        Random rd = new Random();
        private int currentSecond = 0;
        int xaxis = 0;
        int yaxis = 0;
        int group = 200;//组距
        Queue<int> q = new Queue<int>();
        //配置
        //停止测试点位
        readonly int stop_test_position = 7500;
        //瞬时最大值
        double maxResponse = 0;
        //载入气体温度汞浓度资源
        XmlDocument AirDensityXml = new XmlDocument();
        //样本类型
        SampleType sampleType = SampleType.AIR;
        //波形涮新timer
        private DispatcherTimer waveFreshTimer;
        //状态栏时间更新定时器
        private DispatcherTimer statusDayTimer;

        //表格条目样本全局id
        private int newsample_item_globalid = 0;
        private int standardsample_item_globalid = 0;

        private ObservableCollection<NewTestTarget> newTestClt;
        private ObservableCollection<StandardSample> standardSampleClt;

        //正在测试中的条目id号
        string testingitemgid_new = "-1";
        string testingitemgid_std = "-1";
        string testing_gid = "-1";   //真正测试条目的global id
        //点击开始测试后，被选中的表格条目的相对索引号
        NewTestTarget testing_selected_new = null;
        StandardSample testing_selected_standard = null;

        ICollectionView StandardCvw;

        //指向命令面板窗口
        CommandPanelWnd cmdpanelWnd;

        //check icon
        PackIconMaterial checkicon = new PackIconMaterial();

        //测试模块id
        TestModule testmoduleid = TestModule.AIR_GOLD_ATOM_IN_BACK;

        //液体标量程测量标量常量
        const int inquaility = 25; //mL

        public enum TestingTabType
        {
            NEW,
            STANDARD,
            UNKNOWN
        }

        //测试状态
        TestingStatus teststatus = TestingStatus.IDLE;
        TestingTabType testtabtype = TestingTabType.NEW;

        public enum TestingStatus
        {
            TESTING,
            STOPPED,
            IDLE
        }

        public MainWindow()
        {
            InitializeComponent();

            //初始化xml文档
            AirDensityXml.Load(@"resources\ChinaAirDensity.xml");
    
            newTestClt =
                    Utility.getNewTestTargetDataFromXml("resources\\NewTestTarget_Table.xml");
            NewTargetDgd.DataContext = newTestClt;
            standardSampleClt =
                Utility.getStandardTargetDataFromXml("resources\\StandardSamples_Table.xml");
            standardSampleDgd.DataContext = standardSampleClt;
            ;
            StandardCvw = CollectionViewSource.GetDefaultView(standardSampleClt);
            StandardCvw.GroupDescriptions.Add(new PropertyGroupDescription("GroupName"));

            checkicon.Kind = PackIconMaterialKind.Check;
            //Utility.SetValueToXml("/config/QM201H/wavehistory/fileid", "newsample","100");

        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {


            //加载config数据
            //string s = Utility.GetValueFrXml("QM201H/response/compute", "ratio");
            string rs = Utility.GetValueFrXml("/config/QM201H/response/compute/R1", "pointstart");
            string re = Utility.GetValueFrXml("/config/QM201H/response/compute/R1", "end");

            //初始化串口数据分析模块
            SerialDriver.GetDriver().OnReceived(Com_DataReceived);
            DotManager.GetDotManger().onPacketCorrected(CorrectedPacketReceived);
            DotManager.GetDotManger().onPacketRecevied(PacketReceived);
            DotManager.GetDotManger().Start();
            //波形刷新timer
            waveFreshTimer = new DispatcherTimer();
            waveFreshTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);  //1 millseconds
            waveFreshTimer.Tick += new EventHandler(waveFreshTimer_tickHdr);
            waveFreshTimer.Start();
            //状态栏时间timer
            statusDayTimer = new DispatcherTimer();
            statusDayTimer.Interval = new TimeSpan(0, 0, 0, 1);
            statusDayTimer.Tick += new EventHandler(statustimer_tickHdr);
            statusDayTimer.Start();
            //注册波形控件积分值变化事件
            realCpt.OnIntegrateValueChange(AreaIntegrateValueChangedHdlr);

            //创建文件夹
            if (!Directory.Exists("changelog\\"))
            {
                Directory.CreateDirectory("changelog\\");
            }
            if (!Directory.Exists("history\\"))
            {
                Directory.CreateDirectory("history\\");
            }
            if (!Directory.Exists("config\\"))
            {
                Directory.CreateDirectory("config\\");
            }
            if (!Directory.Exists("resources\\"))
            {
                Directory.CreateDirectory("resources\\");
            }

            //自适应窗口，并且居中
            double heightgap = 0;
            if (this.Height > SystemParameters.WorkArea.Size.Height)
            {
                double screenHeight = SystemParameters.PrimaryScreenHeight;
                double screenWidth = SystemParameters.PrimaryScreenWidth;

                heightgap = this.Height - SystemParameters.WorkArea.Size.Height;
                standardSampleDgd.Height -= heightgap;
                NewTargetDgd.Height -= heightgap;
                this.Height -= heightgap;
                this.Top = (SystemParameters.WorkArea.Size.Height - this.Height) / 2;
                this.Left = (SystemParameters.WorkArea.Size.Width - this.Width) / 2;
            }
        }

        private void statustimer_tickHdr(object sender, EventArgs e)
        {
            DateTime d = DateTime.Now;
            string s = d.Year.ToString() +":" + d.Month.ToString() +":"+ d.Day.ToString() + " " + d.Hour.ToString() +":"+ d.Minute.ToString()+":" + d.Second.ToString();
            timestatusbar.Text = s;
        }

        private void waveFreshTimer_tickHdr(object sender, EventArgs e)
        {
            refreshWaveUI();
        }

        private void refreshWaveUI()
        {
            int dotscount = DotManager.GetDotManger().GetDots().Count;
            Collection<ADot> dots = DotManager.GetDotManger().GetDots();
            if (WaveDrawSite.to_pos_index_rel > dotscount - 1)
            {
                return;
            }
            int todrawcount = WaveDrawSite.grain <= (dotscount  - WaveDrawSite.to_pos_index_rel) ? WaveDrawSite.grain : (dotscount - WaveDrawSite.to_pos_index_rel);
            for(int i = WaveDrawSite.to_pos_index_rel; i< WaveDrawSite.to_pos_index_rel + todrawcount; i++)
            {
                //draw dot
                realCpt.AddPoint(new Point(WaveDrawSite.to_pos_index_rel, dots[WaveDrawSite.to_pos_index_rel].Rvalue));
            }
            WaveDrawSite.to_pos_index_rel += todrawcount;
        }


        public void Com_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            byte[] ReDatas = SerialDriver.GetDriver().Read();
            if (ReDatas == null) return;
            string re = "";
            for (int i = 0; i < ReDatas.Length; i++)
            {
                re += (char)ReDatas[i];
            }
            Console.WriteLine("serial received : " + re);
            DotManager.GetDotManger().GetDot(ReDatas);
        }
       

        private void AddRowBtn_Click(object sender, RoutedEventArgs e)
        {
            addrow();
        }

        private void addrow()
        {
            int se = 0;
            int index_out;
            switch (sampletab.SelectedIndex)
            {
                //新样测试
                case 0:
                    NewTestTarget newitem = new NewTestTarget();
                    //计算序号
                    for (int i = 0; i < newTestClt.Count; i++)
                    {
                        //处理边界情况
                        if (i == newTestClt.Count - 1)
                        {
                            se = int.Parse(newTestClt[i].Code) + 1;
                            break;
                        }
                        //序号连续情况下，跳过连续号码
                        if (int.Parse(newTestClt[i].Code) + 1 == int.Parse(newTestClt[i + 1].Code))
                            continue;
                        //找到一个可能的空洞
                        se = int.Parse(newTestClt[i].Code + 1);
                    }
                    //newitem.Code = se.ToString();
                    newitem.GlobalID = GIDMaker.GetMaker().GetNId(out index_out);
                    newitem.NewName = "样品";
                    /*
                    for (int i = 1; i < newitem.GlobalID.Length; i++)
                    {
                        newitem.Code += newitem.GlobalID[i];
                        newitem.NewName += newitem.GlobalID[i];
                    }
                    */
                    newitem.Code = index_out.ToString();
                    newitem.NewName += (index_out - 1).ToString();
                    newTestClt.Add(newitem);
                    break;
                //标样测试
                case 1:
                    StandardSample standarditem = new StandardSample();
                    //计算序号
                    for (int i = 0; i < standardSampleClt.Count; i++)
                    {
                        //处理边界情况
                        if (i == standardSampleClt.Count - 1)
                        {
                            se = int.Parse(standardSampleClt[i].Code) + 1;
                            break;
                        }
                        //序号连续情况下，跳过连续号码
                        if (int.Parse(standardSampleClt[i].Code) + 1 == int.Parse(standardSampleClt[i + 1].Code))
                            continue;
                        //找到一个空洞
                        se = int.Parse(standardSampleClt[i].Code + 1);
                    }
                    //standarditem.Code = se.ToString();
                    standarditem.GlobalID = GIDMaker.GetMaker().GetSId(out index_out);
                    standarditem.SampleName = "标样";
                    standarditem.GroupName = "组1";
                    
                    standarditem.Code = index_out.ToString();
                    standarditem.SampleName += (index_out - 1).ToString();

                    standardSampleClt.Add(standarditem);

                    break;
                default:

                    break;
            }

        }

        private void DelRowBtn_Click(object sender, RoutedEventArgs e)
        {
            int index = 0;
            switch (sampletab.SelectedIndex)
            {
                //新样测试
                case 0:
                    if (NewTargetDgd.SelectedIndex < 0)
                        return;
                    index = getNewCltIndex(NewTargetDgd.SelectedIndex);
                    if (index == -1) return;
                    newTestClt.RemoveAt(index);
                    break;
                //标样测试
                case 1:
                    if (standardSampleDgd.SelectedIndex < 0)
                        return;
                    index = getStandardCltIndex(standardSampleDgd.SelectedIndex);
                    if (index == -1) return;
                    standardSampleClt.RemoveAt(index);
                    break;
                default:
                    break;
            }

            threeline();

        }

        //增加老年需求
        //总是保持三行空白
        private void threeline()
        {
            int blankcount = 0;
            switch (sampletab.SelectedIndex)
            {
                case 0:
                    foreach(NewTestTarget item in newTestClt )
                    {
                        if (item.NewName == "" || item.NewName is null)
                            blankcount++;
                    }

                    break;
                case 1:
                    foreach(StandardSample item in standardSampleClt)
                    {
                        if(item.SampleName == "" || item.SampleName is null)
                        {
                            blankcount++;
                        }
                    }
                    break;
            }
            if (blankcount < 3)
                addrow();
        }

        private void OnStandardTabSelected(object sender, RoutedEventArgs e)
        {
            if (window.IsLoaded)
            {
                paramGbx.Visibility = Visibility.Collapsed;
                RBtn.Visibility = Visibility.Visible;
                Rstackpanel.Visibility = Visibility.Visible;
                printRbtn.Visibility = Visibility.Visible;
            }
        }

        private void OnNewTabSelected(object sender, RoutedEventArgs e)
        {

            if (window.IsLoaded)
            {
                paramGbx.Visibility = Visibility.Visible;
                RBtn.Visibility = Visibility.Hidden;
                Rstackpanel.Visibility = Visibility.Collapsed;
                printRbtn.Visibility = Visibility.Collapsed;
                waveContainer.Visibility = Visibility.Visible;
            }
        }

        private void sampletab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NewTargetDgd.CommitEdit(DataGridEditingUnit.Row, true);
            standardSampleDgd.CommitEdit(DataGridEditingUnit.Row, true);
            ;
        }

        private void newItemHead_Click(object sender, RoutedEventArgs e)
        {

        }

        public int getNewCltIndexFromSelected(NewTestTarget obj)
        {
            if (obj is null)
                return -1;
            string code = obj.Code;


            for (int i = 0; i < newTestClt.Count; i++)
            {
                if (newTestClt[i].Code == code)
                    return i;
            }
            return 0;
        }


        //查询选中条目对应序号。语意有错，入参index无用。为了减少代码修改，保持该函数不变
        public int getNewCltIndex(int index)
        {
            
            if (NewTargetDgd.SelectedItem is null)
                return -1;
            string code = (NewTargetDgd.SelectedItem as NewTestTarget).Code;
            
            
            for (int i = 0; i < newTestClt.Count; i++)
            {
                if (newTestClt[i].Code == code)
                    return i;
            }
            return 0;
        }

        
        private void standardCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //获取选中的标样记录
            if (e.AddedItems.Count == 0) return;
            StandardSample asample = e.AddedItems[0] as StandardSample;
            aTxb.Text = asample.A;
            bTxb.Text = asample.B;
            rTxt.Text = asample.R;
            if (asample.A is null ||
                asample.B is null ||
                asample.R is null
                ) return;
            double a = double.Parse(asample.A);
            double b = double.Parse(asample.B);

            //获取选中的新样记录，计算对应的索引号
            int rowNo = NewTargetDgd.SelectedIndex;
            int cltindex = getNewCltIndex(rowNo);
            if (cltindex == -1) return;
            if (rowNo < 0)
                return;
            if (newTestClt[cltindex].ResponseValue1 == "" ||
                newTestClt[cltindex].ResponseValue1 == null
                )
                return;

            //根据R，计算相关值
            switch (testmoduleid)
            {
                case TestModule.AIR_ADJUST_ZERO_ATOM_IN_AHEAD:
                case TestModule.AIR_ATOM_IN_AHEAD:
                    //(R,响应值)->汞含量
                    newTestClt[cltindex].ThingInSamle = Math.Round( 
                        (double.Parse(newTestClt[cltindex].ResponseValue1) - b) / a, 5).ToString();
                    //样品中汞含量
                    if(newTestClt[cltindex].AirFluent == "" || newTestClt[cltindex].AirFluent is null ||
                        newTestClt[cltindex].AirTotalFluent == "" || newTestClt[cltindex].AirTotalFluent is null)
                    {
                        return;
                    }
                    newTestClt[cltindex].AirG = (Math.Round((
                        (double.Parse(newTestClt[cltindex].AirTotalFluent) / 
                        double.Parse(newTestClt[cltindex].AirFluent)*
                        double.Parse(newTestClt[cltindex].ThingInSamle))/
                        double.Parse(newTestClt[cltindex].AirTotalFluent))/1000,
                        5)).ToString();
                    break;
                case TestModule.AIR_GOLD_ATOM_IN_BACK:
                    //计算气体体积
                    if (newTestClt[cltindex].AirFluent == "" || newTestClt[cltindex].AirFluent is null ||
                        newTestClt[cltindex].AirSampleTime == "" || newTestClt[cltindex].AirSampleTime is null)
                    {
                        return;
                    }
                    newTestClt[cltindex].AirTotolBulk = (Math.Round(double.Parse(newTestClt[cltindex].AirFluent) * double.Parse(newTestClt[cltindex].AirSampleTime),
                        2)).ToString();
                    //y-b/a
                    newTestClt[cltindex].AirG = Math.Round(0.001 * (double.Parse(newTestClt[cltindex].ResponseValue1) - b) / a, 5).ToString();
                    break;
                case TestModule.HOT_AIR_BOX:
                    //(R,响应值)->汞含量
                    newTestClt[cltindex].ThingInSamle = Math.Round(
                        (double.Parse(newTestClt[cltindex].ResponseValue1) - b) / a, 5).ToString();
                    //汞浓度
                    if (newTestClt[cltindex].AirTotolBulk == "" || newTestClt[cltindex].AirTotolBulk is null)
                        return;
                    newTestClt[cltindex].Density = Math.Round( double.Parse(newTestClt[cltindex].ThingInSamle) /
                        double.Parse(newTestClt[cltindex].AirTotolBulk)*
                        1000
                        ,5).ToString();
                    break;
                case TestModule.LIQUID_MULTI_BULK:
                    if (newTestClt[cltindex].AirTotolBulk == "" || newTestClt[cltindex].AirTotolBulk is null ||
                       newTestClt[cltindex].InSampleQuality == "" || newTestClt[cltindex].InSampleQuality is null)
                        return;
                    //(R,响应值)->汞浓度ug / L
                    double d = (double.Parse(newTestClt[cltindex].ResponseValue1) - b) / a;
                    d = d* double.Parse(newTestClt[cltindex].AirTotolBulk)/
                        double.Parse(newTestClt[cltindex].InSampleQuality);
                    newTestClt[cltindex].Density = Math.Round(
                        d, 5).ToString();
                    //样品中汞含量
                    newTestClt[cltindex].AirG = (Math.Round(
                        d / 1000 / 1000 *
                        double.Parse(newTestClt[cltindex].AirTotolBulk)/ 
                        double.Parse(newTestClt[cltindex].AirTotolBulk)*1000,
                        5)).ToString();
                    break;
                case TestModule.LIQUID_STANDARD_BULK:
                default:
                    if (newTestClt[cltindex].AirTotolBulk == "" || newTestClt[cltindex].AirTotolBulk is null )
                        return;
                    //(R,响应值)->汞浓度ug / L
                    double d1 = (double.Parse(newTestClt[cltindex].ResponseValue1) - b) / a;
                    d1 = d1 * double.Parse(newTestClt[cltindex].AirTotolBulk) /
                        inquaility;
                    newTestClt[cltindex].Density = Math.Round(
                        d1, 5).ToString();
                    //样品中汞含量
                    newTestClt[cltindex].AirG = Math.Round(
                        (d1 / 1000 / 1000 *
                        double.Parse(newTestClt[cltindex].AirTotolBulk) /
                        double.Parse(newTestClt[cltindex].AirTotolBulk) * 1000),
                        5).ToString();
                    break;
            }

        }

        private void modifyBtn_Click(object sender, RoutedEventArgs e)
        {

            Utility.SaveToNewXmlFileCls.SaveToNewXmlFile(newTestClt, "resources\\NewTestTarget_Table.xml");
            Utility.SaveToStandardXmlFileCls.SaveToStandardXmlFile(standardSampleClt, "resources\\StandardSamples_Table.xml");
        }

        private void saveall_Click(object sender, RoutedEventArgs e)
        {
            NewTargetDgd.DataContext = null;
            NewTargetDgd.DataContext = newTestClt;
            Utility.SaveToNewXmlFileCls.SaveToNewXmlFile(newTestClt, "resources\\NewTestTarget_Table.xml");
            Utility.SaveToStandardXmlFileCls.SaveToStandardXmlFile(standardSampleClt, "resources\\StandardSamples_Table.xml");
        }

        private void exportexelBtn_Click(object sender, RoutedEventArgs e)
        {
            Utility.Save2excel(NewTargetDgd);
            ;
        }

        private void startTestBtn_Click(object sender, RoutedEventArgs e)
        {
            int newcltindex = 0;
            //realCpt.SetScale(100, 2000, 0, 50);
            //realCpt.NumberOfDValue = 200000;
            switch (sampletab.SelectedIndex)
            {

                case 0:     //新样
                    switch (startTestBtn.Content as string)
                    {
                        case "开始测试":
                            if (NewTargetDgd.SelectedIndex == -1)
                            {
                                MessageBox.Show("请选择一条样本");
                                return;
                            }
                            newcltindex = getNewCltIndex(NewTargetDgd.SelectedIndex);
                            if (newcltindex == -1) return;
                            if (newTestClt[newcltindex].ResponseValue1 != "" && 
                                newTestClt[newcltindex].ResponseValue1 != null)
                            {
                                if (MessageBox.Show("将清除上次的测试结果，是否继续?", "", MessageBoxButton.YesNo) == MessageBoxResult.No) return ;
                                newTestClt[newcltindex].ResponseValue1 = "";
                                newTestClt[newcltindex].AirFluent = "";
                                newTestClt[newcltindex].AirSampleTime = "";
                                newTestClt[newcltindex].AirTotolBulk = "";
                                newTestClt[newcltindex].AirG = "";
                            }

                            teststatus = TestingStatus.TESTING;
                            testing_gid = newTestClt[newcltindex].GlobalID;
                            testtabtype = TestingTabType.NEW;
                            waveinfo_txt.Text = "样本 " + testing_gid.ToString();


                            if (SerialDriver.GetDriver().isOpen() == false)
                            {
                                SerialDriver.GetDriver().Open(
                                    SerialDriver.GetDriver().portname,
                                    SerialDriver.GetDriver().rate,
                                    SerialDriver.GetDriver().parity,
                                    SerialDriver.GetDriver().databits,
                                    SerialDriver.GetDriver().stopbits);
                                //
                            }
                            if (SerialDriver.GetDriver().isOpen() == false) return;

                            //清空图形记录及DotManager中数据
                            DotManager.GetDotManger().ReleaseData();
                            //清理绘波现场
                            realCpt.SetScale(100, 2000, 0, 50);
                            //realCpt.NumberOfDValue = 200000;
                            realCpt.SetNumberOfDValueP(200000);
                            WaveDrawSite.to_pos_index_rel = 0;
                            realCpt.ClearData();

                            startTestBtn.Content = "停止测试";
                            showconnectedIcon();
                            if (SerialDriver.GetDriver().isOpen() == true)
                            {
                                AnimatedColorButton.Visibility = Visibility.Visible;
                                AnimatedColorButton.Visibility = Visibility.Visible;
                            }

                            testing_selected_new = NewTargetDgd.SelectedItem as NewTestTarget;
                            this.testingitemgid_new = newTestClt[getNewCltIndex(NewTargetDgd.SelectedIndex)].GlobalID;
                            this.testingitemgid_std = "-1";

                            NewTargetDgd.DataContext = null;
                            NewTargetDgd.DataContext = newTestClt;

                            standardSampleDgd.DataContext = null;
                            standardSampleDgd.DataContext = standardSampleClt;
                            break;
                        case "停止测试":
                            AnimatedColorButton.Visibility = Visibility.Hidden;
                            AnimatedColorButton.Visibility = Visibility.Hidden;
                            this.testingitemgid_new = "-1";

                            teststatus = TestingStatus.STOPPED;

                            startTestBtn.Content = "开始测试";
                            if (SerialDriver.GetDriver().isOpen() == true)
                            {
                                try
                                {
                                    System.Threading.Thread CloseDown = 
                                        new System.Threading.Thread(new System.Threading.ThreadStart(closeSerialAsc));
                                    CloseDown.Start();
                                }
                                catch (Exception ex)
                                {
                                    ;
                                }
                            }
                            showconnectedIcon();
                            break;
                        default:

                            break;
                    }
                    break;
                case 1:     //标样
                    switch (startTestBtn.Content as string)
                    {
                        case "开始测试":
                            if (standardSampleDgd.SelectedIndex == -1)
                            {
                                MessageBox.Show("请选择一条样本");
                                return;
                            }
                            if (getStandardCltIndex(standardSampleDgd.SelectedIndex) == -1) return;
                            if (standardSampleClt[getStandardCltIndex( standardSampleDgd.SelectedIndex )].ResponseValue1 != "" && 
                                standardSampleClt[getStandardCltIndex( standardSampleDgd.SelectedIndex )].ResponseValue1 != null)
                            {
                                if (MessageBox.Show("将清除上次的测试结果，是否继续?", "", MessageBoxButton.YesNo) == MessageBoxResult.No)
                                    return;
                                standardSampleClt[getStandardCltIndex(standardSampleDgd.SelectedIndex)].ResponseValue1 = "";
                            }

                            teststatus = TestingStatus.TESTING;
                            testing_gid = standardSampleClt[getStandardCltIndex(standardSampleDgd.SelectedIndex)].GlobalID;
                            testtabtype = TestingTabType.STANDARD;
                            waveinfo_txt.Text = "标样 " + testing_gid.ToString();

                            if (SerialDriver.GetDriver().isOpen() == false)
                            {
                                SerialDriver.GetDriver().Open(
                                        SerialDriver.GetDriver().portname,
                                        SerialDriver.GetDriver().rate,
                                        SerialDriver.GetDriver().parity,
                                        SerialDriver.GetDriver().databits,
                                        SerialDriver.GetDriver().stopbits);
                            }
                            if (SerialDriver.GetDriver().isOpen() == false)
                                return;
                            //清空图形记录及DotManager中数据
                            DotManager.GetDotManger().ReleaseData();
                            //清理绘波现场
                            realCpt.SetScale(100, 2000, 0, 50);
                            //realCpt.NumberOfDValue = 200000;
                            realCpt.SetNumberOfDValueP(200000);
                            WaveDrawSite.to_pos_index_rel = 0;
                            realCpt.ClearData();

                            startTestBtn.Content = "停止测试";
                            
                            showconnectedIcon();
                            if (SerialDriver.GetDriver().isOpen() == true)
                            {
                                AnimatedColorButton.Visibility = Visibility.Visible;
                                AnimatedColorButton.Visibility = Visibility.Visible;
                            }

                            testing_selected_standard = standardSampleDgd.SelectedItem as StandardSample;
                            testingitemgid_std = standardSampleClt[getStandardCltIndex(standardSampleDgd.SelectedIndex)].GlobalID;
                            testingitemgid_new = "-1";

                            standardSampleDgd.DataContext = null;
                            standardSampleDgd.DataContext = standardSampleClt;
                            NewTargetDgd.DataContext = null;
                            NewTargetDgd.DataContext = newTestClt;
                            break;
                        case "停止测试":
                            AnimatedColorButton.Visibility = Visibility.Hidden;
                            AnimatedColorButton.Visibility = Visibility.Hidden;
                            startTestBtn.Content = "开始测试";
                            testingitemgid_std = "-1";

                            teststatus = TestingStatus.STOPPED;

                            if (SerialDriver.GetDriver().isOpen() == true)
                            {
                                System.Threading.Thread CloseDown1 =
                                    new System.Threading.Thread(new System.Threading.ThreadStart(closeSerialAsc));
                                CloseDown1.Start();
                            }
                            //计算响应值，填入datagrid之中
                            /*
                            if (standardSampleClt[getStandardCltIndex( standardSampleDgd.SelectedIndex )].ResponseValue1 == "" ||
                                standardSampleClt[getStandardCltIndex( standardSampleDgd.SelectedIndex )].ResponseValue1 == null)
                                standardSampleClt[getStandardCltIndex( standardSampleDgd.SelectedIndex )].ResponseValue1 = Utility.ComputeResponseValue(
                                    dots_start_abs, DotManager.GetDotManger().GetDots().Count).ToString();
                            */
                            showconnectedIcon();
                            break;
                        default:

                            break;
                    }
                    break;
            }
        }

        public void showconnectedIcon()
        {
            if (SerialDriver.GetDriver().isOpen() == true)
            {
                disconnectedcanvas.Visibility = Visibility.Collapsed;
                rs232connectedBorder.Visibility = Visibility.Visible;
                rs232connectedbtn.ToolTip = SerialDriver.GetDriver().portname + "已打开\r\n"+
                    "波特率: "+ SerialDriver.GetDriver().rate + "\r\n"+
                    "校验位: "+ SerialDriver.GetDriver().parity.ToString() +"\r\n" +
                    "停止位: "+ SerialDriver.GetDriver().stopbits.ToString();
                comnameblk.Text = SerialDriver.GetDriver().portname;
                comnameblk.Visibility = Visibility.Visible;
            }
            else
            {
                disconnectedcanvas.Visibility = Visibility.Visible;
                rs232connectedBorder.Visibility = Visibility.Collapsed;
                comnameblk.Visibility = Visibility.Collapsed;
            }
        }


        /*
         * 新线程中关闭serialdriver，避免deadlock
         */
        private void closeSerialAsc()
        {
            SerialDriver.GetDriver().Close();  
            Dispatcher.BeginInvoke(new Action(() =>
            {
                showconnectedIcon();
            }));
        }

        private void exitBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                cmdpanelWnd.Close();
            }
            catch (Exception er) {; }
            this.Close();

        }

        private void debugBtn_Click(object sender, RoutedEventArgs e)
        {

        }


        private void PacketReceived(object dot, int sequence, PacketType ptype)
        {
            int newcltindex = 0;
            int standardcltindex = 0;
            switch(ptype)
            {
                case PacketType.AIR_FLUENT:
                    Console.WriteLine("气体流量包: "+sequence.ToString());
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        newcltindex = getNewCltIndexFromSelected(testing_selected_new);
                        if (newcltindex == -1) return;
                        newTestClt[newcltindex].AirFluent = sequence.ToString();
                    }));
                    break;
                case PacketType.AIR_SAMPLE_TIME:
                    Console.WriteLine("气体采样时间包: " + sequence.ToString());
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        newcltindex = getNewCltIndexFromSelected(testing_selected_new);
                        if (newcltindex == -1) return;
                        newTestClt[newcltindex].AirSampleTime = sequence.ToString();
                    }));
                    break;
                case PacketType.GETSTATUS_RESPONSE:
                    byte[] getstatus_datas = dot as byte[];  //dot存放数据
                    int getstatus_status = sequence;   //sequence存放执行状态
                    Console.WriteLine("状态回应包：状态 - " + getstatus_status.ToString());
                    Console.WriteLine("状态回应包：数据 - " + getstatus_datas.ToString());
                    if (cmdpanelWnd != null)
                    {
                        cmdpanelWnd.ShowGetStatusRes(getstatus_status, getstatus_datas);
                    }
                    break;
                case PacketType.NORCMD_RESPONSE:
                    byte[] r = dot as byte[];
                    Console.WriteLine("普通命令回应包: 主菜单=" + r[0].ToString() + ",次菜单="+r[1].ToString()+",结果="+r[2].ToString());
                    //在面板中显示结果
                    if(cmdpanelWnd != null)
                    {
                        cmdpanelWnd.ShowNormalCmndRes(((r[0] -48) << 4) + r[1] -48,r[2]-48,null);
                    }
                    break;
                case PacketType.RES_COMPUTE_VALUE:
                    Console.WriteLine("计算响应值:" + sequence.ToString());
                    //将结果显示到表格之中
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        switch (sampletab.SelectedIndex)
                        {
                            case 0:         //新样测试
                                newcltindex = getNewCltIndexFromSelected(testing_selected_new);
                                if (newcltindex == -1) return;
                                newTestClt[newcltindex].ResponseValue1 =
                                    sequence.ToString();
                                break;
                            case 1:         //标样测试
                                if (getStandardCltIndexFromSelected(testing_selected_standard) == -1) return;
                                standardSampleClt[getStandardCltIndexFromSelected(testing_selected_standard)].ResponseValue1 =
                                    sequence.ToString();
                                break;
                        }
                    }));
                    break;
                case PacketType.DATA_VALUE:

                    double x = currentSecond;
                    double y = (dot as ADot).Rvalue;

                    if (y > maxResponse)
                        maxResponse = y;

                    Point point = new Point(x, y);
                    
                    currentSecond++;
                    Console.WriteLine("--- dot " + sequence.ToString() + ": " + (dot as ADot).Rvalue + "\r\n");

                    //采样到达一定点数后，自动结束测试，计算并且显示测试结果。
                    if(sequence% stop_test_position >= (stop_test_position-1))
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        { 
                            switch (sampletab.SelectedIndex)
                            {
                                case 0:         //新样测试
                                    //newcltindex = getNewCltIndex(NewTargetDgd.SelectedIndex);
                                    newcltindex = getNewCltIndexFromSelected(testing_selected_new);
                                    if (newcltindex == -1) return;
                                    newTestClt[newcltindex].ResponseValue1 =
                                        maxResponse.ToString();
                                    break;
                                case 1:         //标样测试
                                    if (getStandardCltIndexFromSelected(testing_selected_standard) == -1) return;
                                    standardSampleClt[getStandardCltIndexFromSelected(testing_selected_standard)].ResponseValue1 =
                                        maxResponse.ToString();
                                    break;
                            }
                            maxResponse = 0;
                            //startTestBtn.Content = "开始测试";
                            //AnimatedColorButton.Visibility = Visibility.Hidden;
                        }));
                    }
                    break;
            }

        }

        private void CorrectedPacketReceived(DataFormater.ADot dot, int sequence)
        {
            Console.WriteLine("--- c dot " + sequence.ToString() + ": " + dot.Rvalue);
        }

        private void saveDotsMenu_Checked(object sender, RoutedEventArgs e)
        {
        }

        private void saveDotsMenu_Click(object sender, RoutedEventArgs e)
        {
            //string s = Utility.GetValueFrXml("descendant::R1", "pointstart");

            Collection<ADot> dots =  DotManager.GetDotManger().GetDots();
            ;
        }

        private void saveRawTextMenu_Click(object sender, RoutedEventArgs e)
        {
            DotManager.GetDotManger().DumpRawText("rawtext.txt");
        }

        private void sendTextMenu_Click(object sender, RoutedEventArgs e)
        {
            //SerialDriver.GetDriver().OnReceived(Com_DataReceived);
            //realCpt.NumberOfDValue = 200000;
            ProduceFakeData pfd = new ProduceFakeData("realtestdata_fr2.txt");
            pfd.Send(1);
            showconnectedIcon();
        }

        private void RBtn_Click(object sender, RoutedEventArgs e)
        {
            double[] x;
            double[] y;
            int len = 0;
            int index = 0;
            double a, b, R;
        
            if(standardSampleDgd.SelectedIndex == -1)
            {
                MessageBox.Show("请选择标样组");
                return;
            }
            if (getStandardCltIndex(standardSampleDgd.SelectedIndex) == -1) return;
            string groupname = standardSampleClt[getStandardCltIndex( standardSampleDgd.SelectedIndex )].GroupName;
            foreach(StandardSample item in standardSampleClt)
            {
                if (item.GroupName == groupname)
                {
                    if(item.ResponseValue1 == "" || item.ResponseValue1 is null ||
                       item.AirG =="" || item.AirG is null
                        )
                    {
                        MessageBox.Show("测试未完成，无法计算");
                        return;
                    }
                    else
                    {
                        len++;
                    }
                }
            }
            x = new double[len];
            y = new double[len];
            foreach(StandardSample v in standardSampleClt)
            {
                if (v.GroupName == groupname)
                {
                    x[index] = double.Parse(v.AirG);
                    y[index] = double.Parse(v.ResponseValue1);
                    index++;
                }
                
            }
            Utility.ComputeAB(out a, out b, x, y);
            R = Utility.ComputeR(x, y);
            if(x.Length == 1)
            {
                R = 1;
                a = 1;
            }
            foreach(StandardSample v in standardSampleClt)
            {
                if (v.GroupName == groupname)
                {
                    v.A = Math.Round(a,2).ToString();
                    v.B = Math.Round(b,2).ToString();
                    v.R = Math.Round(R,3).ToString();
                }
            }
        }

        /*
         * 绘制相关系数点直线图
         * 
         */
        private void drawR(double[] x, double[] y, double a, double b, double r, string groupname)
        {
            double maxX = 0, maxY = 0,ratiox = 0,ratioy = 0;
            Line scaleline;
            TextBlock scaletext;
            TextBlock ytext = new TextBlock();
            TextBlock xtext = new TextBlock();
            Line arrowx = new Line();
            Line arrowy = new Line();
            double ScaleGapX;    //x轴单位粒度所占用的点数
            double ScaleGapY;    //y轴单位粒度说占用的点数
            double gapX;        //x轴一个大单位的粒度数
            int gapY;        //y轴一个大单位的粒度数
            double validwidth; //有效区宽度
            double validheight;  //有效区高度

            int topmargin = 20; //顶端的空白，为20个点
            int rightmargin = 200; //右边的空白，为20个点
            int leftmargin = 5;    //左边的空白
            int bottommargin = 20;  //底部的空白

            string samplename = "";

            rCanvas.Children.Clear();

            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] > maxX) maxX = x[i];
                if (y[i] > maxY) maxY = y[i];
            }

            ScaleGapX = (rCanvas.Width - rightmargin - leftmargin) / maxX;
            ScaleGapY = (rCanvas.Height - topmargin - bottommargin) / maxY;
            gapX = (int)(maxX * 10 / x.Length); //maxX太小，比如18.81
            gapX = Math.Round(gapX / 10, 1);
            gapY = (int)(maxY / y.Length);
            gapY = (int)(gapY / 100);
            gapY *= 100;
            validwidth = rCanvas.Width - leftmargin - rightmargin;
            validheight = rCanvas.Height - topmargin - bottommargin;

            //绘制y轴
            Line yline = new Line();
            yline.Stroke = Brushes.Black;
            yline.StrokeThickness = 1; ;
            yline.X1 = leftmargin;
            yline.Y1 = 0;
            yline.X2 = leftmargin;
            yline.Y2 = rCanvas.Height - bottommargin;
            rCanvas.Children.Add(yline);
            ytext.Text = "Y 响应值";
            Canvas.SetTop(ytext, 4);
            Canvas.SetLeft(ytext, leftmargin + 10);
            rCanvas.Children.Add(ytext);
            arrowy.Stroke = Brushes.Black;
            arrowy.StrokeThickness = 1;
            arrowy.X1 = leftmargin;
            arrowy.Y1 = 0;
            arrowy.X2 = leftmargin + 5;
            arrowy.Y2 = 5;
            rCanvas.Children.Add(arrowy);
            //绘制y轴scale尺度
            for (int i = 0; i < y.Length; i++)
            {
                scaleline = new Line();
                scaleline.Stroke = Brushes.Black;
                scaleline.StrokeThickness = 1;
                scaleline.X1 = leftmargin;
                scaleline.Y1 = validheight - gapY * (i + 1)*ScaleGapY + topmargin;
                scaleline.X2 = leftmargin + 5;
                scaleline.Y2 = validheight - gapY * (i + 1) * ScaleGapY + topmargin;
                rCanvas.Children.Add(scaleline);
                scaletext = new TextBlock();
                //scaletext.Text = Math.Round((maxY/(y.Length))*(i+1),2).ToString();
                scaletext.Text = (gapY*(i+1)).ToString();
                Canvas.SetLeft(scaletext,leftmargin + 2);
                Canvas.SetTop(scaletext, (validheight - gapY*(i+1)*ScaleGapY) + topmargin);
                rCanvas.Children.Add(scaletext);
            }

            //绘制x轴
            Line xline = new Line();
            xline.Stroke = Brushes.Black;
            xline.StrokeThickness = 1; ;
            xline.X1 = leftmargin;
            xline.Y1 = rCanvas.Height - bottommargin;
            xline.X2 = rCanvas.Width - rightmargin + 20;
            xline.Y2 = rCanvas.Height - bottommargin;
            rCanvas.Children.Add(xline);
            xtext.Text = "X 汞量(ng)";
            Canvas.SetTop(xtext, rCanvas.Height - 20 - bottommargin);
            Canvas.SetLeft(xtext, rCanvas.Width - rightmargin + 20);
            rCanvas.Children.Add(xtext);
            arrowx.Stroke = Brushes.Black;
            arrowx.StrokeThickness = 1;
            arrowx.X1 = rCanvas.Width - 10 - rightmargin + 20;
            arrowx.Y1 = rCanvas.Height - 10 - bottommargin;
            arrowx.X2 = rCanvas.Width - rightmargin +20;
            arrowx.Y2 = rCanvas.Height - bottommargin;
            rCanvas.Children.Add(arrowx);
            //绘制x轴scale
            for (int i = 0; i < x.Length; i++)
            {
                scaleline = new Line();
                scaleline.Stroke = Brushes.Black;
                scaleline.StrokeThickness = 1;
                scaleline.X1 = gapX * (i + 1)* ScaleGapX + leftmargin;
                scaleline.Y1 = rCanvas.Height -  bottommargin;
                scaleline.X2 = gapX * (i + 1)* ScaleGapX + leftmargin;
                scaleline.Y2 = rCanvas.Height -5 - bottommargin;
                rCanvas.Children.Add(scaleline);
                scaletext = new TextBlock();
                scaletext.Text = (gapX * (i + 1)).ToString();
                Canvas.SetLeft(scaletext, gapX*(i+1)* ScaleGapX - 20 + leftmargin);
                Canvas.SetTop(scaletext, (rCanvas.Height - 20) - bottommargin);
                rCanvas.Children.Add(scaletext);
            }

            //画斜线
            double x1 = 0 + leftmargin;
            double y1 = validheight - b * ScaleGapY + topmargin;
            double x2 = maxX * ScaleGapX + leftmargin;
            double y2 = validheight - maxY * ScaleGapY + topmargin;
            Line mydrawline = new Line();
            mydrawline.Stroke = Brushes.Black;//mydrawline.Stroke = new SolidColorBrush(Color.FromArgb(0xFF, 0x5B, 0x9B, 0xD5));
            mydrawline.StrokeThickness = 1;
            mydrawline.X1 = x1;
            mydrawline.Y1 = y1;
            mydrawline.X2 = x2;
            mydrawline.Y2 = y2;
            rCanvas.Children.Add(mydrawline);

            //画点
            for (int i = 0; i < x.Length; i++)
            {
                //画中心点
                Ellipse centereli = new Ellipse();
                centereli.Stroke = System.Windows.Media.Brushes.Red;
                centereli.Fill = System.Windows.Media.Brushes.Red;
                centereli.Width = 3;
                centereli.Height = 3;
                Thickness mrg = new Thickness(x[i] * ScaleGapX + leftmargin -1.5, 
                    validheight - y[i] * ScaleGapY + topmargin -1.5, 0, 0);
                centereli.Margin = mrg;
                rCanvas.Children.Add(centereli);
                //画外圈
                Ellipse outeli = new Ellipse();
                outeli.Stroke = System.Windows.Media.Brushes.Blue;
                //outeli.Fill = System.Windows.Media.Brushes.Red;
                outeli.Width = 6;
                outeli.Height = 6;
                Thickness outmrg = new Thickness(x[i] * ScaleGapX + leftmargin -3, 
                    validheight - y[i] * ScaleGapY + topmargin -3, 0, 0);
                outeli.Margin = outmrg;
                rCanvas.Children.Add(outeli);
            }
            //在右侧显示显示信息
            TextBlock RparamSpTxbl = new TextBlock();
            //RparamSpTxbl.Text = "";
            RparamSpTxbl.Width = 150;
            switch(testmoduleid)
            {
                case TestModule.AIR_ADJUST_ZERO_ATOM_IN_AHEAD:
                case TestModule.AIR_ATOM_IN_AHEAD:
                    RparamSpTxbl.Text = "样品名称  响应值  汞量ng\r\n";
                    break;
                case TestModule.HOT_AIR_BOX:
                    RparamSpTxbl.Text = "样品名称  响应值  汞含量mg\r\n";
                    break;
                case TestModule.LIQUID_MULTI_BULK:
                case TestModule.LIQUID_STANDARD_BULK:
                    RparamSpTxbl.Text = "样品名称  响应值  汞浓度ug/L\r\n";
                    break;
                case TestModule.AIR_GOLD_ATOM_IN_BACK:
                default:
                    RparamSpTxbl.Text = "样品名称  响应值  汞量ng\r\n";
                    break;
            }
            
            int j = 0;
            for (int i=0; i < x.Length; i++)
            {
                for(int k=j; k < standardSampleClt.Count; k++)
                {
                    {
                        switch (testmoduleid)
                        {
                            case TestModule.AIR_ADJUST_ZERO_ATOM_IN_AHEAD:
                            case TestModule.AIR_ATOM_IN_AHEAD:
                            case TestModule.AIR_GOLD_ATOM_IN_BACK:
                            case TestModule.HOT_AIR_BOX:
                                if (standardSampleClt[k].GroupName == groupname &&
                                    standardSampleClt[k].AirG == x[i].ToString() &&
                                    standardSampleClt[k].ResponseValue1 == y[i].ToString()
                                    )
                                {
                                    samplename = standardSampleClt[k].SampleName;
                                    break;
                                }
                                break;
                            case TestModule.LIQUID_MULTI_BULK:
                            case TestModule.LIQUID_STANDARD_BULK:
                            default:
                                if (standardSampleClt[k].GroupName == groupname &&
                                    standardSampleClt[k].Density == x[i].ToString() &&
                                    standardSampleClt[k].ResponseValue1 == y[i].ToString()
                                    )
                                {
                                    samplename = standardSampleClt[k].SampleName;
                                    break;
                                }
                                break;
                        }
                    }
                }
                RparamSpTxbl.Text += samplename +
                    "\t" + cutnn(y[i].ToString()) + "\t" +
                    cutnn(x[i].ToString()) + "\r\n";
            }
            RparamSpTxbl.Text += "\r\n";
            RparamSpTxbl.Text += "斜率:  " + a.ToString() + "\r\n";
            RparamSpTxbl.Text += "截距:  " + b.ToString() + "\r\n";
            RparamSpTxbl.Text += "相关系数:  " + r.ToString();
            Canvas.SetTop(RparamSpTxbl, 20);
            Canvas.SetLeft(RparamSpTxbl, rCanvas.Width - rightmargin + 30);
            
            rCanvas.Children.Add(RparamSpTxbl);
        }

        //去掉末尾的/n符号
        private string cutnn(string str)
        {
            string result = "";
            for(int i = 0; i<str.Length;i++)
            {
                if (str[i] != '\r' && str[i] != '\n')
                    result += str[i];
            }
            return result;
        }

        public int getStandardCltIndexFromSelected(StandardSample obj)
        {

            if (obj is null) return -1;
            string code = obj.Code;

            for (int i = 0; i < standardSampleClt.Count; i++)
            {
                if (standardSampleClt[i].Code == code)
                    return i;
            }
            return 0;
        }

        //在标样选择中，将视图选中序号转变为数据源中序号
        //查询选中条目对应序号。语意有错，入参index无用。为了减少代码修改，保持该函数不变
        public int getStandardCltIndex(int index)
        {
            
            if (standardSampleDgd.SelectedItem is null) return -1;
            string code = (standardSampleDgd.SelectedItem as StandardSample).Code;
            
            for(int i=0; i < standardSampleClt.Count; i++)
            {
                if (standardSampleClt[i].Code == code)
                    return i;
            }
            return 0;
        }

        private void standardSampleDgd_SelectionChanged_computeR()
        {
            //计算相关系数及截率
            double[] x;
            double[] y;
            int len = 0;
            int index = 0;
            int cltindex = 0;
            double a, b, R;

            if (standardSampleDgd.SelectedIndex < 0) return;

            cltindex = getStandardCltIndex(standardSampleDgd.SelectedIndex);
            if (cltindex == -1) return;

            //数据未测试完成，则不计算相关系数
            string groupname = standardSampleClt[cltindex].GroupName;
            foreach (StandardSample item in standardSampleClt)
            {
                if (item.GroupName == groupname)
                {
                    if (item.ResponseValue1 == "" || item.ResponseValue1 is null ||
                        item.AirG == "" || item.AirG is null
                        )
                    {
                        //MessageBox.Show("测试未完成，无法计算R");
                        return;
                    }
                    else
                    {
                        len++;
                    }
                }
            }

            //收集新x,y数组
            x = new double[len];
            y = new double[len];
            foreach (StandardSample v in standardSampleClt)
            {
                if (v.GroupName == groupname)
                {
                    switch (testmoduleid)
                    {
                        case TestModule.AIR_ADJUST_ZERO_ATOM_IN_AHEAD:
                        case TestModule.AIR_ATOM_IN_AHEAD:
                        case TestModule.AIR_GOLD_ATOM_IN_BACK:
                        case TestModule.HOT_AIR_BOX:
                            x[index] = double.Parse(v.AirG);
                            break;
                        case TestModule.LIQUID_MULTI_BULK:
                        case TestModule.LIQUID_STANDARD_BULK:
                            x[index] = double.Parse(v.Density);
                            break;
                    }


                    y[index] = double.Parse(v.ResponseValue1);
                    index++;
                }
            }

            Utility.ComputeAB(out a, out b, x, y);

            R = Utility.ComputeR(x, y);
            if (x.Length == 1)
            {
                R = 1;
                a = 1;
            }
            foreach (StandardSample v in standardSampleClt)
            {
                if (v.GroupName == groupname)
                {
                    v.A = Math.Round(a, 2).ToString();
                    v.B = Math.Round(b, 2).ToString();
                    v.R = Math.Round(R, 3).ToString();
                }

            }
            //}
            //绘制R 线性回归图
            //
            if (a.ToString() == "NaN" || b.ToString() == "NaN") return;
            drawR(x, y, Math.Round(a, 2), Math.Round(b, 2), Math.Round(R, 3), groupname);
        }

        private TestingTabType getTypeOfTestingItem(string gid)
        {
            if (gid[0] == 'S')
                return TestingTabType.STANDARD;
            else if (gid[0] == 'N')
                return TestingTabType.NEW;
            else
                return TestingTabType.UNKNOWN;
        }

        private void standardSampleDgd_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            standardSampleDgd_SelectionChanged_computeR();
            //缓存及切换波形
            //正在测试，不能切换波形
            if (this.teststatus == TestingStatus.TESTING)
                return;
            //测试停止，离开测试条目之前，cache波形数据
            if (this.teststatus == TestingStatus.STOPPED)
            {
                //从测试条目离开，保存测试波形
                StandardSample aold;
                if (e.RemovedItems.Count == 0)
                    aold = null;
                else
                    aold = e.RemovedItems[0] as StandardSample;
                Collection<ADot> dots = DotManager.GetDotManger().GetDots();
                if (aold != null && dots.Count != 0 && aold.GlobalID == testing_gid)
                {
                    TableCache.GetTableCache().CacheWave(getTypeOfTestingItem(testing_gid), testing_gid, DotManager.GetDotManger().GetDots());
                }
                else
                {
                    //失去焦点情况下，默认上次是选中的是真正测试的条目
                    if (aold == null && dots.Count != 0)
                    {
                        TableCache.GetTableCache().CacheWave(getTypeOfTestingItem(testing_gid), testing_gid, DotManager.GetDotManger().GetDots());
                    }
                }
                //加载波形
                if (e.AddedItems.Count == 0)
                    return;
                string temp_gid = (e.AddedItems[0] as StandardSample).GlobalID;
                Collection<ADot> wave = TableCache.GetTableCache().FindWave(getTypeOfTestingItem(temp_gid), temp_gid);
                if (wave == null) return;
                waveinfo_txt.Text = "样本 ." + temp_gid;
                realCpt.SetScale(100, 2000, 0, 50);
                realCpt.SetNumberOfDValueP(10000);
                realCpt.ClearData();
                for (int i = 0; i < wave.Count; i++)
                {
                    realCpt.AddPoint(new Point(i, wave[i].Rvalue));
                }
            }
        }
        /*
        private void testliquidMenu_Click(object sender, RoutedEventArgs e)
        {
            newAirSampTimeCol.Visibility = Visibility.Hidden;
            newAirFluentCol.Visibility = Visibility.Hidden;
            newLiquidBulkCol.Visibility = Visibility.Visible;
            standardAirSampleTimeCol.Visibility = Visibility.Hidden;
            standardAirFluentCol.Visibility = Visibility.Hidden;
            standardPlaceCol.Visibility = Visibility.Visible;
            standardProviderCol.Visibility = Visibility.Visible;
            sampleType = SampleType.LIQUID;
        }

        private void testairMenu_Click(object sender, RoutedEventArgs e)
        {
            newAirSampTimeCol.Visibility = Visibility.Visible;
            newAirFluentCol.Visibility = Visibility.Visible;
            newLiquidBulkCol.Visibility = Visibility.Hidden;
            standardAirSampleTimeCol.Visibility = Visibility.Visible;
            standardAirFluentCol.Visibility = Visibility.Visible;
            standardPlaceCol.Visibility = Visibility.Hidden;
            standardProviderCol.Visibility = Visibility.Hidden;
            sampleType = SampleType.AIR;
        }
        */
        /*
         * 将表格中的数据保存到历史xml文件中
         */
        private void savehistory_Click(object sender, RoutedEventArgs e)
        {
            Utility.Save2XmlHistory(newTestClt,standardSampleClt);
            ;
        }

        private void loadhistory_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.DefaultExt = "xml";
            openDialog.Filter = "xml 文件|*.xml";
            openDialog.ShowDialog();
            string filename = openDialog.FileName;
            ;
            //截取日期
            int p1 = filename.IndexOf("表格");
            if (p1 == -1) return;
            p1 += 2;
            int p2 = filename.IndexOf(".xml");
            string suffix = "";
            //if (filename.Length < 19) return;
            /*
            for (int i = filename.Length - 18; i < filename.Length - 4; i++)
            {
                suffix += filename[i];
            }
            */
            for(int i = p1; i < p2; i++)
            {
                suffix += filename[i];
            }
            ;
            string newtestxml = "history\\样本测试表格" + suffix + ".xml";
            string standardtestxml = "history\\标样测试表格" + suffix + ".xml";

            newTestClt = Utility.getNewTestTargetDataFromXml(newtestxml);
            NewTargetDgd.DataContext = newTestClt;
            standardSampleClt = Utility.getStandardTargetDataFromXml(standardtestxml);
            standardSampleDgd.DataContext = standardSampleClt;
        }

        //更新新样数据源，保持数据同步
        private void updateNewClt(string v, int row, string head)
        {
            int index = getNewCltIndex(row);
            if (index == -1) return;
            switch (head)
            {
                case "样品名":
                    newTestClt[index].NewName = v;
                    break;
                case "产地":
                    newTestClt[index].Place = v;
                    break;
                case "响应值":
                    newTestClt[index].ResponseValue1 = v;
                    break;
                case "取样时间m":
                    newTestClt[index].AirSampleTime = v;
                    break;
                case "流量L/m":
                case "流量L/min":
                    newTestClt[index].AirFluent = v;
                    break;
                case "汞浓度ng/mL":
                    newTestClt[index].Density = v;
                    break;
                case "样品质量":
                case "样品质量g":
                    newTestClt[index].Weight = v;
                    break;
                case "样品消化液总体积mL":
                case "样品总体积L":
                case "体积L":
                    newTestClt[index].AirTotolBulk = v;
                    break;
                case "样品中汞含量mg/m3":
                case "样品中汞含量mg/kg":
                    newTestClt[index].AirG = v;
                    break;
                case "汞浓度mg/m3":
                case "汞浓度ug/L":
                    newTestClt[index].Density = v;
                    break;
                case "样品气体总流量L/min":
                    newTestClt[index].AirTotalFluent = v;
                    break;
                case "汞含量mg":
                case "汞含量ng/L":
                    newTestClt[index].ThingInSamle = v;
                    break;
                case "进样量mL":
                    newTestClt[index].InSampleQuality = v;
                    break;
            }
        }

        //更新标样数据源，保持数据同步
        private void updateStandardClt(string v, int row, string head)
        {
            int index = getStandardCltIndex(row);
            if (index == -1) return;
            switch (head)
            {
                case "组名":
                    standardSampleClt[index].GroupName = v;
                    break;
                case "样品名":
                    standardSampleClt[index].SampleName = v;
                    break;
                case "汞浓度ng/mL":
                case "汞浓度mg/m3":
                case "汞浓度ug/L":
                    standardSampleClt[index].Density = v;
                    break;
                case "响应值":
                    standardSampleClt[index].ResponseValue1 = v;
                    break;
                case "取样时间m":
                    //standardSampleClt[index].AirSampleTime = v;
                    break;
                case "流量L/m":
                    standardSampleClt[index].AirFluent = v;
                    break;
                case "温度":
                    standardSampleClt[index].Temperature = v;
                    break;
                case "标样体积mL":
                case "体积L":
                case "总体积mL":
                    standardSampleClt[index].AirML = v;
                    break;
                case "汞量ng":
                case "汞量mg":
                    standardSampleClt[index].AirG = Math.Round(double.Parse(v),2).ToString();
                    break;
                case "样品质量":
                    standardSampleClt[index].Weight = v;
                    break;
                case "样品供应商":
                    standardSampleClt[index].ProviderCompany = v;
                    break;
                case "产地":
                    standardSampleClt[index].Place = v;
                    break;
                case "样品购买日期":
                    standardSampleClt[index].BuyDate = v;
                    break;
                case "汞流量mg/L":
                    standardSampleClt[index].AirFluent = v;
                    break;
            }
        }

        private void NewTargetDgd_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditingElement is null) return;
            if (!(e.EditingElement is TextBox)) return;
            string vle = (e.EditingElement as TextBox).Text;
            string headname = e.Column.Header as string;
            //int cltindex = getNewCltIndex(NewTargetDgd.SelectedIndex);

            //将数据更新到数据源
            updateNewClt(vle, NewTargetDgd.SelectedIndex, headname);

        }

        private void standardSampleDgd_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditingElement is null) return;
            if (!(e.EditingElement is TextBox)) return;
            string vle = (e.EditingElement as TextBox).Text;
            string headname = e.Column.Header as string;
            int cltindex = getStandardCltIndex(standardSampleDgd.SelectedIndex);
            if (cltindex == -1) return;
            double den, bulk;
            XmlNode node;
            //将数据更新到数据源
            updateStandardClt(vle, standardSampleDgd.SelectedIndex, headname);
            if (vle == "" || vle is null)
                return;
            //计算汞量
            switch (headname)
            {
                case "温度":
                    if (standardSampleClt[cltindex].AirML is null || standardSampleClt[cltindex].AirML == "")
                        return;
                    try
                    {
                        node = AirDensityXml.SelectSingleNode("/air/density[@hot=" + vle + "]");
                    }
                    catch
                    {
                        //MessageBox.Show("");
                        return;
                    }
                    
                    if (node is null) return;

                    try
                    {
                        bulk = double.Parse(standardSampleClt[cltindex].AirML);
                        den = double.Parse(node.InnerText);
                    }
                    catch
                    {
                        return;
                    }

                    standardSampleClt[cltindex].AirG = Math.Round((den * bulk),2).ToString();
                    break;
                case "标样体积mL":
                    if (standardSampleClt[cltindex].Temperature is null || standardSampleClt[cltindex].Temperature == "")
                        return;
                    string tem = standardSampleClt[cltindex].Temperature;
                    node = AirDensityXml.SelectSingleNode("/air/density[@hot=" + tem + "]");
                    if (node is null) return;
                    try
                    {
                        bulk = double.Parse(vle);
                        den = double.Parse(node.InnerText);
                    }
                    catch
                    {
                        return;
                    }
                    standardSampleClt[cltindex].AirG = Math.Round((den * bulk),2).ToString();
                    break;

            }

        }

        private void printRmenu_Click(object sender, RoutedEventArgs e)
        {
            //rCanvas.Width = 700;
            //rCanvas.Height = 400;
            PrintDialog dlg = new PrintDialog();
            if (dlg.ShowDialog() == true)
            {
                dlg.PrintVisual(rCanvas, "Print Receipt");
            }
        }

        private void exitMenu_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void saveallbtn_Click(object sender, RoutedEventArgs e)
        {
            Utility.SaveToNewXmlFileCls.SaveToNewXmlFile(newTestClt, "resources\\NewTestTarget_Table.xml");
            Utility.SaveToStandardXmlFileCls.SaveToStandardXmlFile(standardSampleClt, "resources\\StandardSamples_Table.xml");
            MessageBox.Show("数据已保存");
        }

        private void exportExcelBtn_Click(object sender, RoutedEventArgs e)
        {
            switch (sampletab.SelectedIndex)
            {
                case 0:  //新样
                    Utility.Save2excel(NewTargetDgd);
                    break;
                case 1: //标样
                    Utility.Save2excel(standardSampleDgd);
                    break;
            }
        }

        private void printRbtn_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog dlg = new PrintDialog();
            if (dlg.ShowDialog() == true)
            {
                dlg.PrintVisual(rCanvas, "Print Receipt");
            }
        }

        private void exportDotsBtn_Click(object sender, RoutedEventArgs e)
        {
            string filename = "";

            Collection<ADot> dots = DotManager.GetDotManger().GetDots();
            if (dots is null || dots.Count == 0)
            {
                MessageBox.Show("没有发现测试数据");
                return;
            }

            string now = DateTime.Now.ToString();
            string suffix = "";
            for (int i = 0; i < now.Length; i++)
            {
                if (now[i] != ':' && now[i] != '/' && now[i] != ' ')
                {
                    suffix += now[i];
                }
            }
            ;
            switch (sampletab.SelectedIndex)
            {
                case 0:  //新样测试
                    filename = "history\\样本测试原始数据" + suffix + ".raw";
                    break;
                case 1:  //标样测试
                    filename = "history\\标样测试原始数据" + suffix + ".raw";
                    break;
            }


            FileStream aFile = new FileStream(filename,FileMode.Create);
            StreamWriter sr = new StreamWriter(aFile);
            for (int i = 0; i < dots.Count; i++)
            {
                sr.Write(dots[i].Rvalue.ToString()+"\r\n");

            }
            sr.Close();
            aFile.Close();

            MessageBox.Show("文件已保存到history目录中");

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void standardCmb_Loaded(object sender, RoutedEventArgs e)
        {
            //去除重复的组名
            ComboBox cmb = sender as ComboBox;
            cmb.ItemsSource = standardSampleDgd.ItemsSource;
            Collection<StandardSample> items = cmb.ItemsSource as Collection<StandardSample>;
            if (items.Count == 0) return;

            Collection<StandardSample> tempitem = new Collection<StandardSample>();
            tempitem.Add(items[0]);
            bool isok = true;
            
            for(int i = 1; i< items.Count; i++)
            {
                for(int k = 0; k < tempitem.Count; k++)
                {
                    if(items[i].GroupName == tempitem[k].GroupName)
                    {
                        isok = false;
                        break;
                    }
                }
                if (isok)
                {
                    tempitem.Add(items[i]);
                }
                isok = true;
            }

            cmb.ItemsSource = tempitem;
        }

        private void checkerlogin_Click(object sender, RoutedEventArgs e)
        {
            Window w = new CheckerLoginWnd();
            w.Owner = this;
            w.ShowDialog();
        }

        private void checkerlogout_Click(object sender, RoutedEventArgs e)
        {
            checkerbtn.Visibility = Visibility.Hidden;
        }

        private void modifytestmenu_Click(object sender, RoutedEventArgs e)
        {
            switch (sampletab.SelectedIndex)
            {
                case 0:
                    if(NewTargetDgd.SelectedIndex == -1)
                    {
                        MessageBox.Show("请选择一条记录");
                        return;
                    }
                    break;
                case 1:
                    if(standardSampleDgd.SelectedIndex == -1)
                    {
                        MessageBox.Show("请选择一条记录");
                        return;
                    }
                    break;
            }
            ModifyDataWnd w = new ModifyDataWnd();
            w.Owner = this;
            w.ShowDialog();
        }

        private void exportexcelmenu_Click(object sender, RoutedEventArgs e)
        {
            switch (sampletab.SelectedIndex)
            {
                case 0:  //新样
                    Utility.Save2excel(NewTargetDgd);
                    break;
                case 1: //标样
                    Utility.Save2excel(standardSampleDgd);
                    break;
            }
        }

        private void clearbtn_Click(object sender, RoutedEventArgs e)
        {
            int index_out;
            switch (sampletab.SelectedIndex)
            {
                case 0: //新样
                    while (newTestClt.Count > 0)
                    {
                        newTestClt.RemoveAt(0);
                    }
                    NewTestTarget n1 = new NewTestTarget();
                    
                    n1.NewName = "样品空白";
                    n1.GlobalID = GIDMaker.GetMaker().GetNId(out index_out);
                    n1.AirG = "0";
                    n1.Code = index_out.ToString();

                    NewTestTarget n2 = new NewTestTarget();
                    n2.NewName = "样品";
                    n2.GlobalID = GIDMaker.GetMaker().GetNId(out index_out);
                    n2.Code = index_out.ToString();
                    n2.NewName += (index_out - 1).ToString();

                    NewTestTarget n3 = new NewTestTarget();
                    n3.NewName = "样品";
                    n3.GlobalID = GIDMaker.GetMaker().GetNId(out index_out);
                    n3.Code = index_out.ToString();
                    n3.NewName += (index_out - 1).ToString();

                    NewTestTarget n4 = new NewTestTarget();
                    n4.NewName = "样品";
                    n4.GlobalID = GIDMaker.GetMaker().GetNId(out index_out);
                    n4.NewName += (index_out - 1).ToString();
                    n4.Code = index_out.ToString();

                    newTestClt.Add(n1);
                    newTestClt.Add(n2);
                    newTestClt.Add(n3);
                    newTestClt.Add(n4);
                    break;
                case 1: //标样
                    while (standardSampleClt.Count > 0)
                    {
                        standardSampleClt.RemoveAt(0);
                    }
                    StandardSample s1 = new StandardSample();

                    s1.SampleName = "标样空白";
                    s1.GroupName = "组1";
                    s1.AirG = "0";
                    s1.GlobalID = GIDMaker.GetMaker().GetSId(out index_out);
                    s1.Code = index_out.ToString();

                    StandardSample s2 = new StandardSample();
                    s2.SampleName = "标样";
                    s2.GroupName = "组1";
                    s2.GlobalID = GIDMaker.GetMaker().GetSId(out index_out);
                    s2.Code = index_out.ToString();
                    s2.SampleName += (index_out - 1).ToString();

                    StandardSample s3 = new StandardSample();
                    s3.SampleName = "标样";
                    s3.GroupName = "组1";
                    s3.GlobalID = GIDMaker.GetMaker().GetSId(out index_out);
                    s3.Code = index_out.ToString();
                    s3.SampleName += (index_out - 1).ToString();

                    StandardSample s4 = new StandardSample();
                    s4.SampleName = "标样";
                    s4.GroupName = "组1";
                    s4.GlobalID = GIDMaker.GetMaker().GetSId(out index_out);
                    s4.Code = index_out.ToString();
                    s4.SampleName += (index_out - 1).ToString();

                    standardSampleClt.Add(s1);
                    standardSampleClt.Add(s2);
                    standardSampleClt.Add(s3);
                    standardSampleClt.Add(s4);
                    break;
            }
        }

        private void commandBtn_Click(object sender, RoutedEventArgs e)
        {
            if (cmdpanelWnd == null)
            {
                cmdpanelWnd = new CommandPanelWnd();
                cmdpanelWnd.Owner = this;
            }
            cmdpanelWnd.Show();
        }

        private void setportmenu_Click(object sender, RoutedEventArgs e)
        {
            SetPortWnd w = new SetPortWnd();
            w.Owner = this;
            w.ShowDialog();
        }

        private void rs232connectedbtn_Click(object sender, RoutedEventArgs e)
        {
            SerialDriver.GetDriver().Close();
            showconnectedIcon();
        }

        private void rs232disconnectedbtn_Click(object sender, RoutedEventArgs e)
        {
            SerialDriver.GetDriver().Open(
                SerialDriver.GetDriver().portname,
                SerialDriver.GetDriver().rate,
                SerialDriver.GetDriver().parity,
                SerialDriver.GetDriver().databits,
                SerialDriver.GetDriver().stopbits);
            showconnectedIcon();
        }

        private void NewTargetDgd_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ContextMenu cm1 = this.FindResource("newtableMenu") as ContextMenu;
            cm1.PlacementTarget = sender as DataGrid;
            cm1.IsOpen = true;
            
        }

        private void standardSampleDgd_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ContextMenu cm2 = this.FindResource("sampletableMenu") as ContextMenu;
            cm2.PlacementTarget = sender as DataGrid;
            cm2.IsOpen = true;
        }
        private void newtablemenu_showhistory(object sender, RoutedEventArgs e)
        {
            WaveHistoryWnd w = new WaveHistoryWnd();
            w.Owner = this;
            w.ShowDialog();
        }
        private void sampletableMenu_showhistory(object sender, RoutedEventArgs e)
        {
            WaveHistoryWnd w = new WaveHistoryWnd();
            w.Owner = this;
            w.ShowDialog();
        }


        private void newsavedotsPMenu_Click(object sender, RoutedEventArgs e)
        {
            if(NewTargetDgd.SelectedIndex == -1)
            {
                MessageBox.Show("请选择正在测试的样品");
                return;
            }
            if (DotManager.GetDotManger().GetDots() is null || DotManager.GetDotManger().GetDots().Count == 0)
            {
                MessageBox.Show("当前样品没有测试数据");
                return;
            }
            string filename = newTestClt[getNewCltIndex(NewTargetDgd.SelectedIndex)].GlobalID;
            filename += "_" + DateTime.Now.Year;
            if(DateTime.Now.Month.ToString().Length == 2)
                filename += DateTime.Now.Month;
            else
                filename += "0" + DateTime.Now.Month;

            if(DateTime.Now.Day.ToString().Length == 2)
                filename += DateTime.Now.Day;
            else
                filename += "0" + DateTime.Now.Day;

            if(DateTime.Now.Hour.ToString().Length == 2)
                filename += DateTime.Now.Hour;
            else
                filename += "0" + DateTime.Now.Hour;
            
            if(DateTime.Now.Minute.ToString().Length == 2)
                filename += DateTime.Now.Minute;
            else
                filename += "0" + DateTime.Now.Minute;

            if(DateTime.Now.Second.ToString().Length == 2)
                filename += DateTime.Now.Second + ".bin";
            else
                filename += "0" + DateTime.Now.Second + ".bin";

            //Utility.SaveToNewXmlFileCls.SaveToNewXmlFile(newTestClt, "resources\\NewTestTarget_Table.xml");
            saveNewDotsToFile(@"history\"+filename);

            MessageBox.Show("数据已保存");
        }

        private void samplesavedotsPMenu_Click(object sender, RoutedEventArgs e)
        {
            if (standardSampleDgd.SelectedIndex == -1)
            {
                MessageBox.Show("请选择正在测试的标样");
                return;
            }
            if (DotManager.GetDotManger().GetDots() is null || DotManager.GetDotManger().GetDots().Count == 0)
            {
                MessageBox.Show("当前标样没有测试数据");
                return;
            }

            string filename = standardSampleClt[getStandardCltIndex(standardSampleDgd.SelectedIndex)].GlobalID;
            filename += "_" + DateTime.Now.Year;
            if(DateTime.Now.Month.ToString().Length == 2)
                filename +=  DateTime.Now.Month;
            else
                filename += "0" + DateTime.Now.Month;

            if(DateTime.Now.Day.ToString().Length == 2)
                filename += DateTime.Now.Day;
            else
                filename += "0" + DateTime.Now.Day;

            if(DateTime.Now.Hour.ToString().Length == 2)
                filename += DateTime.Now.Hour;
            else
                filename += "0" + DateTime.Now.Hour;

            if (DateTime.Now.Minute.ToString().Length == 2)
                filename += DateTime.Now.Minute;
            else
                filename += "0" + DateTime.Now.Minute;

            if(DateTime.Now.Second.ToString().Length == 2)
                filename += DateTime.Now.Second + ".bin";
            else
                filename += "0" + DateTime.Now.Second + ".bin";

            //Utility.SaveToNewXmlFileCls.SaveToNewXmlFile(newTestClt, "resources\\NewTestTarget_Table.xml");
            saveNewDotsToFile(@"history\"+filename);

            MessageBox.Show("数据已保存");
        }

        //将newTestClt中的点保存到文件中，以bin为扩展名
        private void saveNewDotsToFile(string filename)
        {
            FileStream aFile = new FileStream(filename, FileMode.Create);
            StreamWriter sr = new StreamWriter(aFile);
            Collection<ADot> dots = DotManager.GetDotManger().GetDots();
            foreach(ADot item in dots)
            {
                sr.Write(item.Rvalue.ToString() + "\r\n");
            }
            sr.Close();
            aFile.Close();
        }

        private void NewTargetDgd_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SolidColorBrush br = new SolidColorBrush();
            br.Color = Color.FromArgb(0xff,0xff,0x8b,0x8b);
            string gid = (e.Row.Item as NewTestTarget).GlobalID;
            if (gid == testingitemgid_new)
                e.Row.Background = br;
        }

        private void standardSampleDgd_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SolidColorBrush br = new SolidColorBrush();
            br.Color = Color.FromArgb(0xff, 0xff, 0x8b, 0x8b);
            string gid = (e.Row.Item as StandardSample).GlobalID;
            if (gid == testingitemgid_std)
                e.Row.Background = br;
        }

        private void rs232connectedbtn_MouseEnter(object sender, MouseEventArgs e)
        {
            
        }

        private void pauseTestBtn_Click(object sender, RoutedEventArgs e)
        {
            if(pauseTestBtn.Content.ToString() == "暂停测试")
            {
                pauseTestBtn.Content = "恢复测试";
                SerialDriver.GetDriver().Close();
                AnimatedColorButton.Spin = false;
            }
            else
            {
                pauseTestBtn.Content = "暂停测试";
                SerialDriver.GetDriver().Open(
                        SerialDriver.GetDriver().portname,
                        SerialDriver.GetDriver().rate,
                        SerialDriver.GetDriver().parity,
                        SerialDriver.GetDriver().databits,
                        SerialDriver.GetDriver().stopbits
                    );
                AnimatedColorButton.Spin = true;
            }
            showconnectedIcon();
        }

        private void toolBarTray_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Menu_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void AreaIntegrateValueChangedHdlr(double res)
        {
            Console.WriteLine(res.ToString("0.00"));
            int newcltindex;
            //Dispatcher.BeginInvoke(new Action(() =>
            //{
                switch (sampletab.SelectedIndex)
                {
                    case 0:         //新样测试
                        newcltindex = getNewCltIndexFromSelected(testing_selected_new);
                        if (newcltindex == -1) return;
                        newTestClt[newcltindex].ResponseValue1 =
                            res.ToString("0.00");
                        break;
                    case 1:         //标样测试
                        if (getStandardCltIndexFromSelected(testing_selected_standard) == -1) return;
                        standardSampleClt[getStandardCltIndexFromSelected(testing_selected_standard)].ResponseValue1 =
                            res.ToString("0.00");
                        break;
                }
            //}));
        }


        private void savefullitem()
        {
            //未选择数据
            switch (sampletab.SelectedIndex)
            {
                case 0:
                    if (NewTargetDgd.SelectedIndex == -1)
                    {
                        MessageBox.Show("请选择一条数据");
                        return;
                    }
                    break;
                case 1:
                    if (standardSampleDgd.SelectedIndex == -1)
                    {
                        MessageBox.Show("请选择一条数据");
                        return;
                    }
                    break;
            }

            //没有测试数据
            if (DotManager.GetDotManger().GetDots() is null || DotManager.GetDotManger().GetDots().Count == 0)
            {
                MessageBox.Show("当前样品没有测试数据");
                return;
            }

            //获取文件名
            string fullsaveFileName = "";
            string saveFileName = "";
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.DefaultExt = ".cls";
            saveDialog.Filter = "经典数据文件|*.cls";
            saveDialog.ShowDialog();
            fullsaveFileName = saveDialog.FileName;
            saveFileName = saveDialog.SafeFileName;
            if (saveFileName == "") return;
            string fullbinfilename = "";
            string binfilename = "";
            for (int i = 0; i < fullsaveFileName.Length - 3; i++)
            {
                fullbinfilename += fullsaveFileName[i];
            }
            fullbinfilename += "bin";
            for (int i = 0; i < saveFileName.Length - 3; i++)
            {
                binfilename += saveFileName[i];
            }
            binfilename += "bin";
            ;
            //save the bin file
            XmlSerializer serializer = null;
            switch (sampletab.SelectedIndex)
            {
                case 0: //新样
                    serializer = new XmlSerializer(typeof(NewTestTarget));
                    using (FileStream stream = new FileStream(fullsaveFileName, FileMode.Create))
                    {
                        serializer.Serialize(stream, newTestClt[getNewCltIndexFromSelected(NewTargetDgd.SelectedItem as NewTestTarget)]);
                    }
                    break;
                case 1: //标样
                    serializer = new XmlSerializer(typeof(StandardSample));
                    using (FileStream stream = new FileStream(fullsaveFileName, FileMode.Create))
                    {
                        serializer.Serialize(stream, standardSampleClt[getStandardCltIndexFromSelected(standardSampleDgd.SelectedItem as StandardSample)]);
                    }
                    break;
            }

            //save the item grid into cls file
            Collection<ADot> dots = DotManager.GetDotManger().GetDots();
            FileStream aFile = new FileStream(fullbinfilename, FileMode.Create);
            StreamWriter sr = new StreamWriter(aFile);
            for (int i = 0; i < dots.Count; i++)
            {
                sr.Write(dots[i].Rvalue.ToString() + "\r\n");

            }
            sr.Close();
            aFile.Close();
        }

        private void newitemfullsavePMenu_Click(object sender, RoutedEventArgs e)
        {
            savefullitem();
        }

        private void standarditemfullsavePMenu_Click(object sender, RoutedEventArgs e)
        {
            savefullitem();
        }

        private void loadkeyhistory_Click(object sender, RoutedEventArgs e)
        {
            FileStream aFile;
            OpenFileDialog odi = new OpenFileDialog();
            odi.DefaultExt = ".cls";
            odi.Filter = "经典数据文件|*.cls";
            odi.ShowDialog();
            string fullkeyFileName = odi.FileName;
            string keyFileName = odi.SafeFileName;
            string fullbinfilename = "";
            string binfilename = "";
            for (int i = 0; i < fullkeyFileName.Length - 3; i++)
            {
                fullbinfilename += fullkeyFileName[i];
            }
            fullbinfilename += "bin";
            for (int i = 0; i < keyFileName.Length - 3; i++)
            {
                binfilename += keyFileName[i];
            }
            binfilename += "bin";

            //将统计数据载入表格之中
            //1是标样数据还是新样数据
            XmlSerializer newserializer;
            NewTestTarget newitem = null;
            XmlSerializer standardserializer;
            StandardSample standarditem = null;
            TestingTabType tabtype = TestingTabType.UNKNOWN;
            string gid = "";
            try
            {
                newserializer = new XmlSerializer(typeof(NewTestTarget));
                int newindex = -1;
                using (FileStream stream = new FileStream(fullkeyFileName, FileMode.Open))
                {
                    newitem = (NewTestTarget)newserializer.Deserialize(stream);
                }
                if (newitem == null) return;
                for (int i = 0; i < newTestClt.Count; i++)
                {
                    if (newTestClt[i].GlobalID == newitem.GlobalID)
                    {
                        newindex = i;
                        break;
                    }
                }
                if (newindex == -1)//没有相同的
                {
                    newTestClt.Add(newitem);
                }
                else
                {
                    newTestClt.Remove(newTestClt[newindex]);
                    newTestClt.Add(newitem);
                }
                tabtype = TestingTabType.NEW;
                gid = newitem.GlobalID;
            }
            catch (Exception ex)
            {
                ;
            }
            if (newitem is null)
            {
                standardserializer = new XmlSerializer(typeof(StandardSample));
                try
                {
                    int standardindex = -1;
                    using (FileStream stream = new FileStream(fullkeyFileName, FileMode.Open))
                    {
                        standarditem = (StandardSample)standardserializer.Deserialize(stream);
                    }
                    if (standarditem == null) return;
                    for (int i = 0; i < standardSampleClt.Count; i++)
                    {
                        if (standardSampleClt[i].GlobalID == standarditem.GlobalID)
                        {
                            standardindex = i;
                            break;
                        }
                    }
                    if (standardindex == -1)//不存在这个条目
                        standardSampleClt.Add(standarditem);
                    else
                    {
                        standardSampleClt.RemoveAt(standardindex);
                        standardSampleClt.Add(standarditem);
                    }
                    tabtype = TestingTabType.STANDARD;
                    gid = standarditem.GlobalID;
                }
                catch (Exception ee)
                {
                    return;
                }
            }

            //载入波形
            realCpt.SetScale(100, 2000, 0, 50);
            //realCpt.NumberOfDValue = 200000;
            realCpt.SetNumberOfDValueP(200000);
            realCpt.ClearData();
            int xscale = 0;
            int yscale = 0;
            try{ 
                aFile = new FileStream(fullbinfilename, FileMode.Open);
            }
            catch (Exception er)
            {
                return;
            }
            //同时缓存波形
            Collection<ADot> dotscache = new Collection<ADot>();
            StreamReader sr = new StreamReader(aFile);
            while (!sr.EndOfStream)
            {
                yscale = int.Parse( sr.ReadLine() );
                realCpt.AddPoint(new Point(xscale,yscale));
                dotscache.Add(new ADot(xscale, yscale, DotStaus.OK));
                xscale++;
            }

            sr.Close();
            aFile.Close();
            TableCache.GetTableCache().CacheWave(tabtype, gid, dotscache);
        }

        private void window_Closed(object sender, EventArgs e)
        {
            try
            {
                cmdpanelWnd.Close();
            }
            catch (Exception er) {; }
        }

        private void controlpanelmenu_Click(object sender, RoutedEventArgs e)
        {
            if (cmdpanelWnd == null)
            {
                cmdpanelWnd = new CommandPanelWnd();
                cmdpanelWnd.Owner = this;
            }
            cmdpanelWnd.Show();
        }

        //热释气体炉测量
        private void hotairbox_itm_Click(object sender, RoutedEventArgs e)
        {
            testmoduleid = TestModule.HOT_AIR_BOX;

            hotairbox_itm.Icon = checkicon;
            airautomahead_itm.Icon = null;
            airautomback_itm.Icon = null;
            airadjustzeroahead_itm.Icon = null;
            liquidmultibox_itm.Icon = null;
            liquidstandardbox_itm.Icon = null;

            /* -- 新样 --- */
            //取样时间
            newAirSampTimeCol.Visibility = Visibility.Collapsed;
            newAirSampTimeCol.Header = newAirSampTimeCol.Header;
            //流量l/m
            newAirFluentCol.Visibility = Visibility.Collapsed;
            newAirFluentCol.Header = newAirFluentCol.Header;
            //样品质量
            newqualityCol.Visibility = Visibility.Collapsed;
            newqualityCol.Header = newqualityCol.Header;
            //样品总体积L
            newtotalbulkCol.Visibility = Visibility.Visible;
            newtotalbulkCol.Header = "体积L";
            //样品中汞含量mg/m3
            newghltotalCol.Visibility = Visibility.Collapsed;
            newghltotalCol.Header = newghltotalCol.Header;
            //汞浓度mg/m3
            newgndCol.Visibility = Visibility.Visible;
            newgndCol.Header = "汞浓度mg/m3";
            //样品气体总流量L/min
            newypqtzllCol.Visibility = Visibility.Collapsed;
            newypqtzllCol.Header = newypqtzllCol.Header;
            //汞含量mg
            newghlCol.Visibility = Visibility.Visible;
            newghlCol.Header = "汞含量mg";
            //进样量ml
            newjylCol.Visibility = Visibility.Collapsed;
            newjylCol.Header = newjylCol.Header;

            /*--- 标样 ---*/
            //温度
            airtemperature.Visibility = Visibility.Collapsed;
            airtemperature.Header = "温度";
            //标样体积ml
            airbulk.Visibility = Visibility.Visible;
            airbulk.Header = "体积L";
            //汞量ng
            airhan.Visibility = Visibility.Visible;
            airhan.Header = "汞含量mg";
            //样品质量
            standardzl.Visibility = Visibility.Collapsed;
            standardzl.Header = "样品质量";
            //汞浓度mg/m3
            standardgndCol.Visibility = Visibility.Collapsed;
            standardgndCol.Header = "汞浓度mg/m3";
            //汞流量mg/L
            standardllCol.Visibility = Visibility.Collapsed;
            standardllCol.Header = "汞流量mg/L";

            NewTargetDgd.CommitEdit();
            standardSampleDgd.CommitEdit();
        }

        //气体测量原子吸收法前进样
        private void airautomahead_itm_Click(object sender, RoutedEventArgs e)
        {
            testmoduleid = TestModule.AIR_ATOM_IN_AHEAD;

            hotairbox_itm.Icon = null;
            airautomahead_itm.Icon = checkicon;
            airautomback_itm.Icon = null;
            airadjustzeroahead_itm.Icon = null;
            liquidmultibox_itm.Icon = null;
            liquidstandardbox_itm.Icon = null;

            /* -- 新样 --- */
            //取样时间
            newAirSampTimeCol.Visibility = Visibility.Collapsed;
            //流量l/m
            newAirFluentCol.Visibility = Visibility.Visible;
            newAirFluentCol.Header = "流量L/min";
            //样品质量
            newqualityCol.Visibility = Visibility.Collapsed;
            //样品总体积L
            newtotalbulkCol.Visibility = Visibility.Collapsed;
            //样品中汞含量mg/m3
            newghltotalCol.Visibility = Visibility.Visible;
            newghltotalCol.Header = "样品中汞含量mg/m3";
            //汞浓度mg/m3
            newgndCol.Visibility = Visibility.Collapsed;
            //样品气体总流量L/min
            newypqtzllCol.Visibility = Visibility.Visible;
            newypqtzllCol.Header = "样品气体总流量L/min";
            //汞含量mg
            newghlCol.Visibility = Visibility.Visible;
            newghlCol.Header = "汞含量ng/L";
            //进样量ml
            newjylCol.Visibility = Visibility.Collapsed;

            /*--- 标样 ---*/
            //温度
            airtemperature.Visibility = Visibility.Collapsed;
            //标样体积ml
            airbulk.Visibility = Visibility.Collapsed;
            //汞量ng
            airhan.Visibility = Visibility.Visible;
            airhan.Header = "汞量ng";
            //样品质量
            standardzl.Visibility = Visibility.Collapsed;
            //汞浓度mg/m3
            standardgndCol.Visibility = Visibility.Visible;
            standardgndCol.Header = "汞浓度mg/m3";
            //汞流量mg/L
            standardllCol.Visibility = Visibility.Visible;
            standardllCol.Header = "汞流量mg/L";

            NewTargetDgd.CommitEdit();
            standardSampleDgd.CommitEdit();
        }

        //气体测量金属膜原子吸收法后进样(等价于201H)
        private void airautomback_itm_Click(object sender, RoutedEventArgs e)
        {
            testmoduleid = TestModule.AIR_GOLD_ATOM_IN_BACK;

            hotairbox_itm.Icon = null;
            airautomahead_itm.Icon = null;
            airautomback_itm.Icon = checkicon;
            airadjustzeroahead_itm.Icon = null;
            liquidmultibox_itm.Icon = null;
            liquidstandardbox_itm.Icon = null;

            /* -- 新样 --- */
            //取样时间
            newAirSampTimeCol.Visibility = Visibility.Visible;
            newAirSampTimeCol.Header = "取样时间M";
            //流量l/m
            newAirFluentCol.Visibility = Visibility.Visible;
            newAirFluentCol.Header = "流量L/M";
            //样品质量
            newqualityCol.Visibility = Visibility.Visible;
            newqualityCol.Header = "样品质量";
            //样品总体积L
            newtotalbulkCol.Visibility = Visibility.Visible;
            newtotalbulkCol.Header = "样品总体积L";
            //样品中汞含量mg/m3
            newghltotalCol.Visibility = Visibility.Visible;
            newghltotalCol.Header = "样品中汞含量MG/M3";
            //汞浓度mg/m3
            newgndCol.Visibility = Visibility.Collapsed;
            //newgndCol.Header = "汞浓度mg/m3";
            //样品气体总流量L/min
            newypqtzllCol.Visibility = Visibility.Collapsed;
            //newypqtzllCol.Header = "样品气体总流量L/min";
            //汞含量mg
            newghlCol.Visibility = Visibility.Collapsed;
            //newghlCol.Header = "汞含量mg";
            //进样量ml
            newjylCol.Visibility = Visibility.Collapsed;
            //newjylCol.Header = newjylCol.Header;

            /*--- 标样 ---*/
            //温度
            airtemperature.Visibility = Visibility.Visible;
            airtemperature.Header = "温度";
            //标样体积ml
            airbulk.Visibility = Visibility.Visible;
            airbulk.Header = "标样体积ML";
            //汞量ng
            airhan.Visibility = Visibility.Visible;
            airhan.Header = "汞量ng";
            //样品质量
            standardzl.Visibility = Visibility.Visible;
            standardzl.Header = "样品质量";
            //汞浓度mg/m3
            standardgndCol.Visibility = Visibility.Collapsed;
            standardgndCol.Header = "汞浓度mg/m3";
            //汞流量mg/L
            standardllCol.Visibility = Visibility.Collapsed;
            standardllCol.Header = "汞流量mg/L";

            NewTargetDgd.CommitEdit();
            standardSampleDgd.CommitEdit();
        }

        //气体自校零原子吸收法前进样(等价于原子吸收法前进样)
        private void airadjustzeroahead_itm_Click(object sender, RoutedEventArgs e)
        {
            testmoduleid = TestModule.AIR_ADJUST_ZERO_ATOM_IN_AHEAD;

            hotairbox_itm.Icon = null;
            airautomahead_itm.Icon = null;
            airautomback_itm.Icon = null;
            airadjustzeroahead_itm.Icon = checkicon;
            liquidmultibox_itm.Icon = null;
            liquidstandardbox_itm.Icon = null;

            /* -- 新样 --- */
            //取样时间
            newAirSampTimeCol.Visibility = Visibility.Collapsed;
            //流量l/m
            newAirFluentCol.Visibility = Visibility.Visible;
            newAirFluentCol.Header = "流量L/min";
            //样品质量
            newqualityCol.Visibility = Visibility.Collapsed;
            //样品总体积L
            newtotalbulkCol.Visibility = Visibility.Collapsed;
            //样品中汞含量mg/m3
            newghltotalCol.Visibility = Visibility.Visible;
            newghltotalCol.Header = "样品中汞含量mg/m3";
            //汞浓度mg/m3
            newgndCol.Visibility = Visibility.Collapsed;
            //样品气体总流量L/min
            newypqtzllCol.Visibility = Visibility.Visible;
            newypqtzllCol.Header = "样品气体总流量L/min";
            //汞含量mg
            newghlCol.Visibility = Visibility.Visible;
            newghlCol.Header = "汞含量ng/L";
            //进样量ml
            newjylCol.Visibility = Visibility.Collapsed;

            /*--- 标样 ---*/
            //温度
            airtemperature.Visibility = Visibility.Collapsed;
            //标样体积ml
            airbulk.Visibility = Visibility.Collapsed;
            //汞量ng
            airhan.Visibility = Visibility.Visible;
            airhan.Header = "汞量ng";
            //样品质量
            standardzl.Visibility = Visibility.Collapsed;
            //汞浓度mg/m3
            standardgndCol.Visibility = Visibility.Visible;
            standardgndCol.Header = "汞浓度mg/m3";
            //汞流量mg/L
            standardllCol.Visibility = Visibility.Visible;
            standardllCol.Header = "汞流量mg/L";

            NewTargetDgd.CommitEdit();
            standardSampleDgd.CommitEdit();
        }

        //液体多量程测量
        private void liquidmultibox_itm_Click(object sender, RoutedEventArgs e)
        {
            testmoduleid = TestModule.LIQUID_MULTI_BULK;

            hotairbox_itm.Icon = null;
            airautomahead_itm.Icon = null;
            airautomback_itm.Icon = null;
            airadjustzeroahead_itm.Icon = null;
            liquidmultibox_itm.Icon = checkicon;
            liquidstandardbox_itm.Icon = null;

            /* -- 新样 --- */
            //取样时间
            newAirSampTimeCol.Visibility = Visibility.Collapsed;
            //流量l/m
            newAirFluentCol.Visibility = Visibility.Collapsed;
            //样品质量
            newqualityCol.Visibility = Visibility.Visible;
            newqualityCol.Header = "样品质量g";
            //样品总体积L
            newtotalbulkCol.Visibility = Visibility.Visible;
            newtotalbulkCol.Header = "样品消化液总体积mL";
            //样品中汞含量mg/m3
            newghltotalCol.Visibility = Visibility.Visible;
            newghltotalCol.Header = "样品中汞含量mg/kg";
            //汞浓度mg/m3
            newgndCol.Visibility = Visibility.Visible;
            newgndCol.Header = "汞浓度ug/L";
            //样品气体总流量L/min
            newypqtzllCol.Visibility = Visibility.Collapsed;
            //汞含量mg
            newghlCol.Visibility = Visibility.Collapsed;
            //进样量ml
            newjylCol.Visibility = Visibility.Visible;
            newjylCol.Header = "进样量mL";

            /*--- 标样 ---*/
            //温度
            airtemperature.Visibility = Visibility.Collapsed;
            //标样体积ml
            airbulk.Visibility = Visibility.Visible;
            airbulk.Header = "总体积mL";
            //汞量ng
            airhan.Visibility = Visibility.Collapsed;
            //样品质量
            standardzl.Visibility = Visibility.Collapsed;
            //汞浓度mg/m3
            standardgndCol.Visibility = Visibility.Visible;
            standardgndCol.Header = "汞浓度ug/L";
            //汞流量mg/L
            standardllCol.Visibility = Visibility.Collapsed;

            NewTargetDgd.CommitEdit();
            standardSampleDgd.CommitEdit();
        }

        //液体标量程测量
        private void liquidstandardbox_itm_Click(object sender, RoutedEventArgs e)
        {
            testmoduleid = TestModule.LIQUID_STANDARD_BULK;

            hotairbox_itm.Icon = null;
            airautomahead_itm.Icon = null;
            airautomback_itm.Icon = null;
            airadjustzeroahead_itm.Icon = null;
            liquidmultibox_itm.Icon = null;
            liquidstandardbox_itm.Icon = checkicon;

            /* -- 新样 --- */
            //取样时间
            newAirSampTimeCol.Visibility = Visibility.Collapsed;
            //流量l/m
            newAirFluentCol.Visibility = Visibility.Collapsed;
            //样品质量
            newqualityCol.Visibility = Visibility.Visible;
            newqualityCol.Header = "样品质量g";
            //样品总体积L
            newtotalbulkCol.Visibility = Visibility.Visible;
            newtotalbulkCol.Header = "样品消化液总体积mL";
            //样品中汞含量mg/m3
            newghltotalCol.Visibility = Visibility.Visible;
            newghltotalCol.Header = "样品中汞含量mg/kg";
            //汞浓度mg/m3
            newgndCol.Visibility = Visibility.Visible;
            newgndCol.Header = "汞浓度ug/L";
            //样品气体总流量L/min
            newypqtzllCol.Visibility = Visibility.Collapsed;
            //汞含量mg
            newghlCol.Visibility = Visibility.Collapsed;
            //进样量ml
            newjylCol.Visibility = Visibility.Collapsed;


            /*--- 标样 ---*/
            //温度
            airtemperature.Visibility = Visibility.Collapsed;
            //标样体积ml
            airbulk.Visibility = Visibility.Visible;
            airbulk.Header = "总体积mL";
            //汞量ng
            airhan.Visibility = Visibility.Collapsed;
            //样品质量
            standardzl.Visibility = Visibility.Collapsed;
            //汞浓度mg/m3
            standardgndCol.Visibility = Visibility.Visible;
            standardgndCol.Header = "汞浓度ug/L";
            //汞流量mg/L
            standardllCol.Visibility = Visibility.Collapsed;

            NewTargetDgd.CommitEdit();
            standardSampleDgd.CommitEdit();
        }

        private void standardSampleDgd_Unloaded(object sender, RoutedEventArgs e)
        {
//            var grid = (DataGrid)sender;
            //grid.CommitEdit(DataGridEditingUnit.Row, true);
        }

        private void StandardCmb_DropDownClosed(object sender, EventArgs e)
        {
            double a, b, r;
            int rowNo;
            int cltindex;

            try
            {
                ComboBox cb = sender as ComboBox;
                int i = cb.SelectedIndex;
                StandardSample ss = cb.SelectedItem as StandardSample;

                rowNo = NewTargetDgd.SelectedIndex;
                cltindex = getNewCltIndex(rowNo);
                if (cltindex == -1) return;
                if (rowNo < 0)
                    return;
                if (newTestClt[cltindex].ResponseValue1 == "" ||
                    newTestClt[cltindex].ResponseValue1 == null ||
                    ss.A is null ||
                    ss.B is null ||
                    ss.R is null
                    )
                    return;

                if (newTestClt[cltindex].AirFluent == "" || newTestClt[cltindex].AirFluent is null ||
                    newTestClt[cltindex].AirSampleTime == "" || newTestClt[cltindex].AirSampleTime is null)
                {
                    return;
                }

                aTxb.Text = ss.A;
                bTxb.Text = ss.B;
                rTxt.Text = ss.R;

                a = double.Parse(ss.A);
                b = double.Parse(ss.B);
                r = double.Parse(ss.R);
            }
            catch
            {
                return;
            }

            //如果有平均值则计算汞浓度
            newTestClt[cltindex].AirTotolBulk =
                (Math.Round(double.Parse(newTestClt[cltindex].AirFluent) * double.Parse(newTestClt[cltindex].AirSampleTime),
                    2)).ToString();
            //y-b/a
            newTestClt[cltindex].AirG =
                Math.Round(0.001 * (double.Parse(newTestClt[cltindex].ResponseValue1) - b) / a, 5).ToString();
        }

        private void NewTargetDgd_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //正在测试，不能切换波形
            if (this.teststatus == TestingStatus.TESTING)
                return;
            //测试停止，离开测试条目之前，cache波形数据
            if (this.teststatus == TestingStatus.STOPPED)
            {
                //从测试条目离开，保存测试波形
                NewTestTarget aold;
                if (e.RemovedItems.Count == 0)
                    aold = null;
                else
                    aold = e.RemovedItems[0] as NewTestTarget;
                Collection<ADot> dots = DotManager.GetDotManger().GetDots();
                if (aold != null && dots.Count != 0 && aold.GlobalID == testing_gid)
                {
                    TableCache.GetTableCache().CacheWave(getTypeOfTestingItem(testing_gid), testing_gid, DotManager.GetDotManger().GetDots());
                }
                else
                {
                    //失去焦点情况下，默认上次是选中的是真正测试的条目
                    if (aold == null && dots.Count != 0)
                    {
                        TableCache.GetTableCache().CacheWave(getTypeOfTestingItem(testing_gid), testing_gid, DotManager.GetDotManger().GetDots());
                    }
                }
                //加载波形
                if (e.AddedItems.Count == 0)
                    return;
                string temp_gid = (e.AddedItems[0] as NewTestTarget).GlobalID;
                Collection<ADot> wave = TableCache.GetTableCache().FindWave(getTypeOfTestingItem(temp_gid), temp_gid);
                if (wave == null) return;
                waveinfo_txt.Text = "样本 " + temp_gid;
                realCpt.SetScale(100, 2000, 0, 50);
                realCpt.SetNumberOfDValueP(10000);
                realCpt.ClearData();
                for (int i = 0; i < wave.Count; i++)
                {
                    realCpt.AddPoint(new Point(i, wave[i].Rvalue));
                }
            }
        }
    }
}
