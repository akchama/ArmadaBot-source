using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SKM.V3.Models;

namespace ArmadaBot
{
    public static class BotSession
    {
        public static bool lostConnection;
        public static string Username { get; set; }
        public static string Password { get; set; }
        public static DateTime sessionStartTime;
        public static double PlayerHP;
        public static int CollectedDiamonds { get; set; }
        public static int CollectedGlows { get; set; }
        public static bool Collecting { get; set; }
        public static CreateKeyResult newTrialKey { get; set; }
        public static string Playername { get; set; }
        public static string PlayerGold { get; set; }
        public static string PlayerDiamond { get; set; }
        public static string PlayerExp { get; set; }
        public static int CollectedElixir { get; set; }
    }
}
