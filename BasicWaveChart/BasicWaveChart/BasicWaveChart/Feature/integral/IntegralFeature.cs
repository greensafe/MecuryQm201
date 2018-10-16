using BasicWaveChart.widget;
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
        //private static BasicWaveChartUC hostctl = null;
        //private static dynamic hostcontext = new HostContext();

        public static readonly DependencyProperty EnableProperty = DependencyProperty.RegisterAttached(
                "Enable", typeof(bool), typeof(IntegralFeature), new PropertyMetadata(false, OnEnableChanged));

        public static void SetEnable(DependencyObject d, bool use)
        {
            d.SetValue(EnableProperty, use);
        }
        private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BasicWaveChartUC basicwave = d as BasicWaveChartUC;
            basicwave.Loaded += new RoutedEventHandler(targetloaded_hdlr);
            ;
            //throw new NotImplementedException();
        }

        private static void targetloaded_hdlr(object sender, RoutedEventArgs e)
        {
            IntegralWorker.Create(sender as BasicWaveChartUC).Enable();
            //hostcontext.window = hostctl.WindowCanvas;   //canvas
            //hostcontext.container = hostctl.basecanvas;  //canvas
            //Console.WriteLine(hostcontext.window.Width);
            //Console.WriteLine(hostcontext.window.Height);
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
            //commenttx.Background = new SolidColorBrush(Colors.Red);
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
            //context info
            //container
            //dvalues
            //datas_ of ployline
            //xaxis
            //yaxis
            hostcontext.container = ucctl.FindName("WindowCanvas");
            hostcontext.xaxis = ucctl.FindName("xaxis");
            hostcontext.yaxis = ucctl.FindName("yaxis");
            hostcontext.optimizeCanvas = ucctl.FindName("optimizeCanvas") as OptimizeCanvas;
            //hostcontext.datas_ = (ucctl.FindName("optimizeCanvas") as OptimizeCanvas).GetDatas();
            //hostcontext.dvalues = (ucctl.FindName("optimizeCanvas") as OptimizeCanvas).GetDValues();
            //install handle
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
        public virtual double IntegrateData()
        {
            return 29990;
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
            
            int startx_index = findIndexInDatas_(startx);
            int end_index = findIndexInDatas_(endx);
            int index = startx_index;
            //int index = 0;
            areagon.Points.Add(new Point(hostcontext.datas_[startx_index].X,0));
            while ( index >= startx_index && index < end_index)
            //while (index >= 0 && index < hostcontext.datas_.Count)
            {
                areagon.Points.Add(hostcontext.datas_[index]);
                index++;
            }
            areagon.Points.Add(new Point(hostcontext.datas_[end_index].X, 0));
            //areagon.Points.Add(new Point(hostcontext.datas_[hostcontext.datas_.Count-1].X,0));
        }

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
            commenttx.Text = IntegrateData().ToString();
            Canvas.SetLeft(commenttx,(x + y )/2 - commenttx.Width/2);
            Canvas.SetTop(commenttx, 10);
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
