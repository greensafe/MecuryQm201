using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
}
