using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BasicWaveChart.widget
{
    class YAxisCtl : Shape
    {
        #region DependencyProperty

        public int YScaleMaxValue
        {
            get
            {
                return (int)GetValue(YScaleMaxValueProperty);
            }
            set
            {
                SetValue(YScaleMaxValueProperty, value);
            }
        }

        public DependencyProperty YScaleMaxValueProperty = DependencyProperty.Register("YScaleMaxValue", typeof(int), typeof(YAxisCtl));

        //to draw scale after how many number
        public int YScaleLineNumber
        {
            get
            {
                return (int)(GetValue(YScaleLineNumberProperty));
            }
            set
            {
                SetValue(YScaleLineNumberProperty, value);
            }
        }

        public DependencyProperty YScaleLineNumberProperty = DependencyProperty.Register("YScaleLineNumber", typeof(int), typeof(YAxisCtl));

        //to comment text after how many number
        public int YCommentNumber
        {
            get
            {
                return (int)GetValue(YCommentNumberProperty);
            }
            set
            {
                SetValue(YCommentNumberProperty, value);
            }
        }

        public DependencyProperty YCommentNumberProperty = DependencyProperty.Register("YCommentNumber", typeof(int), typeof(YAxisCtl));

        //arrow style
        public ArrowStyleEnm YArrowStyle
        {
            get
            {
                return (ArrowStyleEnm)GetValue(YArrowStyleProperty);
            }
            set
            {
                SetValue(YArrowStyleProperty, value);
            }
        }

        public DependencyProperty YArrowStyleProperty = DependencyProperty.Register("YArrowStyle", typeof(ArrowStyleEnm), typeof(YAxisCtl));
        #endregion

        protected override Geometry DefiningGeometry
        {
            get
            {
                PathGeometry pg = new PathGeometry();
                PathFigure pf = new PathFigure();
                PolyLineSegment pls = new PolyLineSegment();
                pg.Figures.Add(pf);
                pf.StartPoint = new Point(11, 101);
                pls.Points.Add(new Point(10, 115));
                pls.Points.Add(new Point(14, 120));
                pls.Points.Add(new Point(20, 15));
                pls.Points.Add(new Point(30, 140));
                pls.Points.Add(new Point(45, 110));
                pf.Segments.Add(pls);
                pf.IsClosed = true;
                pg.FillRule = FillRule.Nonzero;
                return pg;
            }
        }
    }
}
