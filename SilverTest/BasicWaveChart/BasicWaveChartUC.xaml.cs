using BasicWaveChart.Feature.integral;
using BasicWaveChart.widget;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static BasicWaveChart.Feature.integral.IntegralWorker;

namespace BasicWaveChart
{
    /// <summary>
    /// BasicWaveChartUC.xaml 的交互逻辑
    /// </summary>
    public partial class BasicWaveChartUC : UserControl
    {
        #region DependencyProperty
        public int NumberOfDValue
        {
            get
            {
                return (int)GetValue(NumberOfDValueProperty);
            }
            set
            {
                SetValue(NumberOfDValueProperty, value);
            }
        }
        public static readonly DependencyProperty NumberOfDValueProperty = DependencyProperty.Register("NumberOfDValue", typeof(int), typeof(BasicWaveChartUC),
            new UIPropertyMetadata(1000));

        public void SetNumberOfDValueP(int n)
        {
            NumberOfDValue = n;

            //refresh the xaxis and xaxis_comment_cannvas
            BasicWaveChartUC wavechart = this;
            XAxisCtl xa = wavechart.xaxis;
            xa.ReDrawTextCommentCmd();
            xa.ReDrawScaleCmd();
            wavechart.moveslider.Minimum = -(xa.GetGranulity() * wavechart.NumberOfDValue - wavechart.WindowCanvas_pen.Width);
            wavechart.optimizeCanvas.Width = xa.GetGranulity() * wavechart.NumberOfDValue;
        }

        public string RatioS
        {
            get
            {
                return (string)GetValue(RatioSProperty);
            }
            set
            {
                SetValue(RatioSProperty, value);
            }
        }
        public static readonly DependencyProperty RatioSProperty = DependencyProperty.Register("RatioS", typeof(string),typeof(BasicWaveChartUC),
            new UIPropertyMetadata("1:1", new PropertyChangedCallback(RatioSChanged)),
                new ValidateValueCallback(RatioSIsNumber)
        );

        private static bool RatioSIsNumber(object value)
        {
            return true;
            
        }

        private static void RatioSChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            return;
            
        }

        //the blank area on top of y axis
        public int TopBlankZone
        {
            get
            {
                return (int)GetValue(TopBlankZoneProperty);
            }
            set
            {
                SetValue(TopBlankZoneProperty, value);
            }
        }
        public static readonly DependencyProperty TopBlankZoneProperty = DependencyProperty.Register("TopBlankZone", typeof(int), typeof(BasicWaveChartUC),
            new UIPropertyMetadata(0, new PropertyChangedCallback(TopBlankZone_Changed)));

        private static void TopBlankZone_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            return;
            
        }

        //the blank area on right of x axis
        public int RightBlankZone
        {
            get
            {
                return (int)GetValue(RightBlankZoneProperty);
            }
            set
            {
                SetValue(RightBlankZoneProperty, value);
            }
        }
        public static readonly DependencyProperty RightBlankZoneProperty = DependencyProperty.Register("RightBlankZone", typeof(int), typeof(BasicWaveChartUC),
            new UIPropertyMetadata(0, new PropertyChangedCallback(RightBlankZone_Changed)));
        private static void RightBlankZone_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            return;
            
        }

        //how to draw the wave , packed or horizontal move
        public WaveMoveMode MoveMode
        {
            get
            {
                return (WaveMoveMode)GetValue(MoveModeProperty);
            }
            set
            {
                SetValue(MoveModeProperty, value);
            }
        }
        public static readonly DependencyProperty MoveModeProperty = DependencyProperty.Register("MoveMode", typeof(WaveMoveMode), typeof(BasicWaveChartUC),
            new UIPropertyMetadata(WaveMoveMode.HORIZONTAL));

        #endregion

        public BasicWaveChartUC()
        {
            InitializeComponent();
        }

        #region event handler

        private void ControlContainer_Loaded(object sender, RoutedEventArgs e)
        {
            //refresh xaxis
            WindowCanvas_pen.Width = xaxis_pen.Width - yaxis_pen.Width - this.RightBlankZone - xaxis.XArrowheight;
            WindowCanvas_pen.Height = yaxis_pen.Height - xaxis_pen.Height - this.TopBlankZone - yaxis.YArrowheight;

            xaxis.Width = basecanvas.ActualWidth - yaxis_pen.Width;
            optimizeCanvas.Height = WindowCanvas_pen.Height;
            optimizeCanvas.Width = this.NumberOfDValue * xaxis.GetGranulity();
            xaxis_ply.Width = this.NumberOfDValue * xaxis.GetGranulity();
            xaxis_text_canvas.Width = xaxis.GetGranulity() * this.NumberOfDValue + yaxis.Width;

            xaxis.ReDrawScaleCmd();
            xaxis.ReDrawTextCommentCmd();
            
            try
            {
                moveslider.Minimum = -(xaxis.GetGranulity() * this.NumberOfDValue - WindowCanvas_pen.Width);
            }catch(Exception ex)
            {
                return;
            }

            ContextMenu wavemenu = this.FindResource("wavemenu") as ContextMenu;
            wavemenu.PlacementTarget = optimizeCanvas;

            this.OnScaleChanged(optimizeCanvas.ScaleChangedHdlr);

        }

  
        private void yaxis_text_canvas_Loaded(object sender, RoutedEventArgs e)
        {
          
            //add the scale text
            yaxis_text_canvas.Children.Clear();
            //0
            yaxis_text_canvas.Children.Add(new TextBlock());
            (yaxis_text_canvas.Children[yaxis_text_canvas.Children.Count - 1] as TextBlock).Text = "0";
            (yaxis_text_canvas.Children[yaxis_text_canvas.Children.Count - 1] as TextBlock).FontSize = 8;
            Canvas.SetLeft((yaxis_text_canvas.Children[yaxis_text_canvas.Children.Count - 1] as TextBlock),0);
            Canvas.SetBottom((yaxis_text_canvas.Children[yaxis_text_canvas.Children.Count - 1] as TextBlock), xaxis.Height);
            int loop = (int)(yaxis.YScaleMaxValue / yaxis.YScaleLineNumber / yaxis.YCommentNumber);

            for (int i = 1; i < loop; i++)
            {
                yaxis_text_canvas.Children.Add(new TextBlock());
                (yaxis_text_canvas.Children[yaxis_text_canvas.Children.Count - 1] as TextBlock).Text =
                    (i * yaxis.YScaleLineNumber * yaxis.YCommentNumber).ToString();
                (yaxis_text_canvas.Children[yaxis_text_canvas.Children.Count - 1] as TextBlock).FontSize = 8;
                Canvas.SetLeft((yaxis_text_canvas.Children[yaxis_text_canvas.Children.Count - 1] as TextBlock), 0);
                Canvas.SetBottom((yaxis_text_canvas.Children[yaxis_text_canvas.Children.Count - 1] as TextBlock), xaxis.Height + i*yaxis.YScaleLineNumber*yaxis.YCommentNumber*yaxis.GetGranulity());
            }

            //the text of last big scale
            yaxis_text_canvas.Children.Add(new TextBlock());
            (yaxis_text_canvas.Children[yaxis_text_canvas.Children.Count - 1] as TextBlock).Text =
                (loop * yaxis.YScaleLineNumber * yaxis.YCommentNumber).ToString();
            (yaxis_text_canvas.Children[yaxis_text_canvas.Children.Count - 1] as TextBlock).FontSize = 8;
            Canvas.SetLeft((yaxis_text_canvas.Children[yaxis_text_canvas.Children.Count - 1] as TextBlock), 0);
            Canvas.SetBottom((yaxis_text_canvas.Children[yaxis_text_canvas.Children.Count - 1] as TextBlock), xaxis.Height + loop * yaxis.YScaleLineNumber * yaxis.YCommentNumber * yaxis.GetGranulity());

            //the max of value
            yaxis_text_canvas.Children.Add(new TextBlock());
            (yaxis_text_canvas.Children[yaxis_text_canvas.Children.Count - 1] as TextBlock).Text =
                yaxis.YScaleMaxValue.ToString();
            (yaxis_text_canvas.Children[yaxis_text_canvas.Children.Count - 1] as TextBlock).FontSize = 8;
            Canvas.SetLeft((yaxis_text_canvas.Children[yaxis_text_canvas.Children.Count - 1] as TextBlock), 0);
            Canvas.SetBottom((yaxis_text_canvas.Children[yaxis_text_canvas.Children.Count - 1] as TextBlock), xaxis.Height + yaxis.YScaleMaxValue*yaxis.GetGranulity());
        }

        private void optimizeCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ContextMenu wavemenu = this.FindResource("wavemenu") as ContextMenu;
            wavemenu.IsOpen = true;
        }

        private void fullshowmenu_Click(object sender, RoutedEventArgs e)
        {
            optimizeCanvas.ShowFullView();
        }

        private void moveslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //Console.WriteLine("old:=" + e.OldValue);
            //Console.WriteLine("new:=" + e.NewValue);
            ;
        }

        private void movemenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menu = sender as MenuItem;
            if (menu.Header.ToString() == "移动")
            {
                moveslider.Visibility = Visibility.Visible;
                menu.Header = "关闭移动";
            }
            else
            {
                moveslider.Visibility = Visibility.Hidden;
                menu.Header = "移动";
            }
        }

        private void ControlContainer_Initialized(object sender, EventArgs e)
        {

        }

        private void integralmenu_Click(object sender, RoutedEventArgs e)
        {
            //cann't find the twohandle through FindName, maybe need register
            //
            MenuItem mitem = sender as MenuItem;

            dynamic bigbrother = null;
            foreach (dynamic obj in WindowCanvas.Children)
            {
                if (obj.Name == "twohandle")
                {
                    //
                    bigbrother = obj;
                    //showbigbrother(obj);
                    break;
                }
            }

            if (bigbrother == null)
                return;
            if ((sender as MenuItem).Header.ToString() == "积分")
            {
                showbigbrother(bigbrother);
                foreach (dynamic item in optimizeCanvas.Children)
                {
                    if (item is Polygon)
                    {
                        (item as Polygon).Visibility = Visibility.Visible;
                    }
                }
                (sender as MenuItem).Header = "关闭积分";
            }
            else
            {
                closebigbrother(bigbrother);
                foreach (dynamic item in optimizeCanvas.Children)
                {
                    if (item is Polygon)
                    {
                        (item as Polygon).Visibility = Visibility.Hidden;
                    }
                }
                (sender as MenuItem).Header = "积分";
            }

        }

        #endregion

        #region public function
        public void RegisterIntegrateFunc(IntegrateAreaDelegate func)
        {
            IntegralWorker.myIntegrateArea = func;
        }


        public PointCollection GetDValues()
        {
            return optimizeCanvas.GetDValues();
        }

        public PointCollection GetDatas()
        {
            return optimizeCanvas.GetDatas();
        }

        public void ClearData()
        {
            optimizeCanvas.ClearData();
            moveslider.Value = 0;
        }

        //set the value of x, y axis
        public void SetScale(int xscaleLineNumber, int xscaleMaxValue, int yscaleLineNumber, int yscaleMaxValue)
        {
            if (xscaleLineNumber == 0)
                xscaleLineNumber = xaxis.XScaleLineNumber;
            if (xscaleMaxValue == 0)
                xscaleMaxValue = xaxis.XScaleMaxValue;
            if (yscaleLineNumber == 0)
                yscaleLineNumber = yaxis.YScaleLineNumber;
            if (yscaleMaxValue == 0)
                yscaleMaxValue = yaxis.YScaleMaxValue;

            double oldXScaleLineNumber = xaxis.XScaleLineNumber;
            double oldXScaleMaxValue = xaxis.XScaleMaxValue;
            double oldYScaleLineNumber = yaxis.YScaleLineNumber;
            double oldYScaleMaxValue = yaxis.YScaleMaxValue;

            xaxis.XScaleLineNumber = xscaleLineNumber;
            xaxis.XScaleMaxValue = xscaleMaxValue;
            yaxis.YScaleLineNumber = yscaleLineNumber;
            yaxis.YScaleMaxValue = yscaleMaxValue;

            if (oldXScaleMaxValue != xaxis.XScaleMaxValue || oldYScaleMaxValue != yaxis.YScaleMaxValue)
            {
                //notify the optimizecanvas to redraw
                
                if (ScaleChanged_Ev != null)
                    ScaleChanged_Ev();
            }

            if(xaxis.XScaleLineNumber != oldXScaleLineNumber || xaxis.XScaleMaxValue != oldXScaleMaxValue)
            {
                //basecanvas.Children.Remove(xaxis);
                //basecanvas.Children.Add(xaxis);
                xaxis.ReDrawTextCommentCmd();
            }

            if(yaxis.YScaleLineNumber != oldYScaleLineNumber || yaxis.YScaleMaxValue != oldYScaleMaxValue)
            {
                basecanvas.Children.Remove(yaxis);
                basecanvas.Children.Add(yaxis);
                yaxis.ReDrawCmd();
            }

        }

        //add point to draw
        // @ dvalue - 主通道值
        //   vice_dvalue - 副通道值
        public void AddPoint(Object dvalue,Object vice_dvalue)
        {
            optimizeCanvas.AddPoint(dvalue,vice_dvalue);
        }
        #endregion

        #region event define
        public delegate void ScaleChangeDelegate();
        ScaleChangeDelegate ScaleChanged_Ev;
        public void OnScaleChanged(ScaleChangeDelegate ScaleChangedHdr)
        {
            ScaleChanged_Ev += ScaleChangedHdr;
        }
        public delegate void IntegrateValueChangeDelegate(double res);
        event IntegrateValueChangeDelegate IntegrateValueChange_Ev;
        public void OnIntegrateValueChange(IntegrateValueChangeDelegate hdlr)
        {
            IntegrateValueChange_Ev += hdlr;
        }
        public void TriggerIntegrateValueChangeEv(double res)
        {
            if (IntegrateValueChange_Ev != null)
                IntegrateValueChange_Ev(res);
        }

        #endregion

        private void closebigbrother(dynamic bigbrother)
        {
            bigbrother.Visibility = Visibility.Hidden;
            bigbrother.GetBrother().Visibility = Visibility.Hidden;
            //hide comment
            foreach (dynamic cm in WindowCanvas.Children)
            {
                if (cm.Name == "commenttx")
                {
                    //
                    cm.Visibility = Visibility.Hidden;
                    break;
                }
            }
        }

        private void showbigbrother(dynamic obj)
        {
            obj.Visibility = Visibility.Visible;
            obj.GetBrother().Visibility = Visibility.Visible;
            //show comment
            foreach (dynamic cm in WindowCanvas.Children)
            {
                if (cm.Name == "commenttx")
                {
                    //
                    cm.Visibility = Visibility.Visible;
                    break;
                }
            }
        }

        private void xaxis_Loaded(object sender, RoutedEventArgs e)
        {
            ;
        }

    }
}
