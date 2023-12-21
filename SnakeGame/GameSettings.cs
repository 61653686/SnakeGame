using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame
{
    internal class GameSettings
    {
        public static int ShakeDuration { get; set; } = 2000;
        public static int BoostSpeed { get; set; } = 50;
        public static double WallDensity { get; set; } = .05;
        public static bool WallFatality { get; set; } = true;
    }
}
