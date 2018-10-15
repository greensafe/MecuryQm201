using BasicWaveChart.Feature.basic;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace featurefactory.Basic
{
    class MoveFeature:DependencyObject
    {
        //static dynamic target;

        //connector
        public static readonly DependencyProperty EnableProperty = DependencyProperty.RegisterAttached("Enable", typeof(bool),
            typeof(MoveFeature), new PropertyMetadata(false, new PropertyChangedCallback(OnEnableChanged))
            );

        private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //target = d;
            /*findParent(d as FrameworkElement).Loaded += delegate (object sender, RoutedEventArgs epr) {
                MoveWorker.Create(d).Enable();
            };*/
            (d as FrameworkElement).Loaded += delegate (object sender, RoutedEventArgs epr) {
                MoveWorker.Create(d).Enable();
            };

        }
        public static void SetEnble(DependencyObject d, bool use)
        {
            d.SetValue(EnableProperty, use);
        }

        #region private func
        static private Window findParent(FrameworkElement me)
        {
            while (!(me is Window))
            {
                me = me.Parent as FrameworkElement;
            }
            return me as Window;
        }
        #endregion
    }

    internal class MoveWorker
    {
        dynamic target;
        dynamic targetcontext = new TargertContext();
        private bool moving = false;

        private MoveWorker()
        {

        }
        static public MoveWorker Create(dynamic target)
        {
            MoveWorker worker = new MoveWorker();
            worker.target = target;
            return worker;
        }
        //enable the feature
        public void Enable()
        {
            //targetcontext.container = target.FindName("container") as Canvas;
            targetcontext.container = target.Parent as Canvas;
            target.MouseLeftButtonDown += new MouseButtonEventHandler(MouseleftdownHdlr);
            target.MouseLeftButtonUp += new MouseButtonEventHandler(MouseLeftUpHdlr);
            target.MouseMove += new MouseEventHandler(MouseMoveHdlr);
        }

        private void MouseMoveHdlr(object sender, MouseEventArgs e)
        {
            //only send x 
            double x;
            if (moving == true)
            {
                //only move in horital direction
                x = e.GetPosition(targetcontext.container).X - target.ActualWidth / 2;
                Canvas.SetLeft(target, x);
                //Canvas.SetTop(target, e.GetPosition(targetcontext.container).Y -target.ActualHeight / 2);

                //trigger move event
                (sender as IMoveFeature).TriggerMove();
                return;
            }
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                moving = true;
                target.CaptureMouse();
            }
        }

        private void MouseleftdownHdlr(object sender, MouseButtonEventArgs e)
        {

        }

        private void MouseLeftUpHdlr(object sender, MouseButtonEventArgs e)
        {
            moving = false;
            target.ReleaseMouseCapture();
        }
    }

    internal class TargertContext : DynamicObject
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
