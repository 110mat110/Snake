using Snake.Neural;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Snake {
    public class Game {
        public bool Active = true;

        private List<SnakeObject> SnakeList = new List<SnakeObject>();
        private List<SnakeObject> DeadSnakes = new List<SnakeObject>();
        private List<Apple> Apples = new List<Apple>();
        public int GameWidth { get; private set; }
        public int GameHeight { get; private set; }
        public string EndCausing { get; private set; }

        private Random random;
        private int penalty;
        private int AppleTimeSpan = -1;
        private int NoRounds = -1;
        private double treshold;

        public int BestFitness { get {
                return GetAllSnakes().Max(x => x.Fitness);
        }}
        public int RunningSnakes { get {
                return SnakeList.Count();
        } }

        public int GameRounds { get; private set; }

        /*Game strategy when game ends when noone touch apple for long time*/
        public Game(int gameWidth, int gameHeight, int NoSnakes, List<SnakeObject> snakes, Random random, double MutateRatio, int penalty, int AppleTimeSpan, int NoRounds, double treshold) {
            GameWidth = gameWidth;
            GameHeight = gameHeight;
            GameRounds = 0;
            this.random = random;
            this.penalty = penalty;
            this.treshold = treshold;
            this.AppleTimeSpan = AppleTimeSpan;
            this.NoRounds = NoRounds;

            MutateSnakes(NoSnakes, snakes, random, MutateRatio);

            Apples.Add(new Apple(SnakeList, GameWidth, GameHeight, random));
        }

        public Game(int gameWidth, int gameHeight, int NoSnakes, Random random, int penalty, int AppleTimeSpan, int NoRounds, double treshold) {
            GameWidth = gameWidth;
            GameHeight = gameHeight;
            GameRounds = 0;
            this.AppleTimeSpan = AppleTimeSpan;
            this.NoRounds = NoRounds;
            this.random = random;
            this.penalty = penalty;
            this.treshold = treshold;

            for (int i = 0; i < NoSnakes; i++)
                SnakeList.Add(new SnakeObject(GameWidth, GameHeight, random, penalty, treshold));

            //set apple default canvas position
            //create new apple
            Apples.Add(new Apple(SnakeList, GameWidth, GameHeight, random));
        }
        /*END Game strategy when game ends when noone touch apple for long time*/

            //Strategy, when we add presorted snakes to game
        public Game(int gameWidth, int gameHeight, List<SnakeObject> snakes, Random random, int penalty, int AppleTimeSpan, int NoRounds, double treshold) {
            GameWidth = gameWidth;
            GameHeight = gameHeight;
            GameRounds = 0;
            this.random = random;
            this.penalty = penalty;
            this.treshold = treshold;
            this.AppleTimeSpan = AppleTimeSpan;
            this.NoRounds = NoRounds;

            SnakeList = snakes;

            Apples.Add(new Apple(SnakeList, GameWidth, GameHeight, random));
        }

        private void MutateSnakes(int noSnakes, List<SnakeObject> snakes, Random rng, double MutateRatio) {
            List<SnakeObject> newSnakes = new List<SnakeObject>();
            for(int i=0; i<Math.Round(noSnakes/2 + 0.1); i++)
                newSnakes.Add(new SnakeObject(GameWidth, GameHeight, rng, snakes[i].Brain, penalty, treshold));            
            
            for(int i=1; i<snakes.Count; i++) {
                NeuralNetwork brain = new NeuralNetwork(snakes[i - 1].Brain, snakes[i].Brain, rng, MutateRatio);
                newSnakes.Add(new SnakeObject(GameWidth, GameHeight, rng, brain, penalty, treshold));
                newSnakes.Add(new SnakeObject(GameWidth, GameHeight, rng, brain, penalty, treshold));
            }
            snakes.Clear();

            Shuffle(newSnakes,rng);
            SnakeList = newSnakes.Take(noSnakes).ToList();
        }

        private static void Shuffle<T>(IList<T> list, Random rng) {
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public Task FastLoop(int MaxLoops) {
            return Task.Run(() => {
                if (MaxLoops > -1) {
                    while (Active &&  GameRounds < MaxLoops) {
                        GameTick();
                    }
                } else {
                    while (Active) {
                        GameTick();
                    }
                }
            });
        }

        /*There you have to move snake and do everything*/
        public void GameTick() {
            if (!Active) return;

            GameRounds++;
                //Move all snakes
            MoveSnakes();
            //And then detect colisions
            HandleColisions();
            //Handle apple
            HandleApple();
        }

        private void MoveSnakes() {
            try {
                object locker = new object();
                foreach (SnakeObject s in SnakeList) {
                    s.UseBrainToMove(Apples, locker);
                }
            } catch (Exception ex) {
                Debug.WriteLine(ex.Message);
                EndCausing = "Exception " + ex.Message;
                Active = false;
            }
        }

        private void HandleColisions() {
            List<SnakeObject> SnakesToRemove = new List<SnakeObject>();
            foreach (SnakeObject s in SnakeList) {
                s.DetectColision(SnakeList, Apples);
                if (!s.Active)
                    SnakesToRemove.Add(s);
            }
            foreach (var s in SnakesToRemove) {
                SnakeList.Remove(s);
                DeadSnakes.Add(s);
            }
        }

        public List<SnakeBlock> GetAllSnakeDots() {
            List<SnakeBlock> sblist = new List<SnakeBlock>();
            foreach(var s in SnakeList) {
                if (s.Active) {
                    sblist.AddRange(s.CurrentSnakeBlocks);
                }
            }
            return sblist;
        }

        public List<Apple> GetApplesDots() {
            return Apples;
        }

        private void HandleApple() {

            foreach (var s in SnakeList) {
                s.EatApple(Apples);
            }
            if (AppleTimeSpan > -1) {
                foreach (var A in Apples) {
                    A.LiveSpan++;
                    if (A.LiveSpan > AppleTimeSpan) {
                        EndCausing = "Apple is too old";
                        this.Active = false;
                        return;
                    }
            }
            }

            if (Apples.Count == 0)
                Apples.Add(new Apple(SnakeList, GameWidth, GameHeight, random));
        }

        public List<SnakeObject> GetAllSnakes() {
            return SnakeList.Union(DeadSnakes).ToList();
        }

        private void DecideIfStillGame() {
            if (NoRounds > -1) {
                if (RunningSnakes <= 1 || GameRounds > NoRounds) {
                    EndCausing = "Basic decision 1st";
                    Active = false;
                }
            } else {
                if (RunningSnakes <= 1) {
                    EndCausing = "Basic decision 2nd";
                    Active = false;
                }
            }
        }

        public SnakeObject GetBestSnake() {
            return SnakeList.Count != 0
                ? SnakeList.OrderByDescending(x => x.Fitness).First()
                : GetAllSnakes().OrderByDescending(x => x.Fitness).First();
        }
    }
}
