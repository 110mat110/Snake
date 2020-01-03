using Snake.Neural;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Snake {
    public class SnakeObject {
        public List<SnakeBlock> SnakeList = new List<SnakeBlock>();

        private int GameWidth { get; set; }
        private int GameHeight { get; set; }

        public bool Active { get; private set; }

        public int Fitness { get { return Active? SnakeList.Count: SnakeList.Count-5; } }
        public NeuralNetwork Brain;

        private int LastMoveX = 0;
        private int LastMoveY = 1;

        public SnakeObject(int gameWidth, int gameHeight) {
            //TODO brainfuck ako sa vola kontruktor z ineho?
            new SnakeObject(gameWidth, gameHeight, new Point(new Random().Next(0,GameWidth), new Random().Next(0, GameHeight)));
        }

        public SnakeObject(int gameWidth, int gameHeight, Point firstPosition) {
            Active = true;
            GameWidth = gameWidth;
            GameHeight = gameHeight;
            SnakeList.Add(new SnakeBlock(firstPosition));

            Brain = new NeuralNetwork(5, new Random());
        }

        public SnakeObject(int gameWidth, int gameHeight, Random randomizer) {
            Active = true;
            GameWidth = gameWidth;
            GameHeight = gameHeight;
            SnakeList.Add(new SnakeBlock(new Point(randomizer.Next(0, GameWidth), randomizer.Next(0, GameHeight))));

            Brain = new NeuralNetwork(5, randomizer);
        }

        public SnakeObject(int gameWidth, int gameHeight, Random randomizer, NeuralNetwork brain) {
            Active = true;
            GameWidth = gameWidth;
            GameHeight = gameHeight;
            SnakeList.Add(new SnakeBlock(new Point(randomizer.Next(0, GameWidth), randomizer.Next(0, GameHeight))));

            Brain = brain;
        }

        public void MoveSnakeInDirection(int xdirection, int ydirection) {
            if (!Active) return;
            //If snake is not moving, do not move :P
            if (xdirection == 0 && ydirection == 0) {
                MoveSnakeInDirection(LastMoveX, LastMoveY);
                return;
            }
            LastMoveX = xdirection;
            LastMoveY = ydirection;
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

        public void RandomMoveSnake(Random randomizer) {
            MoveSnakeInDirection(randomizer.Next(3) - 1, randomizer.Next(3) - 1);
        }

        public void UseBrainToMove(List<SnakeObject> snakes, List<Apple> apples) {
            Point nearestApple = FindNearestApple(apples);
            var nearestSnakes = FindXNearestSnakes(snakes, 2);

            //Xapple Yapple Xenemy1 Yenemy1 Xenemy2 Yenemy2
            List<double> inputs = new List<double>() {
                nearestApple.X,
                nearestApple.Y
            };
            foreach(var x in nearestSnakes) {
                inputs.Add(x.X);
                inputs.Add(x.Y);
            }

            var moves = DecodeMove(Brain.DoNeuralStuff(inputs));

            MoveSnakeInDirection(moves[0], moves[1]);
        }

        private int[] DecodeMove(List<double> move) {
            if (move[0] < 0.25) return new int[] { -1, 0 };
            if (move[0] >= 0.25 && move[0] < 0.5) return new int[] { 1, 0 };
            if (move[0] >= 0.50 && move[0] < 0.75) return new int[] { 0, -1 };
            if (move[0] >= 0.75) return new int[] { 0, 1 };

            Debug.WriteLine("Unexpected move " + move[0]);
            return new int[] { 0, 0 };

            /*
            List<int> outputs = new List<int>();
            foreach(var m in move) {
                int o = 0;
                if (m < 0.4) o = -1; 
                if (m > 0.6) o = +1;
                outputs.Add(o);
            }
            return outputs;
            */
        }

        private Point FindNearestApple(List<Apple> apples) {
            double distance = double.MaxValue;
            Point p = apples[0].Position; 
            foreach(var apple in apples) {
                if (Distance(apple.Position, SnakeList[0].ActualPosition) < distance)
                    p = apple.Position;
            }
            return p;
        }

        private List<Point> FindXNearestSnakes(List<SnakeObject> Snakes, int capacity) {

            List<Point> points = new List<Point>();
            foreach (var s in Snakes) {
                if (s.Active && !s.Equals(this))
                    foreach (var sp in s.SnakeList)
                        points.Add(sp.ActualPosition);

            }

            return points.OrderBy(point => Distance(SnakeList[0].ActualPosition, point)).Take(capacity).ToList();
        }

        private double Distance(Point p1, Point p2) {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        public void DetectColision(List<SnakeObject> snakeList, List<Apple> apples) {
            foreach (SnakeObject s in snakeList) {
                if (!s.Equals(this) && s.Active) {
                    foreach (var snakeBlock in s.SnakeList) {
                        //if positions are the same that means, that snake crashed
                        if (Point.Equals(this.SnakeList[0].ActualPosition, snakeBlock.ActualPosition)) {
                            this.Active = false;
                            foreach(var sb in this.SnakeList) {
                                apples.Add(new Apple(sb.ActualPosition));
                            }
                        }
                    }
                }
            }
        }

        public void EatApple(List<Apple> apples) {
            List<Apple> applesToRemoveList = new List<Apple>();
            foreach (var apple in apples) {
                if (Point.Equals(this.SnakeList[0].ActualPosition, apple.Position)){
                    applesToRemoveList.Add(apple);
                    ExtendSnake();
                }
            }
            foreach (var apple in applesToRemoveList) {
                apples.Remove(apple);
            }
        }

        private void ExtendSnake() {
            SnakeList.Add(new SnakeBlock(SnakeList[SnakeList.Count - 1].LastPosition));
        }
    }
}
