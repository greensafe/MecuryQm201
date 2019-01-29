using BasicWaveChart.Feature.integral;
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
        Canvas wincanvas;
        Canvas opticanvas;
        double OptimizeCanvas_Canvas_Left = 0;  //canvas.getleft() cann't get the position of canvas self. use the var to 
                            //save the position of optimizecanvas

        Polyline waveply = new Polyline();
        PointCollection datas_ = new PointCollection();
        PointCollection dvalues = new PointCollection();

        //self-abandon function
        AddPointDelegate AddPointRocket;
        AddPointDelegate AddPointRocketOrigin;

        //enlarg up horital line
        Polyline enlarge_hor_upply = new Polyline();
        Polyline enlarge_hor_dowply = new Polyline();


        public OptimizeCanvas()
        {
            waveply.Stroke = new SolidColorBrush(Colors.Black);
            waveply.StrokeThickness = 3;
            this.Children.Add(waveply);
            datas_ = waveply.Points;

            //放大上下水平线
            enlarge_hor_upply.Stroke = new SolidColorBrush(Colors.Red);
            enlarge_hor_dowply.Stroke = new SolidColorBrush(Colors.Green);
            enlarge_hor_upply.StrokeThickness = 3;
            enlarge_hor_dowply.StrokeThickness = 3;
            this.Children.Add(enlarge_hor_upply);
            this.Children.Add(enlarge_hor_dowply);

            //self register event
            this.Loaded += new RoutedEventHandler(self_Loaded);

            //self-abandon function

            AddPointRocketOrigin = delegate (Point dvalue) {
                
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

                            if (dvalue.Y > yaxis.YScaleMaxValue &&
                            parent.featurestatus == BasicWaveChartUC.FeatureStatus.NORMAL)   //只有普通模式y轴才能自由扩展
                            {
                                int temp = (int)(dvalue.Y / maxy_step + 1) * maxy_step;
                                int scale = (int)(temp / 5 / 5);  //期望能显示5个大刻度，一个大刻包含5个小刻
                                if (scale > 50)
                                {
                                    scale = ((int)(scale / 50)) * 50;//去掉脆数，便于观察
                                }
                                parent.SetScale(0, 0, scale, 0, temp); // the scalechanged_ev will trigger draw action
                            }
                            else if (dvalue.Y <= yaxis.YScaleMaxValue ||   //放大模式不要扩展y轴，超出最大值的点绘制但是不可见
                            (dvalue.Y > yaxis.YScaleMaxValue && parent.featurestatus == BasicWaveChartUC.FeatureStatus.ENALARGE))
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
                            if (dvalue_move.Y > yaxis.YScaleMaxValue &&
                                parent.featurestatus == BasicWaveChartUC.FeatureStatus.NORMAL)   //只有普通模式y轴才能自由扩展
                            {
                                moveleft(dvalue_move);
                                int temp = (int)(dvalue_move.Y / maxy_step + 1) * maxy_step;
                                int scale = (int)(temp / 5 / 5);  //期望能显示5个大刻度，一个大刻包含5个小刻
                                if (scale > 50)
                                {
                                    scale = ((int)(scale / 50)) * 50;//去掉脆数，便于观察
                                }
                                parent.SetScale(0, 0, scale, 0, temp);
                                //parent.SetScale(0, 0, 0, (int)(dvalue_move.Y / maxy_step + 1)* maxy_step); //scalechanged_ev will call draw action
                            }
                            else if (dvalue_move.Y <= yaxis.YScaleMaxValue ||   //放大模式不要扩展y轴，超出最大值的点绘制但是不可见
                            (dvalue_move.Y > yaxis.YScaleMaxValue && parent.featurestatus == BasicWaveChartUC.FeatureStatus.ENALARGE))
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
            AddPointRocket = AddPointRocketOrigin;
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
            wincanvas = this.FindName("WindowCanvas") as Canvas;
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

        public void DrawEnlargeHoritalUpLine(double y)
        {
            enlarge_hor_upply.Points.Clear();
            enlarge_hor_upply.Points.Add(new Point(0, y));
            enlarge_hor_upply.Points.Add(new Point(this.Width, y));
        }

        public void DrawEnlargeHoritalDownLine(double y)
        {
            enlarge_hor_dowply.Points.Clear();
            enlarge_hor_dowply.Points.Add(new Point(0, y));
            enlarge_hor_dowply.Points.Add(new Point(this.Width, y));
        }

        //show full view of wave
        public void ShowFullView()
        {
            //Canvas.SetLeft(this, 0);
            moveslider.Visibility = Visibility.Hidden;
            int all = dvalues.Count;
            moveslider.Value = 0;
            //try to keep the XCommentNumber is 10
            xaxis.XScaleLineNumber = (int)(all / xaxis.XCommentNumber/5);
            parent.SetScale(0, all, 0, 0, 0);
            //parent.NumberOfDValue = all;
            parent.SetNumberOfDValueP(all);
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


        public void ClearData()
        {
            OptimizeCanvas_Canvas_Left = 0;
            dvalues.Clear();
            waveply.Points.Clear();
            //清除积分中的图形痕迹
            foreach(UIElement item in wincanvas.Children)
            {
                if (item is HandleCtl || item is TextBlock)
                    item.Visibility = Visibility.Hidden;
            }
            foreach(UIElement pg in this.Children)
            {
                if (pg is Polygon)
                    pg.Visibility = Visibility.Hidden;
            }
            //恢复自弃函数
            AddPointRocket = AddPointRocketOrigin;

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

        internal void HideEnlargeHoritalLine()
        {
            enlarge_hor_dowply.Visibility = Visibility.Collapsed;
            enlarge_hor_upply.Visibility = Visibility.Collapsed;
        }

        internal void ShowEnlargeHoritalLine()
        {
            enlarge_hor_dowply.Visibility = Visibility.Visible;
            enlarge_hor_upply.Visibility = Visibility.Visible;
        }

        //move the optimizecanvas to left according to the dvalue
        private void moveleft(Point dvalue)
        {
            OptimizeCanvas_Canvas_Left -= (xaxis.GetXX((int)dvalue.X) - datas_[datas_.Count - 1].X);
            moveslider.Value = OptimizeCanvas_Canvas_Left;
        }
        #endregion
    }
}
