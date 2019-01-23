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
        double vice_comparepoint_x = 0;
        double vice_comparepoint_y = 0;


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
        //辅道波形
        Polyline vice_waveply = new Polyline();
        PointCollection vice_datas_ = new PointCollection();
        PointCollection vice_dvalues = new PointCollection();

        //self-abandon function
        AddPointDelegate AddPointRocket;
        AddPointDelegate AddPointRocketOrigin;



        public OptimizeCanvas()
        {
            waveply.Stroke = new SolidColorBrush(Colors.Black);
            waveply.StrokeThickness = 3;
            this.Children.Add(waveply);
            datas_ = waveply.Points;
            //辅道数据
            vice_waveply.Stroke = new SolidColorBrush(Colors.Blue);
            vice_waveply.StrokeThickness = 3;
            this.Children.Add(vice_waveply);
            vice_datas_ = vice_waveply.Points;

            //主通道点是有效的，待绘制
            bool main_dot_is_effected = false;

            //self register event
            this.Loaded += new RoutedEventHandler(self_Loaded);

            //self-abandon function
            //理想状况下副通道值与主通道值一一对应。故波形移动仅仅由主通道控制
            AddPointRocketOrigin = delegate (Object odvalue ,Object ovice_dvalue) {
                Point dvalue;
                Point vice_dvalue;
                switch (parent.MoveMode)
                {
                    case WaveMoveMode.PACKED:
                        //
                        throw (new NotImplementedException());
                    case WaveMoveMode.HORIZONTAL:
                    default:
                        #region phase 1 - push
                        if (odvalue != null)
                        {
                            dvalue = (Point)odvalue;
                            if (dvalue.X <= xaxis.XScaleMaxValue)
                            {
                                dvalues.Add(dvalue);

                                if (dvalue.Y > yaxis.YScaleMaxValue)
                                {
                                    int temp = (int)(dvalue.Y / maxy_step + 1) * maxy_step;
                                    int scale = (int)(temp / 5 / 5);  //期望能显示5个大刻度，一个大刻包含5个小刻
                                    if (scale > 50)
                                    {
                                        scale = ((int)(scale / 50)) * 50;//去掉脆数，便于观察
                                    }
                                    parent.SetScale(0, 0, scale, temp); // the scalechanged_ev will trigger draw action
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
                                        if (isEffected(dvalue))
                                        {
                                            main_dot_is_effected = true;
                                            datas_.Add(new Point(xaxis.GetXX((int)dvalue.X), yaxis.GetYY((int)dvalue.Y)));
                                        }
                                    }
                                }
                            }
                            /*
                            if (dvalue.X < xaxis.XScaleMaxValue)
                                return;
                            */
                        }
                       
                        //副通道
                        if(ovice_dvalue != null)
                        {
                            vice_dvalue = (Point)ovice_dvalue;
                            vice_dvalues.Add(vice_dvalue);

                            if (vice_dvalue.Y > yaxis.YScaleMaxValue)
                            {
                                int temp = (int)(vice_dvalue.Y / maxy_step + 1) * maxy_step;
                                int scale = (int)(temp / 5 / 5);  //期望能显示5个大刻度，一个大刻包含5个小刻
                                if (scale > 50)
                                {
                                    scale = ((int)(scale / 50)) * 50;//去掉脆数，便于观察
                                }
                                parent.SetScale(0, 0, scale, temp); // the scalechanged_ev will trigger draw action
                            }
                            else
                            {
                                if (vice_datas_.Count == 0)
                                {
                                    vice_datas_.Add(new Point(xaxis.GetXX((int)vice_dvalue.X), yaxis.GetYY((int)vice_dvalue.Y)));
                                    vice_comparepoint_y = yaxis.GetYY((int)vice_dvalue.Y);
                                    vice_comparepoint_x = xaxis.GetXX((int)vice_dvalue.X);
                                }
                                else
                                {
                                    //if (isEffected(vice_dvalue)) vice_datas_.Add(new Point(xaxis.GetXX((int)vice_dvalue.X), yaxis.GetYY((int)vice_dvalue.Y)));
                                    if (main_dot_is_effected == true)
                                    {
                                        main_dot_is_effected = false;
                                        vice_datas_.Add(new Point(xaxis.GetXX((int)vice_dvalue.X), yaxis.GetYY((int)vice_dvalue.Y)));
                                    }
                                }
                            }
                        }
                        //副通道不要影响移动逻辑
                        if (odvalue == null) return;

                        if (odvalue != null){
                            dvalue = (Point)odvalue;
                            if (dvalue.X < xaxis.XScaleMaxValue)
                                return;
                        }
                        //add the XScaleMaxValue and abandon
                        #endregion
                        #region phase 2 - move
                        AddPointRocket = delegate (Object odvalue_move, Object ovice_dvalue_move)
                        {
                            Point dvalue_move;
                            Point vice_dvalue_move;

                            if (odvalue_move != null)
                            {
                                dvalue_move = (Point)odvalue_move;
                                dvalues.Add(dvalue_move);
                                if (dvalue_move.Y > yaxis.YScaleMaxValue)
                                {
                                    moveleft(dvalue_move);
                                    int temp = (int)(dvalue_move.Y / maxy_step + 1) * maxy_step;
                                    int scale = (int)(temp / 5 / 5);  //期望能显示5个大刻度，一个大刻包含5个小刻
                                    if (scale > 50)
                                    {
                                        scale = ((int)(scale / 50)) * 50;//去掉脆数，便于观察
                                    }
                                    parent.SetScale(0, 0, scale, temp);
                                    //parent.SetScale(0, 0, 0, (int)(dvalue_move.Y / maxy_step + 1) * maxy_step); //scalechanged_ev will call draw action
                                }
                                else
                                {
                                    if (isEffected(dvalue_move))
                                    {
                                        main_dot_is_effected = true;
                                        moveleft(dvalue_move);
                                        datas_.Add(new Point(xaxis.GetXX((int)dvalue_move.X), yaxis.GetYY((int)dvalue_move.Y)));
                                    }
                                }
                            }

                            //副通道
                            if (ovice_dvalue_move != null)
                            {
                                vice_dvalue_move = (Point)ovice_dvalue_move;
                                vice_dvalues.Add(vice_dvalue_move);
                                if (vice_dvalue_move.Y > yaxis.YScaleMaxValue)
                                {
                                    //moveleft(dvalue_move);
                                    int temp = (int)(vice_dvalue_move.Y / maxy_step + 1) * maxy_step;
                                    int scale = (int)(temp / 5 / 5);  //期望能显示5个大刻度，一个大刻包含5个小刻
                                    if (scale > 50)
                                    {
                                        scale = ((int)(scale / 50)) * 50;//去掉脆数，便于观察
                                    }
                                    parent.SetScale(0, 0, scale, temp);
                                    //parent.SetScale(0, 0, 0, (int)(vice_dvalue_move.Y / maxy_step + 1) * maxy_step); //scalechanged_ev will call draw action
                                }
                                else
                                {
                                    if (main_dot_is_effected == true)
                                    {
                                        //moveleft(dvalue_move);
                                        main_dot_is_effected = false;
                                        vice_datas_.Add(new Point(xaxis.GetXX((int)vice_dvalue_move.X), yaxis.GetYY((int)vice_dvalue_move.Y)));
                                    }
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
        //@ dvalue - 主通道值
        //  vice_dvalue - 副通道值
        // 应该只有两种方式。
        //    主通道调用   AddPoint(dvalue,null)
        //    副通道调用   AddPoint(null, vice_dvalue)
        public void AddPoint(Object dvalue,Object vice_dvalue)
        {
            AddPointRocket(dvalue,vice_dvalue);
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
            parent.SetScale(0, all, 0,0);
            //parent.NumberOfDValue = all;
            parent.SetNumberOfDValueP(all);
        }

        //
        public PointCollection GetDValues()
        {
            return dvalues;
        }

        public PointCollection GetViceDValues()
        {
            return vice_dvalues;
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
            //清除辅道数据
            vice_dvalues.Clear();
            vice_waveply.Points.Clear();
            //清除积分中的图形痕迹
            foreach (UIElement item in wincanvas.Children)
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
        //@ dvalue - 主通道值
        //  vice_dvalue - 副通道值
        private delegate void AddPointDelegate(Object odvalue, Object ovice_dvalue);

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
        }
        #endregion
    }
}
