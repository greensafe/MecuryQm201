using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BasicWaveChart.Feature.integral
{
    public class IntegralFeature: DependencyObject
    {
        private static BasicWaveChartUC hostctl = null;
        private static dynamic hostcontext = new HostContext();

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
            hostctl = sender as BasicWaveChartUC;        
            hostcontext.window = hostctl.WindowCanvas;   //canvas
            hostcontext.container = hostctl.basecanvas;  //canvas
            Console.WriteLine(hostcontext.window.Width);
            Console.WriteLine(hostcontext.window.Height);
        }
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
