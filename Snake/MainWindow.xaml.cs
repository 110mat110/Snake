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

        List<Game> gameList = new List<Game>();

        private const int GameWidth = 60;
        private const int GameHeight = 30;
        private const int NoSnakes = 5;
        private const int NoGames = 200;
        DispatcherTimer gameTicker;
        private const int NoRounds = 2000;
        private int ActualRound = 0;
        private const double MutateRatio = 1;

        private Random globalRandom;
        private int skiprounds = 200;

        public MainWindow() {
            InitializeComponent();
            globalRandom = new Random();
            for (int i =0; i<NoGames; i++)
                gameList.Add(new Game(GameWidth, GameHeight, NoSnakes, globalRandom));

            /*Simulating game mechanics. Every 50ms (2fps) there is game tick event*/
            gameTicker = new DispatcherTimer {
                //Interval = new TimeSpan(0, 0, 0, 0, 50)
                Interval = new TimeSpan(1)
            };
            gameTicker.Tick += GameTicker_TickAsync;

            DrawGame(gameList[0]);
        }

        /*There you have to move snake and do everything*/
        private void GameTicker_TickAsync(object sender, EventArgs e) {
            bool AtLeastOneGamesActive = false;

            foreach (var game in gameList) {
                if (game.Active) {
                    game.GameTick();
                }
            }

            foreach (var game in gameList) {
                //if (game.GameRounds > NoRounds) {
                if (game.RunningSnakes <= 1 || game.GameRounds> NoRounds) {
                    game.Active = false;
                }

                if (game.Active)
                    AtLeastOneGamesActive = true;
            }

            //Draw game
            if(gameTicker.IsEnabled)
                DrawGame(gameList[0]);

            if (!AtLeastOneGamesActive)
                ResetGame();
        }

        private async Task FastLoop() {

            List<Task> tasks = new List<Task>();
            foreach (var game in gameList) {
                    tasks.Add(game.FastLoop(NoRounds));
            }
            await Task.WhenAll(tasks);

            Debug.WriteLine("Rounds: " + gameList[0].GameRounds);
            ResetGame();
        }

        private void ResetGame() {
            ActualRound++;
            Round.Content = "Round: " + ActualRound;

            List<SnakeObject> allSnakes = new List<SnakeObject>();
            foreach (var game in gameList)
                allSnakes.AddRange(game.GetAllSnakes());

            double bestFitnes = allSnakes.OrderByDescending(x => x.Fitness).First().Fitness;
            BestFitenss.Content = "BF: " + bestFitnes;
            Debug.WriteLine("RND " + ActualRound + " BF: " + bestFitnes);

            //MessageBox.Show("game ended! best fitness is " + game.BestFitness);
            gameList.Clear();
            for(int i = 0; i<NoGames; i++)
                gameList.Add(new Game(GameWidth, GameHeight, NoSnakes, allSnakes.OrderByDescending(x => x.Fitness).Take(NoSnakes).ToList(), globalRandom, MutateRatio));
        }

        private void DrawGame(Game game) {
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

        private async void Button_Click(object sender, RoutedEventArgs e) {
            gameTicker.Stop();
            while (ActualRound % skiprounds != 0) {
                await FastLoop();
            }

            gameTicker.Start();
        }
    }
}
