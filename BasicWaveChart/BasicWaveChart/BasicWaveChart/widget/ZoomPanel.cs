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
        private int zoomx = 1;
        private int zoomy = 1;
        //ratio about height:width
        public string Ratio {
            get
            {
                return null;
            }
            set
            {
                Regex regex = new Regex("\b[:]\b");
                if (regex.IsMatch(value)){
                    string[] rs = Regex.Split(value, ":");
                    zoomy = int.Parse(rs[0]);
                    zoomx = int.Parse(rs[1]);
                }
                else
                {
                    Console.WriteLine("BasicWaveChart: ratio's format is not valid");
                }
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            
            return base.ArrangeOverride(finalSize);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return base.MeasureOverride(availableSize);
        }
    }
}
