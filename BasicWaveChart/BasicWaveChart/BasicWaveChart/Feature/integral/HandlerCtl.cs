using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BasicWaveChart.Feature.integral
{
    class HandleCtl : Canvas
    {
        private HandleCtl RightBrother = null;
        private void init()
        {
            this.Width = 50;
            this.Height = 50;

            ImageBrush brush = new ImageBrush();
            brush.ImageSource = new BitmapImage(new Uri("pillar.png", UriKind.Relative));
            this.Background = brush;

            Canvas.SetLeft((this), 100);
            Canvas.SetTop((this), 200);
            this.Children.Add(new Rectangle());
            (this.Children[0] as Rectangle).Width = 2;
            (this.Children[0] as Rectangle).Height = 50;
            (this.Children[0] as Rectangle).Fill = new SolidColorBrush(Colors.Black);
            Canvas.SetLeft((this.Children[0] as Rectangle), 25);
            Canvas.SetTop((this.Children[0] as Rectangle), -48);

            this.Loaded += delegate (object sender, RoutedEventArgs e)
            {
            };
        }

        public HandleCtl(HandleCtl rightbrother)
        {
            init();
            RightBrother = rightbrother;
            Canvas.SetLeft(RightBrother, Canvas.GetLeft(this) + 100);
            Canvas.SetTop(RightBrother, 200);
        }

        public HandleCtl()
        {
            init();
        }

        public HandleCtl GetBrother()
        {
            return RightBrother;
        }
        public double GetRayX()
        {
            return Canvas.GetLeft(this) + this.Width / 2;
        }
    }
}
