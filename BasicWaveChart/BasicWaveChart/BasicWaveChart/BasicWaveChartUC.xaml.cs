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

namespace BasicWaveChart
{
    /// <summary>
    /// BasicWaveChartUC.xaml 的交互逻辑
    /// </summary>
    public partial class BasicWaveChartUC : UserControl
    {
        //private readonly int NumberOfDValue = 100000;
        private int numberOfDValue;
        public int NumberOfDValue
        {
            get
            {
                return numberOfDValue;
            }
            set
            {
                numberOfDValue = value;
                try //try to refresh the xaxis and xaxis_comment_cannvas
                {
                    XAxisCtl xa = xaxis;
                    basecanvas.Children.Remove(xaxis);
                    basecanvas.Children.Add(xa);
                    xaxis.ReDrawTextCommentCmd();
                    moveslider.Minimum = -(xa.GetGranulity() * this.NumberOfDValue - xmark.lineinfo.observeWinWidth);
                }
                catch
                {
                    Console.WriteLine("ignore error: refresh xaxis fail");
                }
            }
        }

        #region DependencyProperty
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
        DependencyProperty RatioSProperty = DependencyProperty.Register("RatioS", typeof(string),typeof(BasicWaveChartUC),
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
        DependencyProperty TopBlankZoneProperty = DependencyProperty.Register("TopBlankZone", typeof(int), typeof(BasicWaveChartUC),
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
        DependencyProperty RightBlankZoneProperty = DependencyProperty.Register("RightBlankZone", typeof(int), typeof(BasicWaveChartUC),
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
        DependencyProperty MoveModeProperty = DependencyProperty.Register("MoveMode", typeof(WaveMoveMode), typeof(BasicWaveChartUC),
            new UIPropertyMetadata(WaveMoveMode.HORIZONTAL));

        #endregion

        public BasicWaveChartUC()
        {
            NumberOfDValue = 1187;
            InitializeComponent();
        }

        #region self event handler

        private void ControlContainer_Loaded(object sender, RoutedEventArgs e)
        {
            //init marker
            xmark.lineinfo.RightBlankZone = this.RightBlankZone;
            xmark.lineinfo.arrowheight = 10;
            xmark.lineinfo.observeWinWidth = basecanvas.ActualWidth - yaxis.Width - xmark.lineinfo.arrowheight
                - this.RightBlankZone;
            xmark.lineinfo.ostart = yaxis.Width;

            optimizeCanvas.Height = yaxis.Height - xaxis.Height - this.TopBlankZone - yaxis.YArrowheight;
            int temp = xaxis.XScaleMaxValue;
            xaxis.XScaleMaxValue = 1;
            xaxis.XScaleMaxValue = temp;
            optimizeCanvas.Width = this.NumberOfDValue * xaxis.GetGranulity();

            WindowCanvas.Width = xaxis.Width - yaxis.Width - this.RightBlankZone - xaxis.XArrowheight;
            WindowCanvas.Height = yaxis.Height - xaxis.Height - this.TopBlankZone - yaxis.YArrowheight;

            moveslider.Minimum = -(xaxis.GetGranulity()*this.numberOfDValue - WindowCanvas.Width);

            ContextMenu wavemenu = this.FindResource("wavemenu") as ContextMenu;
            wavemenu.PlacementTarget = optimizeCanvas;

            //refresh xaxis
            XAxisCtl xa = xaxis;
            basecanvas.Children.Remove(xaxis);
            basecanvas.Children.Add(xa);

            this.OnScaleChanged(optimizeCanvas.ScaleChangedHdlr);

        }

        private void xaxis_text_canvas_Unloaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void xaxis_text_canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //add the scale text

            //0
            xaxis_text_canvas.Children.Clear();
            xaxis_text_canvas.Children.Add(new TextBlock());
            (xaxis_text_canvas.Children[xaxis_text_canvas.Children.Count - 1] as TextBlock).Text = "0";
            (xaxis_text_canvas.Children[xaxis_text_canvas.Children.Count - 1] as TextBlock).FontSize = 8;
            Canvas.SetLeft((xaxis_text_canvas.Children[xaxis_text_canvas.Children.Count - 1] as TextBlock), yaxis.Width);
            Canvas.SetBottom((xaxis_text_canvas.Children[xaxis_text_canvas.Children.Count - 1] as TextBlock), 0);
            int loop = (int)(this.NumberOfDValue / xaxis.XScaleLineNumber / xaxis.XCommentNumber); //todo relace 1187

            for (int i = 1; i < loop; i++)
            {
                xaxis_text_canvas.Children.Add(new TextBlock());
                (xaxis_text_canvas.Children[xaxis_text_canvas.Children.Count - 1] as TextBlock).Text =
                    (i * xaxis.XScaleLineNumber * xaxis.XCommentNumber).ToString();
                (xaxis_text_canvas.Children[xaxis_text_canvas.Children.Count - 1] as TextBlock).FontSize = 8;
                Canvas.SetLeft((xaxis_text_canvas.Children[xaxis_text_canvas.Children.Count - 1] as TextBlock), (i * xaxis.XScaleLineNumber * xaxis.XCommentNumber) * xaxis.GetGranulity() + yaxis.Width);
                Canvas.SetBottom((xaxis_text_canvas.Children[xaxis_text_canvas.Children.Count - 1] as TextBlock), 0);
            }

            //the text of last big scale
            xaxis_text_canvas.Children.Add(new TextBlock());
            (xaxis_text_canvas.Children[xaxis_text_canvas.Children.Count - 1] as TextBlock).Text =
                (loop * xaxis.XScaleLineNumber * xaxis.XCommentNumber).ToString();
            (xaxis_text_canvas.Children[xaxis_text_canvas.Children.Count - 1] as TextBlock).FontSize = 8;
            Canvas.SetLeft((xaxis_text_canvas.Children[xaxis_text_canvas.Children.Count - 1] as TextBlock), (loop * xaxis.XScaleLineNumber * xaxis.XCommentNumber) * xaxis.GetGranulity() + yaxis.Width);
            Canvas.SetBottom((xaxis_text_canvas.Children[xaxis_text_canvas.Children.Count - 1] as TextBlock), 0);

            //the max of dvalue
            xaxis_text_canvas.Children.Add(new TextBlock());
            (xaxis_text_canvas.Children[xaxis_text_canvas.Children.Count - 1] as TextBlock).Text =
                this.NumberOfDValue.ToString();  //todo replace 1187
            (xaxis_text_canvas.Children[xaxis_text_canvas.Children.Count - 1] as TextBlock).FontSize = 8;
            Canvas.SetLeft((xaxis_text_canvas.Children[xaxis_text_canvas.Children.Count - 1] as TextBlock), this.NumberOfDValue * xaxis.GetGranulity() + yaxis.Width); //todo replace the 1187
            Canvas.SetBottom((xaxis_text_canvas.Children[xaxis_text_canvas.Children.Count - 1] as TextBlock), 0);

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
            //Canvas.SetBottom((yaxis_text_canvas.Children[yaxis_text_canvas.Children.Count - 1] as TextBlock), xaxis.Height + yaxis.YScaleMaxValue * yaxis.GetGranulity()-20);
            Canvas.SetBottom((yaxis_text_canvas.Children[yaxis_text_canvas.Children.Count - 1] as TextBlock), xaxis.Height + yaxis.YScaleMaxValue*yaxis.GetGranulity());
        }

        private void xaxis_text_canvas_Loaded(object sender, RoutedEventArgs e)
        {
            xaxis_text_canvas.Width = xaxis.GetGranulity() * this.NumberOfDValue + yaxis.Width ;  //todo:replace 1187
        }

        private void optimizeCanvas_Loaded_1(object sender, RoutedEventArgs e)
        {

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
            Console.WriteLine("old:" + e.OldValue);
            Console.WriteLine("new:" + e.NewValue);
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

        #endregion

        #region public function

        //
        public PointCollection GetDValues()
        {
            return optimizeCanvas.GetDValues();
        }

        public PointCollection GetDatas()
        {
            return optimizeCanvas.GetDatas();
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
                //only this method to redraw xaxis after property change
                basecanvas.Children.Remove(xaxis);
                basecanvas.Children.Add(xaxis);
                xaxis.ReDrawTextCommentCmd();
            }

            if(yaxis.YScaleLineNumber != oldYScaleLineNumber || yaxis.YScaleMaxValue != oldYScaleMaxValue)
            {
                basecanvas.Children.Remove(yaxis);
                basecanvas.Children.Add(yaxis);
                yaxis.ReDrawCmd();
            }

        }
        #endregion

        //add point to draw
        public void AddPoint(Point dvalue)
        {
            optimizeCanvas.AddPoint(dvalue);
        }

        #region event define
        public delegate void ScaleChangeDelegate();
        ScaleChangeDelegate ScaleChanged_Ev;
        public void OnScaleChanged(ScaleChangeDelegate ScaleChangedHdr)
        {
            ScaleChanged_Ev += ScaleChangedHdr;
        }

        #endregion

        private void ControlContainer_Initialized(object sender, EventArgs e)
        {

        }

        private void integralmenu_Click(object sender, RoutedEventArgs e)
        {
            //cann't find the twohandle through FindName, maybe need register
            //
            dynamic bigbrother = null;
            foreach (dynamic obj in WindowCanvas.Children)
            {
                if(obj.Name == "twohandle")
                {
                    //
                    bigbrother = obj;
                    //showbigbrother(obj);
                    break;
                }
            }
            if( (sender as MenuItem).Header.ToString() == "积分")
            {
                showbigbrother(bigbrother);
                (sender as MenuItem).Header = "关闭积分";
            }
            else
            {
                closebigbrother(bigbrother);
                (sender as MenuItem).Header = "积分";
            }

        }

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

        private void WindowCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            xmark.lineinfo.RightBlankZone = this.RightBlankZone;
        }
    }
}
