﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BasicWaveChart.widget
{
    /*
     * say together.
     * dot - a real dot in screen
     * dvalue - value of data, the min value is "1" . if it's 0.01, 0.002,0.003, should first convert to 1,2,3
     */
    class YAxisCtl : Shape
    {
        protected double granulity_width = 1;               //the width of expected granulity
        private double littleScaleHeight = 6;               //height of little scale
        private double bigScaleHeight = 10;                 //height of big scale
        private double axisThickness = 3;                   //the thickness of axis 
        private double littleScaleThickness = 3;            //thickness of little scale
        private double bigScaleThickness = 3;               //thickness of big scale
        private double rightblank = 3;                        //magin at right of axis
        private double arrowheight = 10;                     //size of arrow

        #region DependencyProperty
        //arrow height
        public double YArrowheight
        {
            get
            {
                return (double)GetValue(YArrowheightProperty);
            }
            set
            {
                SetValue(YArrowheightProperty, value);
            }
        }
        public DependencyProperty YArrowheightProperty = DependencyProperty.Register("YArrowheight", typeof(double), typeof(YAxisCtl));

        //max dvalue to draw
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

        //how many dvalue that a scale include
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

        //to comment text after how many YScaleLineNumber
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

                XAxisCtl xaxisctl = this.FindName("xaxis") as XAxisCtl;
                BasicWaveChartUC wavechartuc = this.FindName("ControlContainer") as BasicWaveChartUC;


                if (xaxisctl is null || wavechartuc is null)
                {
                    this.granulity_width = 1;
                    return new PathGeometry();
                }
                this.granulity_width = (this.Height - xaxisctl.Height - this.arrowheight  - wavechartuc.TopBlankZone) / (int)GetValue(YScaleMaxValueProperty);

                PathGeometry pg = new PathGeometry();

                PathFigure pf = new PathFigure();
                pg.Figures.Add(pf);

                double vlinevalue = this.Width - rightblank;
                /*
                PolyLineSegment axisSeg = new PolyLineSegment();
                //pf.StartPoint = new Point(yaxisctl.Width, this.Height - topblank);
                axisSeg.Points.Add(new Point(yaxisctl.Width, hlinevalue));
                axisSeg.Points.Add(new Point(this.Width - arrowheight, hlinevalue));
                pf.Segments.Add(axisSeg);
                */

                PolyLineSegment ScaleSeg = new PolyLineSegment();
                if (this.YScaleLineNumber == 0) this.YScaleLineNumber = 100;
                int scalenumber = (int)(this.YScaleMaxValue / this.YScaleLineNumber);
                for (int i = 0; i < scalenumber; i++)
                {
                    ScaleSeg.Points.Add(new Point(vlinevalue, i * YScaleLineNumber * granulity_width + xaxisctl.Height));
                    if (i % this.YCommentNumber == 0)
                    {
                        ScaleSeg.Points.Add(new Point(vlinevalue - bigScaleHeight, i * YScaleLineNumber * granulity_width + xaxisctl.Height));
                    }
                    else
                    {
                        ScaleSeg.Points.Add(new Point(vlinevalue - littleScaleHeight, i * YScaleLineNumber * granulity_width + xaxisctl.Height));
                    }
                    ScaleSeg.Points.Add(new Point(vlinevalue, i * YScaleLineNumber * granulity_width + xaxisctl.Height));
                }

                ScaleSeg.Points.Add(new Point( vlinevalue, YScaleLineNumber * scalenumber * granulity_width + xaxisctl.Height));
                ScaleSeg.Points.Add(new Point(vlinevalue + 20, YScaleLineNumber * scalenumber * granulity_width + xaxisctl.Height));
                ScaleSeg.Points.Add(new Point(vlinevalue - 20, YScaleLineNumber * scalenumber * granulity_width + xaxisctl.Height));

                pf.Segments.Add(ScaleSeg);
                PolyLineSegment arrowseg = new PolyLineSegment();

                switch (this.YArrowStyle)
                {
                    case ArrowStyleEnm.SOLID:
                    case ArrowStyleEnm.HOLLOW:
                        {
                            arrowseg.Points.Add(new Point(vlinevalue, this.Height - wavechartuc.TopBlankZone - this.arrowheight));
                            arrowseg.Points.Add(new Point(vlinevalue - this.arrowheight / 2, this.Height - wavechartuc.TopBlankZone - this.arrowheight));
                            arrowseg.Points.Add(new Point(vlinevalue + this.arrowheight / 2, this.Height - wavechartuc.TopBlankZone - this.arrowheight));
                            arrowseg.Points.Add(new Point(vlinevalue, this.Height - wavechartuc.TopBlankZone));
                            arrowseg.Points.Add(new Point(vlinevalue - this.arrowheight / 2, this.Height - wavechartuc.TopBlankZone - this.arrowheight));
                            pf.IsFilled = true;
                            break;
                        }
                    case ArrowStyleEnm.NONE:
                    default:
                        {
                            arrowseg.Points.Add(new Point(vlinevalue, this.Height - wavechartuc.TopBlankZone - this.arrowheight));
                            arrowseg.Points.Add(new Point(vlinevalue, this.Height - wavechartuc.TopBlankZone));
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
    }
}
