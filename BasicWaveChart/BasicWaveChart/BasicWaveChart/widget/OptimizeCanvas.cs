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
        //draw info
        //when point.y is larger than YScaleMaxValue, step
        readonly int maxy_step = 100;
        double comparepoint_x = 0;
        double comparepoint_y = 0;

        //effective point that can draw
        readonly double effectiveW = 0.5;
        readonly double effectiveH = 0.5;

        //context info
        BasicWaveChartUC parent;
        XAxisCtl xaxis ;
        YAxisCtl yaxis ;
        Slider moveslider;
        double OptimizeCanvas_Canvas_Left = 0;  //canvas.getleft() cann't get the position of canvas self. use the var to 
                            //save the position of optimizecanvas

        Polyline waveply = new Polyline();
        PointCollection datas_ = new PointCollection();
        PointCollection dvalues = new PointCollection();

        //self-abandon function
        AddPointDelegate AddPointRocket;



        public OptimizeCanvas()
        {
            waveply.Stroke = new SolidColorBrush(Colors.Black);
            waveply.StrokeThickness = 3;
            this.Children.Add(waveply);
            datas_ = waveply.Points;

            //self register event
            this.Loaded += new RoutedEventHandler(self_Loaded);

            //self-abandon function
            AddPointRocket = delegate (Point dvalue) {
                
                switch (parent.MoveMode)
                {
                    case WaveMoveMode.PACKED:
                        //
                        throw (new NotImplementedException());
                    case WaveMoveMode.HORIZONTAL:
                    default:
                        #region phase 1 - push
                        if(dvalue.X <= xaxis.XScaleMaxValue)
                        {
                            dvalues.Add(dvalue);
                            
                            if (dvalue.Y > yaxis.YScaleMaxValue)
                            {
                                int temp = (int)(dvalue.Y / maxy_step + 1) * maxy_step;
                                parent.SetScale(0, 0, 0, temp); // the scalechanged_ev will trigger draw action
                            }
                            else
                            {
                                if (datas_.Count == 0)
                                {
                                    datas_.Add(new Point(xaxis.GetXX((int)dvalue.X), yaxis.GetYY((int)dvalue.Y)));
                                    comparepoint_y = yaxis.GetYY((int)dvalue.Y);
                                    comparepoint_x = xaxis.GetXX((int)dvalue.X);
                                }
                                else
                                {
                                    if (isEffected(dvalue)) datas_.Add(new Point(xaxis.GetXX((int)dvalue.X), yaxis.GetYY((int)dvalue.Y)));
                                }
                            }
                        }
                        if (dvalue.X < xaxis.XScaleMaxValue)
                            return;
                        //add the XScaleMaxValue and abandon
                        #endregion
                        #region phase 2 - move
                        AddPointRocket = delegate (Point dvalue_move)
                        {
                            dvalues.Add(dvalue_move);
                            if(dvalue_move.Y > yaxis.YScaleMaxValue)
                            {
                                moveleft(dvalue_move);
                                parent.SetScale(0, 0, 0, (int)(dvalue_move.Y / maxy_step + 1)* maxy_step); //scalechanged_ev will call draw action
                            }
                            else
                            {
                                if(isEffected(dvalue_move))
                                {
                                    moveleft(dvalue_move);
                                    datas_.Add(new Point(xaxis.GetXX((int)dvalue_move.X),yaxis.GetYY((int)dvalue_move.Y)));
                                }
                            }
                        };
                        #endregion
                        break;
                }
            };

        }

        #region property
        public bool enableOptimize
        {
            get;set;
        }
        #endregion

        #region event handler

        //self event
        private void self_Loaded(object sender, RoutedEventArgs e)
        {
            parent = this.FindName("ControlContainer") as BasicWaveChartUC;
            xaxis = this.FindName("xaxis") as XAxisCtl;
            yaxis = this.FindName("yaxis") as YAxisCtl;
            moveslider = this.FindName("moveslider") as Slider;
            ;
        }

        public void ScaleChangedHdlr()
        {
            //redraw the wave
            if (dvalues == null) return;

            //
            datas_.Clear();
            comparepoint_x = 0;
            comparepoint_y = 0;
            foreach(Point dvalue in dvalues)
            {
                if (isEffected(dvalue))
                {
                    double x = xaxis.GetXX((int)dvalue.X);
                    double y = yaxis.GetYY((int)dvalue.Y);
                    datas_.Add(new Point(x,y));
                }
            }
        }
        #endregion

        #region public function
        //add a point to the wave
        public void AddPoint(Point dvalue)
        {
            AddPointRocket(dvalue);
        }

        //show full view of wave
        public void ShowFullView()
        {
            //Canvas.SetLeft(this, 0);
            moveslider.Visibility = Visibility.Hidden;
            int all = dvalues.Count;
            moveslider.Value = 0;
            parent.SetScale(0, all, 0,0);
        }

        //
        public PointCollection GetDValues()
        {
            return dvalues;
        }

        public PointCollection GetDatas()
        {
            return datas_;
        }

        #endregion


        #region private area
        private delegate void AddPointDelegate(Point dvalue);

        //judge whether the dvalue is needed to draw for optimization
        private bool isEffected(Point dvalue)
        {
            bool xbool = false;
            bool ybool = false;

            double x = xaxis.GetXX((int)dvalue.X);
            double y = yaxis.GetYY((int)dvalue.Y);
            if (x - comparepoint_x > effectiveW)
            {
                comparepoint_x = x;
                xbool = true;
            }
            if(y - comparepoint_y > effectiveH)
            {
                comparepoint_y = y;
                ybool = true;
            }

            if (xbool == true || ybool == true)
                return true;
            else
                return false;
        }

        //move the optimizecanvas to left according to the dvalue
        private void moveleft(Point dvalue)
        {
            OptimizeCanvas_Canvas_Left -= (xaxis.GetXX((int)dvalue.X) - datas_[datas_.Count - 1].X);
            moveslider.Value = OptimizeCanvas_Canvas_Left;
            //Canvas.SetLeft(this, OptimizeCanvas_Canvas_Left);
        }
        #endregion
    }
}
