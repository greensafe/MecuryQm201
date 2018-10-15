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
        private static dynamic hostcontext = new HostContext();

        HandleCtl mainhandle;
        private IntegralWorker(BasicWaveChartUC param)
        {
            ucctl = param;
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
            hostcontext.datas_ = (ucctl.FindName("optimizeCanvas") as OptimizeCanvas).GetDatas();
            hostcontext.dvalues = (ucctl.FindName("optimizeCanvas") as OptimizeCanvas).GetDValues();
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
        }

        //面积积分方法可以重写
        public virtual double IntegrateData()
        {
            return 1;
        }

        #region private function
        //make the area polygon
        private void makepg()
        {
            ;
        }

        //show the comment 
        private void showcomment()
        {
            ;
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
