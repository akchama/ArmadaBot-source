using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArmadaBot.ArmadaBattle.Messages;
using SKM.V3.Methods;

namespace ArmadaBot
{
    public static class Client
    {
        public static Dictionary<string, List<ObjectInitMessageElement>> GameObjects = new Dictionary<string, List<ObjectInitMessageElement>>();
        public static int SpeedHackValue { get; set; }
    }
}
