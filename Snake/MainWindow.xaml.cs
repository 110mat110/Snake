using Snake.Neural;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

namespace Snake {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private Game game;
        private const int GameWidth = 60;
        private const int GameHeight = 30;
        private const int NoSnakes = 10;
        DispatcherTimer gameTicker;
        private const int NoRounds = 10000;
        private int ActualRound = 0;

        public MainWindow() {
            InitializeComponent();

            game = new Game(GameWidth, GameHeight, NoSnakes);

            /*Simulating game mechanics. Every 50ms (2fps) there is game tick event*/
            gameTicker = new DispatcherTimer {
                Interval = new TimeSpan(1)
            };
            gameTicker.Tick += GameTicker_Tick;

            DrawGame();
        }

        /*There you have to move snake and do everything*/
        private void GameTicker_Tick(object sender, EventArgs e) {
            try {
                game.GameTick();
            } catch (Exception) {
                ResetGame();
            }
            if (game.RunningSnakes <= 1 || game.GameRounds> NoRounds) {
                ResetGame();
            }

            //Draw game
            if(gameTicker.IsEnabled)
                DrawGame();
        }

        private void ResetGame() {
            ActualRound++;

            Debug.WriteLine("BF: " + game.BestFitness);

            Round.Content = "Round: " + ActualRound;
            BestFitenss.Content = "BF: " + game.BestFitness;
            var bestSnake = game.GetAllSnakes().Where(x => x.Fitness == game.BestFitness).First();

            //MessageBox.Show("game ended! best fitness is " + game.BestFitness);
            game = new Game(GameWidth, GameHeight, NoSnakes, game.GetAllSnakes().OrderByDescending(x => x.Fitness).Take(5).ToList());
        }

        private void DrawGame() {
            //first clear whole canvas
            GameCanvas.Children.Clear();
            //then add each point to canvas
                foreach(var dot in game.GetAllSnakeDots())
                //Genereate rectangle and add it to canvas
                GameCanvas.Children.Add(CreateSnakerectangle(dot.ActualPosition));
            //draw an apple
            foreach(var apple in game.GetApplesDots())
            GameCanvas.Children.Add(CreateApple(apple.Position));

        }

        private UIElement CreateSnakerectangle(Point directions) {
            //To universaly determine one rectangle size
            var width = GameCanvas.Width / GameWidth;
            var height = GameCanvas.Height / GameHeight;
            //create rectangle with this atributes
            var rect = new Rectangle {
                Stroke = new SolidColorBrush(Colors.Black),
                Fill = new SolidColorBrush(Colors.Black),
                Width = width,
                Height = height,
                StrokeThickness = 1
            };
            //Set rectangle and align it by top left corner
            Canvas.SetLeft(rect, directions.X * width);
            Canvas.SetTop(rect, directions.Y * height);
            return rect;
        }

        private UIElement CreateApple(Point directions) {
            //To universaly determine one rectangle size
            var width = GameCanvas.Width / GameWidth;
            var height = GameCanvas.Height / GameHeight;
            //create rectangle with this atributes
            var rect = new Rectangle {
                Stroke = new SolidColorBrush(Colors.Red),
                Fill = new SolidColorBrush(Colors.Red),
                Width = width,
                Height = height,
                StrokeThickness = 1
            };
            //Set rectangle and align it by top left corner
            Canvas.SetLeft(rect, directions.X * width);
            Canvas.SetTop(rect, directions.Y * height);
            return rect;
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            gameTicker.Stop();
            while (ActualRound % 100 != 0) {
                GameTicker_Tick(this, new EventArgs());
            }

            gameTicker.Start();
        }
    }
}
