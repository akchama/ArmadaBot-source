using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmadaBot
{
    public class BotPoint
    {
        public long? X { get; set; }
        public long? Y { get; set; }

        public BotPoint(long? x, long? y)
        {
            X = x;
            Y = y;
        }
    }
}
