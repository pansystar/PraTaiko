using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PraTaiko
{
    public class ResultData
    {
        public int score { get; private set; }
        public void setScore(int s)
        {
            score += s;
        }

        public int gauge { get; private set; }
        public void setGauge(int g)
        {
            gauge = g;
        }

        public int great { get; private set; }
        public int good { get; private set; }
        public int bad { get; private set; }
        public int roll { get; private set; }
        public int maxCombo { get; private set; }
        public ResultData()
        {            
            score = 0;
            gauge = 0;
            great = 0;
            good = 0;
            bad = 0;
            roll = 0;
            maxCombo = 0;
        }
    }
}
