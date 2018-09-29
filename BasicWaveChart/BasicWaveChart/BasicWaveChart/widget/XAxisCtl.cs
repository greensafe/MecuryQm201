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
    class XAxisCtl : Shape
    {
        //the width of expected granulity
        protected double granulity_width = 1;
        
        #region DependencyProperty
        
        public int XScaleMaxValue
        {
            get {
                return (int)GetValue(XScaleMaxValueProperty);
            }
            set {
                SetValue(XScaleMaxValueProperty, value);
            }
        }

        public DependencyProperty XScaleMaxValueProperty = DependencyProperty.Register("XScaleMaxValue", typeof(int), typeof(XAxisCtl));

        //to draw scale after how many number
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
        
        public DependencyProperty XScaleLineNumberProperty = DependencyProperty.Register("XScaleLineNumber", typeof(int), typeof(XAxisCtl));

        //to comment text after how many number
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

        public DependencyProperty XCommentNumberProperty = DependencyProperty.Register("XCommentNumber", typeof(int), typeof(XAxisCtl));

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

        public DependencyProperty XArrowStyleProperty = DependencyProperty.Register("XArrowStyle", typeof(ArrowStyleEnm), typeof(XAxisCtl));
        #endregion
        
        protected override Geometry DefiningGeometry
        {
            get
            {
                PathGeometry pg = new PathGeometry();
                PathFigure pf = new PathFigure();
                PolyLineSegment pls = new PolyLineSegment();
                pg.Figures.Add(pf);
                pf.StartPoint = new Point(1, 1);
                pls.Points.Add(new Point(10, 15));
                pls.Points.Add(new Point(14, 20));
                pls.Points.Add(new Point(20, 5));
                pls.Points.Add(new Point(30, 40));
                pls.Points.Add(new Point(45, 10));
                pf.Segments.Add(pls);
                pf.IsClosed = true;
                pg.FillRule = FillRule.Nonzero;
                return pg;
            }
        }
    }

}

