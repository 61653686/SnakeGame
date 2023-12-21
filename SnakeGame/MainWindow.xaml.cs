using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SnakeGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dictionary<GridValue, ImageSource> gridValToImage = new()
        {
            { GridValue.Empty, Images.Empty },
            { GridValue.Snake, Images.Body },
            { GridValue.Food, Images.Food },
            { GridValue.Wall, Images.Wall }
        };

        private readonly Dictionary<Direction, int> dirToRotation = new()
        {
            { Direction.Up, 0 },
            { Direction.Right, 90  },
            { Direction.Down, 180 },
            { Direction.Left, 270 }
        };

        //How big the board is
        private readonly int rows = 15, cols = 15;
        private readonly Image[,] gridImages;
        private GameState gameState;
        private bool gameRunning;
        private List<int> highScores = new();
        private int boostSpeed = 0;
        private Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            gridImages = SetupGrid();
            gameState = new GameState(rows, cols);
        }

        private async Task RunGame()
        {
            Draw();
            await ShowCountDown();
            Overlay.Visibility = Visibility.Hidden;
            await GameLoop();
            await ShowGameOver();
            gameState = new GameState(rows, cols);
        }

        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Overlay.Visibility == Visibility.Visible)
            {
                e.Handled = true;
            }

            if (!gameRunning)
            {
                gameRunning = true;
                await RunGame();
                gameRunning = false;
            }
        }

        private async Task ShowCountDown()
        {
            for (int i = 3; i >= 1; i--)
            {
                OverlayText.Text = i.ToString();
                await Task.Delay(500);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (gameState.GameOver)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.Left:
                    gameState.ChangeDirection(Direction.Left);
                    break;
                case Key.Right:
                    gameState.ChangeDirection(Direction.Right);
                    break;
                case Key.Up:
                    gameState.ChangeDirection(Direction.Up);
                    break;
                case Key.Down:
                    gameState.ChangeDirection(Direction.Down);
                    break;
                case Key.A:
                    gameState.ChangeDirection(Direction.Left);
                    break;
                case Key.D:
                    gameState.ChangeDirection(Direction.Right);
                    break;
                case Key.W:
                    gameState.ChangeDirection(Direction.Up);
                    break;
                case Key.S:
                    gameState.ChangeDirection(Direction.Down);
                    break;
                case Key.Space:
                    boostSpeed = (boostSpeed == 0) ? GameSettings.BoostSpeed : 0;
                    break;

            }
        }

        private async Task GameLoop()
        {
            while (!gameState.GameOver)
            {
                await Task.Delay(100-boostSpeed);
                //How fast the snake moves (Higher is slower)
                gameState.Move();
                Draw();
            }
        }

        private Image[,] SetupGrid()
        {
            Image[,] images = new Image[rows, cols];
            GameGrid.Rows = rows;
            GameGrid.Columns = cols;
            GameGrid.Width = GameGrid.Height * (cols / (double)rows);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Image image = new Image
                    {
                        //How broken turning is
                        Source = Images.Empty,
                        RenderTransformOrigin = new Point(0.5, 0.5)
                    };

                    images[r, c] = image;
                    GameGrid.Children.Add(image);
                }
            }

            return images;
        }

        private void Draw()
        {
            DrawGrid();
            DrawSnakeHead();
            ScoreText.Text = $"SCORE {gameState.Score}";
        }

        private void DrawGrid()
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    GridValue gridVal = gameState.Grid[r, c];
                    gridImages[r, c].Source = gridValToImage[gridVal];
                    gridImages[r, c].RenderTransform = Transform.Identity;
                }
            }

        }

        private void DrawSnakeHead()
        {
            Position headPos = gameState.HeadPosition();
            Image image = gridImages[headPos.Row, headPos.Col];
            image.Source = Images.Head;

            int rotation = dirToRotation[gameState.Dir];
            image.RenderTransform = new RotateTransform(rotation);
        }
        private async Task DrawDeadSnake()
        {
            List<Position> positions = new List<Position>(gameState.SnakePositions());

            for (int i = 0; i < positions.Count; i++)
            {
                Position pos = positions[i];
                ImageSource source = (i == 0) ? Images.DeadHead : Images.DeadBody;
                gridImages[pos.Row, pos.Col].Source = source;
                await Task.Delay(Math.Max(50-(i*2),10));
            }
        }


        private async Task ShowGameOver()
        {
            GameSettings.BoostSpeed = 0;
            ShakeWindow(GameSettings.ShakeDuration);

            Audio.GameOver.Play();
            await DrawDeadSnake();
            await Task.Delay(1000);
            Overlay.Visibility = Visibility.Visible;
            OverlayText.Text = "USE WASD OR ARROW KEYS TO MOVE";
            HighScoreCutScene();
            UpdateLeaderboard();
        }

        private void UpdateLeaderboard()
        {
            highScores.Add(gameState.Score);
            highScores.Sort();
            highScores.Reverse();

            if (highScores.Count > 5)
            {
                highScores.RemoveAt(5);
            }

            HighScore.Text = "High-Score";

            foreach (int score in highScores)
            {
                HighScore.Text += $"\n{score}";
            }
        }

        private async Task ShakeWindow(int durationMs)
        {
            var oLeft = this.Left;
            var oTop = this.Top;
            var shakeTimer = new DispatcherTimer(DispatcherPriority.Send);

            shakeTimer.Tick += (sender, eventArgs) =>
            {
                this.Left = oLeft = random.Next(300, 320);
                this.Top = oTop = random.Next(200, 220);

            };

            shakeTimer.Interval = TimeSpan.FromMilliseconds(100);
            shakeTimer.Start();

            await Task.Delay(durationMs);
            shakeTimer.Stop();
        }   

        private async void HighScoreCutScene()
        {
            for(int i = 0; i < 40; i++)
            {
                HighScore.Margin = new Thickness(HighScore.Margin.Left, HighScore.Margin.Top, HighScore.Margin.Right - 10, HighScore.Margin.Bottom);
                await Task.Delay(20);
            }
            await Task.Delay(1500);
            for (int i = 0; i < 40; i++)
            {
                HighScore.Margin = new Thickness(HighScore.Margin.Left, HighScore.Margin.Top, HighScore.Margin.Right + 10, HighScore.Margin.Bottom);
                await Task.Delay(20);
            }

        }

    }
}
