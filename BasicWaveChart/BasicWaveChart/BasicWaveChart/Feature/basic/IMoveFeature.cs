using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicWaveChart.Feature.basic
{
    delegate void MoveDlg(double pos);
    interface IMoveFeature
    {
        //get the middle x position of control
        double GetXm();
        //get the middle y position of control
        double GetYm();

        //event
        //move event define
        event MoveDlg Move_Ev;
        void TriggerMove();
    }
}
