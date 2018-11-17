using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BasicWaveChart.widget
{
    /*
     * say together.
     * dot - a real dot in screen
     * dvalue - value of data, the min value is "1" . if it's 0.01, 0.002,0.003, should first convert to 1,2,3
     */

    class XAxisCtl : Shape
    {

        protected double granulity_width = 1;               //the width of expected granulity
        private double littleScaleHeight = 6;               //height of little scale
        private double bigScaleHeight = 10;                 //height of big scale
        private double axisThickness = 3;                   //the thickness of axis 
        private double littleScaleThickness = 3;            //thickness of little scale
        private double bigScaleThickness = 3;               //thickness of big scale
        private double topblank = 0;                        //top magin above axis
        private double arrowheight = 10;                     //size of arrow
        private double windowwidth = 0;                     //the width to show and to compute granulity_width,
                                                            //

        #region DependencyProperty
        //height of arrow
        public double XArrowheight
        {
            get
            {
                return (double)GetValue(XArrowheightProperty);
            }
            set
            {
                SetValue(XArrowheightProperty, value);
            }
        }
        public static readonly DependencyProperty XArrowheightProperty = DependencyProperty.Register("XArrowheight", typeof(double), 
            typeof(XAxisCtl));

        //max dvalue to draw
        public int XScaleMaxValue
        {
            get {
                return (int)GetValue(XScaleMaxValueProperty);
            }
            set {
                SetValue(XScaleMaxValueProperty, value);
            }
        }

        public static readonly DependencyProperty XScaleMaxValueProperty = DependencyProperty.Register("XScaleMaxValue", typeof(int), typeof(XAxisCtl),
            new UIPropertyMetadata(680,new PropertyChangedCallback(XScaleMaxValue_Changed)));

        private static void XScaleMaxValue_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XAxisCtl self = d as XAxisCtl;
            YAxisCtl yaxisctl = self.FindName("yaxis") as YAxisCtl;
            BasicWaveChartUC wavechartuc = self.FindName("ControlContainer") as BasicWaveChartUC;
            if (self == null || yaxisctl == null ) return;

            try
            {
                self.granulity_width = wavechartuc.WindowCanvas_pen.Width / self.XScaleMaxValue;
            }
            catch
            {
                return;  //try , ignore error;
            }
        }

        //how many dvalue that a scale include
        public int XScaleLineNumber
        {
            get {
                return (int)(GetValue(XScaleLineNumberProperty));
            }
            set
            {
                SetValue(XScaleLineNumberProperty, value);
            }
        }
        
        public static readonly DependencyProperty XScaleLineNumberProperty = DependencyProperty.Register("XScaleLineNumber", typeof(int), typeof(XAxisCtl));

        //to comment text after how many XScaleLineNumber
        public int XCommentNumber
        {
            get
            {
                return (int)GetValue(XCommentNumberProperty);
            }
            set
            {
                SetValue(XCommentNumberProperty, value);
            }
        }

        public static readonly DependencyProperty XCommentNumberProperty = DependencyProperty.Register("XCommentNumber", typeof(int), typeof(XAxisCtl));

        //arrow style
        public ArrowStyleEnm XArrowStyle
        {
            get
            {
                return (ArrowStyleEnm)GetValue(XArrowStyleProperty);
            }
            set
            {
                SetValue(XArrowStyleProperty, value);
            }
        }

        public static readonly DependencyProperty XArrowStyleProperty = DependencyProperty.Register("XArrowStyle", typeof(ArrowStyleEnm), typeof(XAxisCtl));
        #endregion
        
        protected override Geometry DefiningGeometry
        {
            get
            {
                /*
                this.Stroke = new SolidColorBrush(Colors.Black);
                this.StrokeThickness = 1;
                */

                YAxisCtl yaxisctl = this.FindName("yaxis") as YAxisCtl;
                

                BasicWaveChartUC wavechartuc = this.FindName("ControlContainer") as BasicWaveChartUC;
                Canvas windowcanvas = wavechartuc.FindName("WindowCanvas") as Canvas;

                PathGeometry pg = new PathGeometry();

                PathFigure pf = new PathFigure();
                pg.Figures.Add(pf);
                PolyLineSegment ScaleSeg = new PolyLineSegment();

                if (yaxisctl is null || wavechartuc is null)
                {
                    this.granulity_width = 1;
                    return new PathGeometry();
                }
                try
                {
                    this.windowwidth = wavechartuc.WindowCanvas_pen.Width;
                }
                catch
                {
                    ScaleSeg.Points.Add(new Point(0,0));
                    return pg;
                }
                this.granulity_width =  this.windowwidth / this.XScaleMaxValue;
                this.Width = this.granulity_width * wavechartuc.NumberOfDValue + this.arrowheight + yaxisctl.Width; //todo relace with numberofdvalues

                double hlinevalue = this.Height - topblank;
                
                if (this.XScaleLineNumber == 0) this.XScaleLineNumber = 100;
                int scalenumber = (int)(wavechartuc.NumberOfDValue / this.XScaleLineNumber); //todo relace 1187
                for (int i = 0; i<= scalenumber; i++)
                {
                    ScaleSeg.Points.Add(new Point(i*XScaleLineNumber*granulity_width + yaxisctl.Width,hlinevalue));
                    if (i % this.XCommentNumber == 0)
                    {
                        ScaleSeg.Points.Add(new Point(i * XScaleLineNumber * granulity_width + yaxisctl.Width, hlinevalue - bigScaleHeight));
                    }
                    else
                    {
                        ScaleSeg.Points.Add(new Point(i * XScaleLineNumber * granulity_width + yaxisctl.Width, hlinevalue - littleScaleHeight));
                    }
                    ScaleSeg.Points.Add(new Point(i * XScaleLineNumber * granulity_width + yaxisctl.Width, hlinevalue));
                }

                ScaleSeg.Points.Add(new Point(wavechartuc.NumberOfDValue * granulity_width + yaxisctl.Width, hlinevalue)); //todo replace
                ScaleSeg.Points.Add(new Point(wavechartuc.NumberOfDValue * granulity_width + yaxisctl.Width, hlinevalue - 20));  //todo replace
                ScaleSeg.Points.Add(new Point(wavechartuc.NumberOfDValue * granulity_width + yaxisctl.Width, hlinevalue)); //todo replace

                pf.Segments.Add(ScaleSeg);
                PolyLineSegment arrowseg = new PolyLineSegment();

                switch (this.XArrowStyle)
                {
                    case ArrowStyleEnm.SOLID:
                    case ArrowStyleEnm.HOLLOW:
                        {

                            arrowseg.Points.Add(new Point(this.Width - wavechartuc.RightBlankZone - this.arrowheight, hlinevalue));
                            arrowseg.Points.Add(new Point(this.Width - wavechartuc.RightBlankZone - this.arrowheight, hlinevalue - this.arrowheight / 2));
                            arrowseg.Points.Add(new Point(this.Width - wavechartuc.RightBlankZone - this.arrowheight, hlinevalue + this.arrowheight / 2));
                            arrowseg.Points.Add(new Point(this.Width - wavechartuc.RightBlankZone, hlinevalue));
                            arrowseg.Points.Add(new Point(this.Width - wavechartuc.RightBlankZone - this.arrowheight, hlinevalue - this.arrowheight / 2));
                            pf.IsFilled = true;
                            break;
                        }
                    case ArrowStyleEnm.NONE:
                    default:
                        {
                            arrowseg.Points.Add(new Point(this.Width - wavechartuc.RightBlankZone, hlinevalue));
                            break;
                        }
                }
                pf.Segments.Add(arrowseg);

                return pg;
            }
        }

        public double GetGranulity()
        {
            return granulity_width;
        }

        // get the drawing value in y axis according to dvalue
        public double GetXX(int dvalue)
        {
            return dvalue * granulity_width;
        }

        //get the index of dvalues according the point x
        public int GetDValueIndex(double x)
        {
            return (int)(x / granulity_width);
        }

        //redraw command
        public void ReDrawTextCommentCmd()
        {

            Canvas xaxis_text_canvas = this.FindName("xaxis_text_canvas") as Canvas;
            BasicWaveChartUC wavechartuc = this.FindName("ControlContainer") as BasicWaveChartUC;
            XAxisCtl xaxis = this.FindName("xaxis") as XAxisCtl;
            YAxisCtl yaxis = this.FindName("yaxis") as YAxisCtl;

            //add the scale text
            xaxis_text_canvas.Children.Clear();

            //0
            xaxis_text_canvas.Children.Add(new TextBlock());
            (xaxis_text_canvas.Children[xaxis_text_canvas.Children.Count - 1] as TextBlock).Text = "0";
            (xaxis_text_canvas.Children[xaxis_text_canvas.Children.Count - 1] as TextBlock).FontSize = 8;
            Canvas.SetLeft((xaxis_text_canvas.Children[xaxis_text_canvas.Children.Count - 1] as TextBlock), yaxis.Width);
            Canvas.SetBottom((xaxis_text_canvas.Children[xaxis_text_canvas.Children.Count - 1] as TextBlock), 0);
            int loop = (int)(wavechartuc.NumberOfDValue / xaxis.XScaleLineNumber / xaxis.XCommentNumber);

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
                wavechartuc.NumberOfDValue.ToString();  //todo replace 1187
            (xaxis_text_canvas.Children[xaxis_text_canvas.Children.Count - 1] as TextBlock).FontSize = 8;
            Canvas.SetLeft((xaxis_text_canvas.Children[xaxis_text_canvas.Children.Count - 1] as TextBlock), wavechartuc.NumberOfDValue * xaxis.GetGranulity() + yaxis.Width); //todo replace the 1187
            Canvas.SetBottom((xaxis_text_canvas.Children[xaxis_text_canvas.Children.Count - 1] as TextBlock), 0);

        }
    }
}

