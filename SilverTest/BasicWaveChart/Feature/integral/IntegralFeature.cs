﻿using BasicWaveChart.widget;
using featurefactory.Basic;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BasicWaveChart.Feature.integral
{
    public class IntegralFeature: DependencyObject
    {
        public static readonly DependencyProperty EnableProperty = DependencyProperty.RegisterAttached(
                "Enable", typeof(bool), typeof(IntegralFeature), new PropertyMetadata(false, OnEnableChanged));

        public static void SetEnable(DependencyObject d, bool use)
        {
            d.SetValue(EnableProperty, use);
        }
        private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (((bool)e.NewValue == true) && ((bool)e.OldValue == false))
            {
                BasicWaveChartUC basicwave = d as BasicWaveChartUC;
                basicwave.Loaded += new RoutedEventHandler(targetloaded_hdlr);
            }
            ;
        }

        private static void targetloaded_hdlr(object sender, RoutedEventArgs e)
        {
            IntegralWorker.Create(sender as BasicWaveChartUC).Enable();
        }
    }

    internal class IntegralWorker
    {
        BasicWaveChartUC ucctl;
        
        Polygon areagon;
        TextBlock commenttx;
        private static dynamic hostcontext = new HostContext();

        HandleCtl mainhandle;
        private IntegralWorker(BasicWaveChartUC param)
        {
            ucctl = param;
            areagon = new Polygon();
            areagon.Name = "areagon";
            areagon.Visibility = Visibility.Visible;
            commenttx = new TextBlock();
            commenttx.Width = 80;
            commenttx.Height = 20;
            commenttx.Visibility = Visibility.Visible;
            commenttx.Name = "commenttx";
            commenttx.TextAlignment = TextAlignment.Center;
            //main handle
            mainhandle = new HandleCtl(new HandleCtl());
        }
        public static IntegralWorker Create(BasicWaveChartUC param)
        {
            IntegralWorker w = new IntegralWorker(param);
            return w;
        }
        public void Enable()
        {
            try
            {
                ContextMenu menus = ucctl.TryFindResource("wavemenu") as ContextMenu;
                foreach(MenuItem item in menus.Items)
                {
                    if(item.Header.ToString() == "积分")
                    {
                        item.Visibility = Visibility.Visible;
                    }
                }
                ;
            }
            catch { }

            hostcontext.container = ucctl.FindName("WindowCanvas");
            hostcontext.xaxis = ucctl.FindName("xaxis");
            hostcontext.yaxis = ucctl.FindName("yaxis");
            hostcontext.optimizeCanvas = ucctl.FindName("optimizeCanvas") as OptimizeCanvas;
            (hostcontext.container as Canvas).Children.Add(mainhandle);
            (hostcontext.container as Canvas).Children.Add(mainhandle.GetBrother());
            Canvas.SetLeft(mainhandle, hostcontext.container.Width / 3);
            Canvas.SetTop(mainhandle, 0);
            Canvas.SetLeft(mainhandle.GetBrother(), hostcontext.container.Width / 3 + 100);
            Canvas.SetTop(mainhandle.GetBrother(), 0);
            //convert the handle
            ScaleTransform scaletrs = new ScaleTransform();
            scaletrs.ScaleY = -1;
            TranslateTransform movetrs = new TranslateTransform();
            movetrs.Y = mainhandle.Height;
            TransformGroup grouptrs = new TransformGroup();
            grouptrs.Children.Add(scaletrs);
            grouptrs.Children.Add(movetrs);
            mainhandle.RenderTransform = grouptrs;
            mainhandle.GetBrother().RenderTransform = grouptrs;
            //open move feature
            MoveFeature.SetEnble(mainhandle, true);
            MoveFeature.SetEnble(mainhandle.GetBrother(), true);
            //hidden
            mainhandle.Visibility = Visibility.Hidden;
            mainhandle.GetBrother().Visibility = Visibility.Hidden;
            mainhandle.Name = "twohandle";
            //register move handle
            mainhandle.Move_Ev += MainHandleMoveHdlr;
            mainhandle.GetBrother().Move_Ev += brotherHandleMoveHdlr;
            //polygon
            hostcontext.optimizeCanvas.Children.Add(areagon);
            areagon.Stroke = new SolidColorBrush(Colors.Black);
            areagon.StrokeThickness = 4;
            areagon.Fill = new SolidColorBrush(Colors.Red);
            //comment
            hostcontext.container.Children.Add(commenttx);
            ScaleTransform scaletrs_comment = new ScaleTransform();
            scaletrs_comment.ScaleY = -1;
            TranslateTransform movetrs_comment = new TranslateTransform();
            movetrs_comment.Y = commenttx.Height;
            TransformGroup grouptrs_comment = new TransformGroup();
            grouptrs_comment.Children.Add(scaletrs_comment);
            grouptrs_comment.Children.Add(movetrs_comment);
            commenttx.RenderTransform = grouptrs_comment;
        }

        private void brotherHandleMoveHdlr(double pos)
        {
            makepg(mainhandle.GetXm(), pos);
            showcomment(mainhandle.GetXm(), pos);
        }

        private void MainHandleMoveHdlr(double pos)
        {
            makepg(pos, mainhandle.GetBrother().GetXm());
            showcomment(pos, mainhandle.GetBrother().GetXm());
        }



        //面积积分方法可以重写
        public virtual double IntegrateData(int dvalues_start, int dvalues_end)
        {
            double total = 0;
            
            for(int i = dvalues_start; i <= dvalues_end; i++)
            {
                total += hostcontext.dvalues[i].Y;
            }
            total /= (dvalues_end - dvalues_start);
            return total;
        }

        #region private function
        //make the area polygon
        private void makepg(double bigx, double brotherx)
        {
            hostcontext.datas_ = hostcontext.optimizeCanvas.GetDatas();
            hostcontext.dvalues = hostcontext.optimizeCanvas.GetDValues();

            areagon.Points.Clear();

            double startx = Canvas.GetLeft(hostcontext.optimizeCanvas);
            startx = startx * (-1) + bigx;
            double endx = startx + (brotherx - bigx);
            double px = startx;
            
            int startx_index = hostcontext.xaxis.GetDValueIndex(startx);
            int end_index = hostcontext.xaxis.GetDValueIndex(endx);
            int index = startx_index;
            hostcontext.polygon_dvalues_start_index = startx_index;
            hostcontext.polygon_dvalues_end_index = end_index;
            //int index = 0;
            //areagon.Points.Add(new Point(hostcontext.datas_[startx_index].X,0));
            areagon.Points.Add(new Point(hostcontext.xaxis.GetXX(startx_index), 0));
            double tempx, tempy;
            int tempY;
            //avoid the startx_index and end_index flying out
            if (startx_index >= hostcontext.dvalues.Count)
                return;
            if (startx_index < 0)
            {
                startx_index = 0;
                index = 0;
            }
            if (end_index < 0)
                end_index = 0;
            if (end_index >= hostcontext.dvalues.Count)
                end_index = hostcontext.dvalues.Count - 1;
            while ( index >= startx_index && index < end_index)
            {
                //areagon.Points.Add(hostcontext.datas_[index]);
                tempx = hostcontext.xaxis.GetXX(index);
                tempY = (int)hostcontext.dvalues[index].Y;
                tempy = hostcontext.yaxis.GetYY(tempY);
                areagon.Points.Add(new Point( tempx , tempy ));
                index++;
            }
            areagon.Points.Add(new Point(hostcontext.xaxis.GetXX(end_index), 0));
        }

        //
        private int findIndexInDatas_(double x)
        {
            int index = 0;
            //looking for the start in data_
            foreach (Point p in (hostcontext.datas_ as PointCollection))
            {
                if (index >= hostcontext.datas_.Count - 1)
                    break;
                if ((int)hostcontext.datas_[index].X == (int)x)
                {
                    break;
                }
                index++;
            }
           
            return index;
        }

        //show the comment 
        private void showcomment(double x, double y)
        {
            double res;
            if (hostcontext.polygon_dvalues_start_index >= hostcontext.dvalues.Count)
            {
                commenttx.Text = "0.00";
                return;
            }
            if (hostcontext.polygon_dvalues_start_index < 0)
                hostcontext.polygon_dvalues_start_index = 0;
            if (hostcontext.polygon_dvalues_end_index >= hostcontext.dvalues.Count)
                hostcontext.polygon_dvalues_end_index = hostcontext.dvalues.Count - 1;
            if (hostcontext.polygon_dvalues_end_index < 0)
                hostcontext.polygon_dvalues_end_index = 0;
            res = IntegrateData(hostcontext.polygon_dvalues_start_index,
                hostcontext.polygon_dvalues_end_index);
            commenttx.Text = res.ToString("0.00");
            Canvas.SetLeft(commenttx,(x + y )/2 - commenttx.Width/2);
            Canvas.SetTop(commenttx, 10);

            ucctl.TriggerIntegrateValueChangeEv(res);
        }
        #endregion
    }

    public class HostContext : DynamicObject
    {
        Dictionary<string, object> dictionary
            = new Dictionary<string, object>();
        public int Count
        {
            get
            {
                return dictionary.Count;
            }
        }
        public override bool TryGetMember(
            GetMemberBinder binder, out object result)
        {
            string name = binder.Name.ToLower();
            return dictionary.TryGetValue(name, out result);
        }
        public override bool TrySetMember(
            SetMemberBinder binder, object value)
        {
            dictionary[binder.Name.ToLower()] = value;
            return true;
        }
    }

}
