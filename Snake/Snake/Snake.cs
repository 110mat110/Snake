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
        public List<SnakeBlock> CurrentSnakeBlocks = new List<SnakeBlock>();

        private int GameWidth { get; set; }
        private int GameHeight { get; set; }

        public bool Active { get; private set; }

        public int Fitness { get { return Active ? CurrentSnakeBlocks.Count : CurrentSnakeBlocks.Count - Penalty; } }
        public NeuralNetwork Brain;

        public int Penalty { private get; set; }
        public double Treshold { private get; set; }
        private int LastMoveX = 1;
        private int LastMoveY = 0;

        public SnakeObject(int gameWidth, int gameHeight, Point firstPosition, Random random, int penalty, double treshold) {
            Active = true;
            GameWidth = gameWidth;
            GameHeight = gameHeight;
            CurrentSnakeBlocks.Add(new SnakeBlock(firstPosition));
            Penalty = penalty;
            Treshold = treshold;

            Brain = new NeuralNetwork(new List<int>() { 10 }, random);
        }

        public SnakeObject(int gameWidth, int gameHeight, Random randomizer, int penalty, double treshold) {
            Active = true;
            GameWidth = gameWidth;
            GameHeight = gameHeight;
            CurrentSnakeBlocks.Add(new SnakeBlock(new Point(randomizer.Next(0, GameWidth), randomizer.Next(0, GameHeight))));
            Penalty = penalty;
            Treshold = treshold;

            Brain = new NeuralNetwork(new List<int>() { 10 }, randomizer);
        }

        public SnakeObject(int gameWidth, int gameHeight, Random randomizer, NeuralNetwork brain, int penalty, double treshold) {
            Active = true;
            GameWidth = gameWidth;
            GameHeight = gameHeight;
            CurrentSnakeBlocks.Add(new SnakeBlock(new Point(randomizer.Next(0, GameWidth), randomizer.Next(0, GameHeight))));
            Penalty = penalty;
            Treshold = treshold;

            Brain = brain;
        }

        public void MoveSnakeInDirection(Vector directionVector) {
            MoveSnakeInDirection((int)directionVector.X, (int)directionVector.Y);
        }

        public void MoveSnakeInDirection(int xdirection, int ydirection) {
            if (!Active) return;
            //If snake is not moving, do not move :P
            if (xdirection == 0 && ydirection == 0) {
                if (LastMoveX == 0 && LastMoveY == 0)
                    return;
                MoveSnakeInDirection(LastMoveX, LastMoveY);
                return;
            }
            LastMoveX = xdirection;
            LastMoveY = ydirection;
            //SnakeList[0] is head, so take its position
            var headPos = CurrentSnakeBlocks[0].ActualPosition;
            //And calculate new position
            headPos.X += xdirection;
            headPos.Y += ydirection;
            //Check, if head is not out of game board. If yes, then come from oposite direction
            if (headPos.X < 0) headPos.X = GameWidth;
            if (headPos.X > GameWidth) headPos.X = 0;

            if (headPos.Y < 0) headPos.Y = GameHeight;
            if (headPos.Y > GameHeight) headPos.Y = 0;

            //Then move head to this position
            CurrentSnakeBlocks[0].Move(headPos);

            //And move rest of blocks except head
            for (int i = 1; i < CurrentSnakeBlocks.Count; i++) {
                //Set block last position of previous block
                CurrentSnakeBlocks[i].Move(CurrentSnakeBlocks[i - 1].LastPosition);
            }
        }

        public void RandomMoveSnake(Random randomizer) {
            MoveSnakeInDirection(randomizer.Next(3) - 1, randomizer.Next(3) - 1);
        }

        public void UseBinaryBrainToMove(List<Apple> apples, object locker) {
            bool[] inputs = new bool[] { false, false, false, false };
            if (apples.Where(x => (x.Position.X == CurrentSnakeBlocks[0].ActualPosition.X) && x.Position.Y > CurrentSnakeBlocks[0].ActualPosition.Y).Count() > 0) inputs[0] = true;
            if (apples.Where(x => (x.Position.X == CurrentSnakeBlocks[0].ActualPosition.X) && x.Position.Y < CurrentSnakeBlocks[0].ActualPosition.Y).Count() > 0) inputs[1] = true;
            if (apples.Where(x => (x.Position.Y == CurrentSnakeBlocks[0].ActualPosition.Y) && x.Position.X > CurrentSnakeBlocks[0].ActualPosition.X).Count() > 0) inputs[2] = true;
            if (apples.Where(x => (x.Position.Y == CurrentSnakeBlocks[0].ActualPosition.Y) && x.Position.X < CurrentSnakeBlocks[0].ActualPosition.X).Count() > 0) inputs[3] = true;

            var moves = DecodeMove(Brain.DoNeuralStuff(inputs), Treshold);

            MoveSnakeInDirection(moves);
        }

        public void UseBrainToMove(List<Apple> apples, object locker) {
            double[] inputs = new double[] { 0, 0, 0, 0 };
            if (apples.Where(x => (x.Position.X == CurrentSnakeBlocks[0].ActualPosition.X) && x.Position.Y > CurrentSnakeBlocks[0].ActualPosition.Y).Count() > 0)
                inputs[0] = CurrentSnakeBlocks[0].ActualPosition.Y - apples.Where(x => (x.Position.X == CurrentSnakeBlocks[0].ActualPosition.X) && x.Position.Y > CurrentSnakeBlocks[0].ActualPosition.Y).First().Position.Y;
            if (apples.Where(x => (x.Position.X == CurrentSnakeBlocks[0].ActualPosition.X) && x.Position.Y < CurrentSnakeBlocks[0].ActualPosition.Y).Count() > 0)
                inputs[1] = apples.Where(x => (x.Position.X == CurrentSnakeBlocks[0].ActualPosition.X) && x.Position.Y < CurrentSnakeBlocks[0].ActualPosition.Y).First().Position.Y - CurrentSnakeBlocks[0].ActualPosition.Y;
            if (apples.Where(x => (x.Position.Y == CurrentSnakeBlocks[0].ActualPosition.Y) && x.Position.X > CurrentSnakeBlocks[0].ActualPosition.X).Count() > 0)
                inputs[2] = CurrentSnakeBlocks[0].ActualPosition.X - apples.Where(x => (x.Position.Y == CurrentSnakeBlocks[0].ActualPosition.Y) && x.Position.X > CurrentSnakeBlocks[0].ActualPosition.X).First().Position.X;
            if (apples.Where(x => (x.Position.Y == CurrentSnakeBlocks[0].ActualPosition.Y) && x.Position.X < CurrentSnakeBlocks[0].ActualPosition.X).Count() > 0)
                inputs[3] = apples.Where(x => (x.Position.Y == CurrentSnakeBlocks[0].ActualPosition.Y) && x.Position.X < CurrentSnakeBlocks[0].ActualPosition.X).First().Position.X- CurrentSnakeBlocks[0].ActualPosition.X ;

            var moves = DecodeMove(Brain.DoNeuralStuff(inputs.ToList()), Treshold);

            MoveSnakeInDirection(moves);
        }

        private void UseBrainToMove(List<SnakeObject> snakes, List<Apple> apples, object locker) {
            Point nearestApple = FindNearestApple(apples);
            var nearestSnakes = FindXNearestSnakes(snakes, 2);

            //Xapple Yapple Xenemy1 Yenemy1 Xenemy2 Yenemy2
            List<double> inputs = new List<double>() {
                (nearestApple.X - CurrentSnakeBlocks[0].ActualPosition.X)/GameHeight,
                (nearestApple.Y- CurrentSnakeBlocks[0].ActualPosition.Y)/GameWidth,
            };
            //No need for other snakes
            lock (locker) {
                foreach (var x in nearestSnakes) {
                    inputs.Add((x.X - CurrentSnakeBlocks[0].ActualPosition.X) / GameHeight);
                    inputs.Add((x.Y - CurrentSnakeBlocks[0].ActualPosition.Y) / GameWidth);
                }
            }
            var moves = DecodeMove(Brain.DoNeuralStuff(inputs), Treshold);

            MoveSnakeInDirection(moves);
        }
        private Vector DecodeMove(List<double> move, double threshold) {
            //only one way and only if there is enough strength
            int index = move.IndexOf(move.Max());

            if (move[index] > Treshold) {
                switch (index) {
                    case 0: return new Vector(+1, 0);
                    case 1: return new Vector(-1, 0);
                    case 2: return new Vector(0, 1);
                    case 3: return new Vector(0, -1);

                    default: return new Vector(0, 0);
                }
            }

            return new Vector(0, 0);
        }

        private Point FindNearestApple(List<Apple> apples) {
            double distance = double.MaxValue;
            Point p = apples[0].Position; 
            foreach(var apple in apples) {
                if (Distance(apple.Position, CurrentSnakeBlocks[0].ActualPosition) < distance)
                    p = apple.Position;
            }
            return p;
        }

        private List<Point> FindXNearestSnakes(List<SnakeObject> Snakes, int capacity) {

            List<Point> points = new List<Point>();
            foreach (var s in Snakes) {
                if (s.Active && !s.Equals(this))
                    foreach (var sp in s.CurrentSnakeBlocks)
                        points.Add(sp.ActualPosition);

            }

            return points.OrderBy(point => Distance(CurrentSnakeBlocks[0].ActualPosition, point)).Take(capacity).ToList();
        }

        private double Distance(Point p1, Point p2) {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        public void DetectColision(List<SnakeObject> snakeList, List<Apple> apples) {
            foreach (SnakeObject s in snakeList) {
                if (!s.Equals(this) && s.Active) {
                    foreach (var snakeBlock in s.CurrentSnakeBlocks) {
                        //if positions are the same that means, that snake crashed
                        if (Point.Equals(this.CurrentSnakeBlocks[0].ActualPosition, snakeBlock.ActualPosition)) {
                            this.Active = false;
                            foreach(var sb in this.CurrentSnakeBlocks) {
                                apples.Add(new Apple(sb.ActualPosition));
                            }
                        }
                    }
                }
                if (s.Equals(this)) {
                    for(int i=1; i<s.CurrentSnakeBlocks.Count; i++) {
                        //if positions are the same that means, that snake crashed
                        if (Point.Equals(this.CurrentSnakeBlocks[0].ActualPosition, s.CurrentSnakeBlocks[i].ActualPosition)) {
                            this.Active = false;
                            foreach (var sb in this.CurrentSnakeBlocks) {
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
                if (Point.Equals(this.CurrentSnakeBlocks[0].ActualPosition, apple.Position)){
                    applesToRemoveList.Add(apple);
                    ExtendSnake();
                }
            }
            foreach (var apple in applesToRemoveList) {
                apples.Remove(apple);
            }
        }

        private void ExtendSnake() {
            CurrentSnakeBlocks.Add(new SnakeBlock(CurrentSnakeBlocks[CurrentSnakeBlocks.Count - 1].LastPosition));
        }
    }
}
