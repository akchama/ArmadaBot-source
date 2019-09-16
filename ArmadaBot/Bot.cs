using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmadaBot
{
    class Bot
    {
        public static void Log(string message)
        {
            try
            {
                Form1.form1.Invoke(Form1.form1.writer, new string[]
                {
                    message
                });
            }
            catch (Exception)
            {
            }
        }

        public static void ExecuteJavaScript()
        {
            try
            {
                Form1.form1.Invoke(Form1.form1.scriptrunner, new object[]
                {
                    
                });
            }
            catch (Exception)
            {
            }
        }

        public static IEnumerable<Point> Coordinates(Point start,
            Point end,
            bool includeFirstPoint = false)
        {
            int gridSize = 40 + 40 * Client.SpeedHackValue;
            int dx = Math.Sign(end.X - start.X);
            int dy = Math.Sign(end.Y - start.Y);
            double steps = Math.Max(Math.Abs(end.X - start.X), Math.Abs(end.Y - start.Y)) +
                           (includeFirstPoint ? 1 : 0);

            double x = start.X + (includeFirstPoint ? 0 : dx * gridSize);
            double y = start.Y + (includeFirstPoint ? 0 : dy * gridSize);

            for (int i = 1; i <= steps / gridSize; ++i)
            {
                yield return new Point(x, y);

                x = x == end.X ? end.X : x + dx * gridSize;
                y = y == end.Y ? end.Y : y + dy * gridSize;
            }
        }

        public static bool Running { get; set; }
    }

    public struct Point
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
