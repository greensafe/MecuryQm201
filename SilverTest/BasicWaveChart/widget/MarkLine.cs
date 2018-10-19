using BasicWaveChart.Feature.integral;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BasicWaveChart.widget
{
    // mark line param for drawing
    class MarkLine : Canvas
    {
        public dynamic lineinfo;
        public MarkLine()
        {
            lineinfo = new HostContext();
        }
    }
}
