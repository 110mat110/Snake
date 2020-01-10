using Snake.Neural;
using Snake.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Snake {
    public enum Orientation { Top, Right, Down, Left};

    public class SnakeObject {
        public List<SnakeBlock> CurrentSnakeBlocks = new List<SnakeBlock>();

        public bool Active { get; private set; }

        public double Fitness { get { return Active ? LifeTime * (1 + CurrentSnakeBlocks.Count * 0.1) : LifeTime * (1 + (CurrentSnakeBlocks.Count - Settings.Default.Penalty) * 0.1); } }
        public NeuralNetwork Brain;
        public int Energy { get; set; }

        private int LastMoveX = 1;
        private int LastMoveY = 0;

        private int LifeTime = 0;
        public Orientation Orientation { get; private set; }

        #region Constructor
        public SnakeObject(Point firstPosition, Random random) {
            Orientation = Orientation.Top;
            Active = true;
            Energy = Settings.Default.AppleEnergy;
            CurrentSnakeBlocks.Add(new SnakeBlock(firstPosition));

            Brain = new NeuralNetwork(new List<int>() { 10, 4 }, random);
        }
        public SnakeObject(Random randomizer) {
            Active = true;
            Orientation = Orientation.Top;
            Energy = Settings.Default.AppleEnergy;
            CurrentSnakeBlocks.Add(new SnakeBlock(new Point(randomizer.Next(0, Settings.Default.GameWidth), randomizer.Next(0, Settings.Default.GameHeight))));

            Brain = new NeuralNetwork(new List<int>() { 10, 4 }, randomizer);
        }
        public SnakeObject(Random randomizer, NeuralNetwork brain) {
            Active = true;
            Orientation = Orientation.Top;
            Energy = Settings.Default.AppleEnergy;
            CurrentSnakeBlocks.Add(new SnakeBlock(new Point(randomizer.Next(0, Settings.Default.GameWidth), randomizer.Next(0, Settings.Default.GameHeight))));

            Brain = brain;
        }
        #endregion

        #region Movement
        public void TurnSnake(bool left, bool right) {
            if (right) {
                var ior = (int)Orientation + 1;
                if (ior == 4) ior = 0;
                Orientation = (Orientation)ior;
            }
            if (left) {
                var ior = (int)Orientation - 1;
                if (ior == -1) ior = 3;
                Orientation = (Orientation)ior;
            }

            if (Orientation == Orientation.Top) MoveSnakeInDirection(new Vector(0, -1));
            if (Orientation == Orientation.Down) MoveSnakeInDirection(new Vector(0, 1));
            if (Orientation == Orientation.Left) MoveSnakeInDirection(new Vector(-1, 0));
            if (Orientation == Orientation.Right) MoveSnakeInDirection(new Vector(1, 0));
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

            if (!Settings.Default.Walls) {
                if (headPos.X < 0) headPos.X = Settings.Default.GameWidth;
                if (headPos.X > Settings.Default.GameWidth) headPos.X = 0;

                if (headPos.Y < 0) headPos.Y = Settings.Default.GameHeight;
                if (headPos.Y > Settings.Default.GameHeight) headPos.Y = 0;
            }
            //now walls are solid, so move outside of map and then dies

            //Then move head to this position
            CurrentSnakeBlocks[0].Move(headPos);

            //And move rest of blocks except head
            for (int i = 1; i < CurrentSnakeBlocks.Count; i++) {
                //Set block last position of previous block
                CurrentSnakeBlocks[i].Move(CurrentSnakeBlocks[i - 1].LastPosition);
            }

            LifeTime++;
            Energy--;
        }

        private bool[] DecodeTurn(List<double> move) {
            if (move.Any(x => x > Settings.Default.Treshold)) {
                if (move[2] > move[0] && move[2] > move[1])
                    return new bool[] { false, false };
                else
                    return new bool[] { move[0] > move[1], move[0] < move[1] };
            }
            return new bool[] { false, false };
        }
        private Vector DecodeMove(List<double> move) {
            //only one way and only if there is enough strength
            int index = move.IndexOf(move.Max());

            if (move[index] > Settings.Default.Treshold) {
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
        #endregion

        #region Move stretegy
        public void RandomMoveSnake(Random randomizer) {
            MoveSnakeInDirection(randomizer.Next(3) - 1, randomizer.Next(3) - 1);
        }

        public void UseBinaryBrainToMove(List<Apple> apples, object locker) {

            List<bool> inputs = Sensors.FindBinaryAppleIn4Directions(apples, CurrentSnakeBlocks[0]);
            var moves = DecodeMove(Brain.DoNeuralStuff(inputs));

            MoveSnakeInDirection(moves);
        }

        public void UseBrainToMove(List<Apple> apples, object locker) {
            List<double> inputs = Sensors.FindAppleIn4Directions(apples, CurrentSnakeBlocks[0]);
            inputs.AddRange(Sensors.FindWallIn4Directions(CurrentSnakeBlocks[0]));
            var moves = DecodeMove(Brain.DoNeuralStuff(inputs));

            MoveSnakeInDirection(moves);
        }

        public void UseBrainToTurn(List<Apple> apples, object locker) {
            List<double> inputs = Sensors.FindAppleDirectionaly(apples, this);
            inputs.AddRange(Sensors.FindSnakeOrientation(this));
            if(Settings.Default.Walls) inputs.AddRange(Sensors.FindWallDirectionaly(this));
            var moves = DecodeTurn(Brain.DoNeuralStuff(inputs));

            TurnSnake(moves[0], moves[1]);
        }
        #endregion

        #region Life Functions
        public void DetectColision(List<SnakeObject> snakeList, List<Apple> apples) {
            //Solid walls
            if (CurrentSnakeBlocks[0].ActualPosition.X > Settings.Default.GameWidth
                || CurrentSnakeBlocks[0].ActualPosition.X < 0
                || CurrentSnakeBlocks[0].ActualPosition.Y > Settings.Default.GameHeight
                || CurrentSnakeBlocks[0].ActualPosition.Y < 0)
                KillSnake(apples);


            foreach (SnakeObject s in snakeList) {
                if (!s.Equals(this) && s.Active) {
                    foreach (var snakeBlock in s.CurrentSnakeBlocks) {
                        if (Point.Equals(this.CurrentSnakeBlocks[0].ActualPosition, snakeBlock.ActualPosition)) {
                            KillSnake(apples);
                        }
                    }
                }
                if (s.Equals(this)) {
                    for (int i = 1; i < s.CurrentSnakeBlocks.Count; i++) {
                        if (Point.Equals(this.CurrentSnakeBlocks[0].ActualPosition, s.CurrentSnakeBlocks[i].ActualPosition)) {
                            KillSnake(apples);
                        }
                    }
                }
            }
        }

        private void KillSnake(List<Apple> apples) {
            this.Active = false;
            foreach (var sb in this.CurrentSnakeBlocks) {
                apples.Add(new Apple(sb.ActualPosition));
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
                Energy = Settings.Default.AppleEnergy;
            }
        }

        private void ExtendSnake() {
            CurrentSnakeBlocks.Add(new SnakeBlock(CurrentSnakeBlocks[CurrentSnakeBlocks.Count - 1].LastPosition));
        }

        public void CheckEnergy(List<Apple> apples) {
            if (Energy <= 0) KillSnake(apples);
        }
        #endregion
    }
}
