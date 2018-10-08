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
        private readonly int NumberOfDValue = 100000;

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
        #endregion

        public BasicWaveChartUC()
        {
            InitializeComponent();
        }

        #region self event handler

        private void optimizeCanvas_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void ControlContainer_Loaded(object sender, RoutedEventArgs e)
        {
            /*
            optimizeCanvas.Width = xaxis.Width - yaxis.Width - this.RightBlankZone - xaxis.XArrowheight;
            optimizeCanvas.Height = yaxis.Height - xaxis.Height - this.TopBlankZone - yaxis.YArrowheight;
            */
            optimizeCanvas.Width = xaxis.GetGranulity() * NumberOfDValue;
            optimizeCanvas.Height = yaxis.Height - xaxis.Height - this.TopBlankZone - yaxis.YArrowheight;

            WindowCanvas.Width = xaxis.Width - yaxis.Width - this.RightBlankZone - xaxis.XArrowheight;
            WindowCanvas.Height = yaxis.Height - xaxis.Height - this.TopBlankZone - yaxis.YArrowheight;
            ;
            this.OnScaleChanged(optimizeCanvas.ScaleChangedHdlr);
        }

        private void xaxis_text_canvas_Unloaded(object sender, RoutedEventArgs e)
        {
            ;
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
            int loop = (int)(xaxis.XScaleMaxValue / xaxis.XScaleLineNumber / xaxis.XCommentNumber);

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
        }

        private void xaxis_text_canvas_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        #endregion

        #region public function

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

        #region event define
        public delegate void ScaleChangeDelegate();
        ScaleChangeDelegate ScaleChanged_Ev;
        public void OnScaleChanged(ScaleChangeDelegate ScaleChangedHdr)
        {
            ScaleChanged_Ev += ScaleChangedHdr;
        }
        #endregion
    }
}
