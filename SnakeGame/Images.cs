using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SnakeGame
{
    public static class Images
    {
        public readonly static ImageSource Empty = LoadImage("Empty.png");
        public readonly static ImageSource Body = LoadImage("scales.png");
        public readonly static ImageSource Head = LoadImage("likeahead.png");
        public readonly static ImageSource Food = LoadImage("Appel.png");
        public readonly static ImageSource DeadBody = LoadImage("DeadBody.png");
        public readonly static ImageSource DeadHead = LoadImage("DeadHead.png");
        public readonly static ImageSource Wall = LoadImage("Wall.jpg");

        private static ImageSource LoadImage(string fileName)
        {
            return new BitmapImage(new Uri($"Assets/{fileName}", UriKind.Relative));
        }
    }
}
