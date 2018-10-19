using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BasicWaveChart.widget
{
    /*
     * Zoom content 
     */
    class ZoomPanel : Panel
    {
        private int zoomx = 3;
        private int zoomy = 1;

        //ratio about height:width
        #region RatioProperty
        public string Ratio {
            get
            {
                return (string)GetValue(RatioProperty);
            }
            set
            {
                SetValue(RatioProperty, value);
                /*
                if (value == null) { Console.WriteLine("ZoomPanel: RatioProperty is null");return; }
                Regex regex = new Regex(@"\d[:]\d");
                if (regex.IsMatch(value)){
                    string[] rs = Regex.Split(value, ":");
                    zoomy = int.Parse(rs[0]);
                    zoomx = int.Parse(rs[1]);
                    Console.WriteLine("set: zoomx=" + zoomx.ToString());
                    Console.WriteLine("set: zoomy=" + zoomy.ToString());
                }
                else
                {
                    zoomy = 1;
                    zoomx = 2;
                    Console.WriteLine("BasicWaveChart: ratio's format is not valid");
                }
                */
            }
        }
        public static readonly DependencyProperty RatioProperty =
                DependencyProperty.Register("Ratio", typeof(string), typeof(ZoomPanel),
                new UIPropertyMetadata("1:1", new PropertyChangedCallback(RatioChanged)),
                new ValidateValueCallback(RatioIsNumber)
        );

        private static void RatioChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null) { Console.WriteLine("ZoomPanel: change RatioProperty is null"); return; }

            ZoomPanel panel = d as ZoomPanel;
            string ratio = e.NewValue as string;
            Regex regex = new Regex(@"\d[:]\d");

            
            if (regex.IsMatch(ratio))
            {
                string[] rs = Regex.Split(ratio, ":");
                panel.zoomy = int.Parse(rs[0]);
                panel.zoomx = int.Parse(rs[1]);
                Console.WriteLine("set: zoomx=" + panel.zoomx.ToString());
                Console.WriteLine("set: zoomy=" + panel.zoomy.ToString());
            }
            else
            {
                panel.zoomy = 1;
                panel.zoomx = 2;
                Console.WriteLine("BasicWaveChart: ratio's format is not valid");
            }
            
            return;
            //throw new NotImplementedException();
        }

        private static bool RatioIsNumber(object value)
        {
            return true;
            //throw new NotImplementedException();
        }
        #endregion

        protected override Size ArrangeOverride(Size finalSize)
        {
            Size rsize = new Size();
            Point start = new Point(0, 0);

            if(finalSize.Width/finalSize.Height - zoomx/zoomy > 0)
            {
                rsize.Height = finalSize.Height;
                rsize.Width = finalSize.Width;
            }
            else
            {
                rsize.Width = finalSize.Width;
                rsize.Height = finalSize.Width * zoomy / zoomx;
                start.Y = (finalSize.Height - rsize.Height) / 2;
            }
            this.InternalChildren[0].Arrange(new Rect(start, rsize));
           
            return finalSize;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return base.MeasureOverride(availableSize);
        }
    }


 

    //try
    public class Star : Shape
    {
        protected PathGeometry pg;
        PathFigure pf;
        PolyLineSegment pls;

        public Star()
        {
            pg = new PathGeometry();
            pf = new PathFigure();
            pls = new PolyLineSegment();
            pg.Figures.Add(pf);
        }

        // Specify the center of the star
        public static readonly DependencyProperty CenterProperty =
            DependencyProperty.Register("Center", typeof(Point), typeof(Star),
            new FrameworkPropertyMetadata(new Point(20.0, 20.0),
            FrameworkPropertyMetadataOptions.AffectsMeasure));
        public Point Center
        {
            set { SetValue(CenterProperty, value); }
            get { return (Point)GetValue(CenterProperty); }
        }

        // Specify the size of the star:
        public static readonly DependencyProperty SizeRProperty =
            DependencyProperty.Register("SizeR", typeof(double), typeof(Star),
            new FrameworkPropertyMetadata(10.0,
            FrameworkPropertyMetadataOptions.AffectsMeasure));
        public double SizeR
        {
            set { SetValue(SizeRProperty, value); }
            get { return (double)GetValue(SizeRProperty); }
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                double r = SizeR;
                double x = Center.X;
                double y = Center.Y;
                double sn36 = Math.Sin(36.0 * Math.PI / 180.0);
                double sn72 = Math.Sin(72.0 * Math.PI / 180.0);
                double cs36 = Math.Cos(36.0 * Math.PI / 180.0);
                double cs72 = Math.Cos(72.0 * Math.PI / 180.0);

                pf.StartPoint = new Point(x, y - r);
                pls.Points.Add(new Point(x + r * sn36, y + r * cs36));
                pls.Points.Add(new Point(x - r * sn72, y - r * cs72));
                pls.Points.Add(new Point(x + r * sn72, y - r * cs72));
                pls.Points.Add(new Point(x - r * sn36, y + r * cs36));
                pls.Points.Add(new Point(x, y - r));
                pf.Segments.Add(pls);
                pf.IsClosed = true;
                pg.FillRule = FillRule.Nonzero;

                return pg;
            }
        }
    }
}
