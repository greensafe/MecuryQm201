using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BasicWaveChart
{
    /// <summary>
    /// BasicWaveChartUC.xaml 的交互逻辑
    /// </summary>
    public partial class BasicWaveChartUC : UserControl
    {
        public string RatioS
        {
            get
            {
                return (string)GetValue(RatioSProperty);
            }
            set
            {
                SetValue(RatioSProperty, value);
            }
        }
        DependencyProperty RatioSProperty = DependencyProperty.Register("RatioS", typeof(string),typeof(BasicWaveChartUC),
            new UIPropertyMetadata("1:1", new PropertyChangedCallback(RatioSChanged)),
                new ValidateValueCallback(RatioSNumber)
        );

        private static bool RatioSNumber(object value)
        {
            return true;
            //throw new NotImplementedException();
        }

        private static void RatioSChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            return;
            //throw new NotImplementedException();
        }

        public BasicWaveChartUC()
        {
            InitializeComponent();
        }

    }
}
