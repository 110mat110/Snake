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

        private List<SnakeBlock> SnakeList = new List<SnakeBlock>();
        private const int GameWidth = 40;
        private const int GameHeight = 20;

        private Apple apple = null;

        public MainWindow() {
            InitializeComponent();
            //Create default small snake in top corner
            SnakeList.Add(new SnakeBlock(new Point(0, 0)));
            SnakeList.Add(new SnakeBlock(new Point(0, 1)));
            SnakeList.Add(new SnakeBlock(new Point(0, 2)));
            //set apple default canvas position
            //create new apple
            apple = new Apple(SnakeList, GameWidth, GameHeight);

            /*Simulating game mechanics. Every 50ms (2fps) there is game tick event*/
            DispatcherTimer gameTicker = new DispatcherTimer {
                Interval = new TimeSpan(0,0,0,0,50)
            };
            gameTicker.Tick += GameTicker_Tick;
            //Start!
            gameTicker.Start();
        }

        /*There you have to move snake and do everything*/
        private void GameTicker_Tick(object sender, EventArgs e) {
            int Xdirection = 0;
            int Ydirection = 0;

            //Read keyboard and set xy derivations
            if (Keyboard.IsKeyDown(Key.Right)) {
                Xdirection = 1;
                Ydirection = 0;
            }
            if (Keyboard.IsKeyDown(Key.Left)) {
                Xdirection = -1;
                Ydirection = 0;
            }
            if (Keyboard.IsKeyDown(Key.Up)){
                Xdirection = 0;
                Ydirection = -1;
            }
            if (Keyboard.IsKeyDown(Key.Down)){
                Xdirection = 0;
                Ydirection = 1;
            }

            //Now we know where to go, so move
            MoveSnakeInDirection(Xdirection, Ydirection);

            //Handle apple
            HandleApple();

            //collision detection
            DetectCollision();

            //Draw game
            DrawGame();
        }

        private void HandleApple() {
            //Check, if snake head is on same position as apple
            if (apple.CheckIfSnakeEats(SnakeList)) {
                //if yes, extend snake by one
                SnakeList.Add(new SnakeBlock(SnakeList[SnakeList.Count - 1].LastPosition));
                //and create another apple. Old one will be forgotten
                apple = new Apple(SnakeList, GameWidth, GameHeight);
            }
        }

        private void DrawGame() {
            //first clear whole canvas
            GameCanvas.Children.Clear();
            //then add each point to canvas
            foreach(var dot in SnakeList) {
                //Genereate rectangle and add it to canvas
                GameCanvas.Children.Add(CreateSnakerectangle(dot.ActualPosition));
            }
            //draw an apple
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

        //check if head is collision with any other snake block. We do not consider wall as colision
        private void DetectCollision() {

            for(int i = 1; i<SnakeList.Count; i++) {
                //if positions are the same that means, that snake crashed
                if (Point.Equals(SnakeList[0].ActualPosition, SnakeList[i].ActualPosition)) {
                    //So show message
                    MessageBox.Show("You lost!");
                    //And close game
                    App.Current.Shutdown();
                }
            }
        }

        private void MoveSnakeInDirection(int xdirection, int ydirection) {
            //If snake is not moving, do not move :P
            if (xdirection == 0 && ydirection == 0) return;

            //SnakeList[0] is head, so take its position
            var headPos = SnakeList[0].ActualPosition;
            //And calculate new position
            headPos.X += xdirection;
            headPos.Y += ydirection;
            //Check, if head is not out of game board. If yes, then come from oposite direction
            if (headPos.X < 0) headPos.X = GameWidth;
            if (headPos.X > GameWidth) headPos.X = 0;

            if (headPos.Y < 0) headPos.Y = GameHeight;
            if (headPos.Y > GameHeight) headPos.Y = 0;

            //Then move head to this position
            SnakeList[0].Move(headPos);

            //And move rest of blocks except head
            for (int i = 1; i < SnakeList.Count; i++) {
                //Set block last position of previous block
                SnakeList[i].Move(SnakeList[i - 1].LastPosition);
            }
        }
    }
}
