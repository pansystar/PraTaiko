using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PraTaiko
{
    class PlayScore
    {
        class Gauge
        {
            public int value { get; private set; }
            private float good;
            private float bad;
            private int upValue;
            public void add(char g)
            {
                switch (g)
                {
                    case 'G':
                        value += upValue;
                        break;
                    case 'g':
                        value += (int)(good * upValue);
                        break;
                    case 'b':
                        value += (int)(bad * upValue);
                        break;
                }
            }
        }
        Gauge gauge;
        public PlayScore()
        {
            gauge = new Gauge();
        }
    }
}
