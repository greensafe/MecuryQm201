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
    class OptimizeCanvas:Canvas
    {
        //effective point that can draw
        double effectiveW = 0.5;
        double effectiveH = 0.5;
        Polyline waveply = new Polyline();
        PointCollection data_ = new PointCollection();
        PointCollection dvalues = new PointCollection();

        public OptimizeCanvas()
        {
            waveply.Stroke = new SolidColorBrush(Colors.Black);
            waveply.StrokeThickness = 3;
            this.Children.Add(waveply);
            data_ = waveply.Points;
            //for demo
            dvalues.Add(new Point(1, 3));
            dvalues.Add(new Point(10, 400));
            dvalues.Add(new Point(400,2300));
            dvalues.Add(new Point(600, 5300));
            dvalues.Add(new Point(700, 1300));
        }

        #region property
        public bool enableOptimize
        {
            get;set;
        }
        #endregion

        #region event handler
        public void ScaleChangedHdlr()
        {
            //redraw the wave
            if (dvalues == null) return;
            XAxisCtl xaxis = this.FindName("xaxis") as XAxisCtl;
            YAxisCtl yaxis = this.FindName("yaxis") as YAxisCtl;

            data_.Clear();
            foreach(Point dvalue in dvalues)
            {
                data_.Add(new Point(xaxis.GetXX((int)dvalue.X),
                    yaxis.GetYY((int)dvalue.Y)));
            }
        }
        #endregion

        #region public function

        #endregion

    }
}
