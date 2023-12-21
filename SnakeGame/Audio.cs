using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SnakeGame
{
    public static class Audio
    {
        public readonly static MediaPlayer GameOver = LoadAudio("GameOver.wav");
        public readonly static MediaPlayer EatApple = LoadAudio("AppleMMM.wav");
        private static MediaPlayer LoadAudio(string filename, bool autoReset = true)
        {
            MediaPlayer player = new();

            player.Open(new Uri($"Assets/{filename}", UriKind.Relative));

            if(autoReset)
            {
                player.MediaEnded += Player_MediaEnded;
            }
            return player;
        }

        private static void Player_MediaEnded(object sender, EventArgs eventArgs)
        {
            MediaPlayer m = sender as MediaPlayer;
            m.Stop();
            m.Position = new TimeSpan(0);
        }

        
    }
}
