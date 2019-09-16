using System.Collections.Generic;
using ArmadaBot.ArmadaBattle.Messages;
using CefSharp.Structs;

namespace ArmadaBot
{
    public static class BotData
    {
        public static bool Start { get; set; } = false;
        public static List<ObjectInitMessageElement> Nearest { get; set; } = new List<ObjectInitMessageElement>();
        public static BotPoint CurrentPosition { get; set; } = new BotPoint(0, 0);
        public static BotPoint MovingPosition { get; set; }
        public static List<ObjectInitMessageElement> RemoveObject { get; set; }
        public static List<ObjectInitMessageElement> LastCollectedItem { get; set; }
        public static BotPoint InitialPosition { get; set; }
    }
}