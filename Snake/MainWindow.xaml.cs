using Snake.Neural;
using Snake.Properties;
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
        DispatcherTimer gameTicker;
        DispatcherTimer manualTimer;
        private int ActualRound = 0;

        private Random globalRandom;

        public MainWindow() {
            InitializeComponent();

            RoundProgress.Maximum = Settings.Default.NoRounds;
            globalRandom = new Random();
            for (int i =0; i<Settings.Default.NoGamesAtOnce; i++)
                gameList.Add(new Game(globalRandom));

            manualTimer = new DispatcherTimer {
                Interval = new TimeSpan(0, 0, 0, 0, 100)
            };
            manualTimer.Tick += ManualTimer_Tick;
            manualTimer.Start();
            /*Simulating game mechanics. Every 50ms (2fps) there is game tick event*/
            gameTicker = new DispatcherTimer {
                Interval = new TimeSpan(1)
            };
            gameTicker.Tick += GameTicker_TickAsync;

            DrawGame(gameList.Where(x=> x.Active).First());
        }

        private void ManualTimer_Tick(object sender, EventArgs e) {
            gameList[0].GetBestSnake().TurnSnake(Keyboard.IsKeyDown(Key.Left), Keyboard.IsKeyDown(Key.Right));

            var values = Sensors.FindAppleDirectionaly(gameList[0].Apples, gameList[0].GetBestSnake());

            Debug.WriteLine(values[0] + " " + values[1] + " " + values[2] + " " + values[3]);
            DrawGame(gameList[0]);
        }

        /*There you have to move snake and do everything*/
        private void GameTicker_TickAsync(object sender, EventArgs e) {

            foreach (var game in gameList) {
                if (game.Active) {
                    game.GameTick();
                }
            }

            foreach (var game in gameList) {
                //if (game.GameRounds > NoRounds) {
                if (Settings.Default.NoRounds > -1) {
                    if (game.RunningSnakes > 2) {
                        if (game.RunningSnakes <= 1 || game.GameRounds > Settings.Default.NoRounds) {
                            game.Active = false;
                        }
                    } else {
                        if (game.GameRounds > Settings.Default.NoRounds) {
                            game.Active = false;
                        }
                    }
                } else {
                    if (game.RunningSnakes <= 1) {
                        game.Active = false;
                    }
                }


            }

            if (gameList.Where(x => x.Active).Count() > 0) {
                if (gameTicker.IsEnabled) {
                    DrawGame(gameList.Where(x => x.Active).First());
                    DrawNeuralNetwork(gameList.Where(x => x.Active).First().GetBestSnake().Brain);
                }
                //Draw game

            }else
                ResetGame();

        }

        private async Task FastLoop() {

            List<Task> tasks = new List<Task>();
            foreach (var game in gameList) {
                    tasks.Add(game.FastLoop(Settings.Default.NoRounds));
            }
            await Task.WhenAll(tasks);

            Debug.WriteLine("Rounds: " + gameList.OrderByDescending(x=>x.GetAllSnakes().Max(y=>y.Fitness)).First().GameRounds);
            RoundProgress.Maximum = Settings.Default.skipRoudns;
            RoundProgress.Value = ActualRound % Settings.Default.skipRoudns;
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

            var snakes = Mutator.MutateForOneSnakeGame(allSnakes, globalRandom);

            foreach (var snake in snakes) {
                gameList.Add(new Game(new List<SnakeObject>() { snake }, globalRandom, false));
            }
            /*
            for(int i = 0; i<NoGamesAtOnce; i++)
                gameList.Add(new Game(GameWidth, GameHeight, NoSnakes, allSnakes.OrderByDescending(x => x.Fitness).Take(NoSnakes).ToList(), globalRandom, MutateRatio, penalty, AppleTimeSpan, NoRounds, Treshold));
            */
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

            RoundProgress.Maximum = Settings.Default.NoRounds;
            RoundProgress.Value = game.GameRounds;
        }

        private UIElement CreateSnakerectangle(Point directions) {
            //To universaly determine one rectangle size
            var width = GameCanvas.Width / Settings.Default.GameWidth;
            var height = GameCanvas.Height / Settings.Default.GameHeight;
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
            var width = GameCanvas.Width / Settings.Default.GameWidth;
            var height = GameCanvas.Height / Settings.Default.GameHeight;
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
            manualTimer.Stop();
            ((Button)sender).Content = "Fast Forward";

            gameTicker.Stop();
            while (ActualRound % Settings.Default.skipRoudns != 0) {
                await FastLoop();
            }

            gameTicker.Start();
        }

        private void DrawNeuralNetwork(NeuralNetwork nn) {

            neuralCanvas.Children.Clear();
            int height = nn.NeuronLayers.Max(x => x.Count);
            int width = nn.NeuronLayers.Count;

            double verticalDifference = (neuralCanvas.Height) / height;
            double horizontalDifference = (neuralCanvas.Width - 40) / width;
            int i = 0;

            foreach (var layer in nn.Connections) {
                foreach (Connection connection in layer) {
                    var rect = CreateConnection(connection, i * verticalDifference +5, (i + 1) * verticalDifference + 5, connection.DestinationNeuronIndex * horizontalDifference + 5, connection.SourceNeuronIndex * horizontalDifference + 5);
                    neuralCanvas.Children.Add(rect);
                }
                i++;
            }
            i = 0;
            foreach (var layer in nn.NeuronLayers) {
                int j = 0;
                foreach (var neuron in layer) {
                    var rect = CreateNeuron(neuron);
                    Canvas.SetTop(rect, j*horizontalDifference);
                    Canvas.SetLeft(rect, i*verticalDifference);
                    neuralCanvas.Children.Add(rect);
                    j++;
                }
                i++;
            }
            i = 0;
            foreach (var neuron in nn.NeuronLayers[nn.NeuronLayers.Count-1]) {
                var rect = CreateText(neuron);
                Canvas.SetLeft(rect, nn.NeuronLayers.Count * horizontalDifference);
                Canvas.SetTop(rect, i * verticalDifference);
                neuralCanvas.Children.Add(rect);
                i++;
            }

        }

        private UIElement CreateNeuron(Neuron neuron) {
            Color color = new Color() {
                A = 255,
                R = (byte)((1 - neuron.LastOutput) * 255),
                B = (byte)((1 - neuron.LastOutput) * 255),
                G = 255
            };

            //create circle with this atributes
            var rect = new Ellipse {
                Stroke = new SolidColorBrush(Colors.Black),
                Fill = new SolidColorBrush(color),
                Width = 10,
                Height = 10,
                StrokeThickness = 1
            };
            return rect;
        }
        private UIElement CreateText(Neuron neuron) {

            TextBlock textBlock = new TextBlock();
            textBlock.Text = string.Format("{0:0.##}", neuron.LastOutput);

            textBlock.Foreground = new SolidColorBrush(Colors.Black);
            return textBlock;
        }

        private UIElement CreateConnection(Connection c, double X1, double X2, double Y1, double Y2) {

            var line = new Line {
                Stroke = c.Weight > 0
                ? new SolidColorBrush(new Color() {
                    A = 255,
                    R = 255,
                    G = (byte)(255 *(1- c.Weight)),
                    B = (byte)(255 * (1 - c.Weight)),
                })
                : new SolidColorBrush(new Color() {
                    A = 255,
                    R = (byte)Math.Abs((255 * (1 - c.Weight))),
                    G = (byte)Math.Abs((255 * (1 - c.Weight))),
                    B = (byte)255,
                }),

                X1 = X1,
                X2 = X2,
                Y1 =Y1,
                Y2 = Y2,
                StrokeThickness = 2
            };
            //Set rectangle and align it by top left corner
            return line;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            new SettingsWindow().Show();
        }
    }
}
