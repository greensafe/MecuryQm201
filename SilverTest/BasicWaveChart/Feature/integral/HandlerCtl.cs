using BasicWaveChart.Feature.basic;
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
    class HandleCtl : Canvas, IMoveFeature
    {
        private HandleCtl RightBrother = null;

        public event MoveDlg Move_Ev;

        private void init()
        {
            this.Width = 20;
            this.Height = 20;

            this.RenderTransform = new TransformGroup();
            

            ImageBrush brush = new ImageBrush();
            //brush.ImageSource = new BitmapImage(new Uri(@"Feature\integral\images\pillar.png", UriKind.Relative));
            //brush.ImageSource = new BitmapImage(new Uri("pillar.png", UriKind.Relative));
            //this.Background = brush;
            

            Canvas.SetLeft((this), 100);
            Canvas.SetTop((this), 200);
            this.Loaded += delegate (object sender, RoutedEventArgs e)
            {
                this.Background = this.FindResource("mybr") as ImageBrush;
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

        public double GetXm()
        {
            double x = Canvas.GetLeft(this);
            return x + this.Width / 2 - 1;
        }

        public double GetYm()
        {
            throw new NotImplementedException();
        }

        public void TriggerMove()
        {
            Move_Ev( Canvas.GetLeft(this) + this.Width / 2 - 1);
        }
    }
}
