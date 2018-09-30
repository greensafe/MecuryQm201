using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BasicWaveChart.widget
{
    class OptimizeCanvas:Canvas
    {
        //effective point that can draw
        double effectiveW = 0.5;
        double effectiveH = 0.5;

        public bool enableOptimize
        {
            get;set;
        }
    }
}
