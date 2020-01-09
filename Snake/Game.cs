using Snake.Neural;
using Snake.Properties;
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
        public string EndCausing { get; private set; }

        private Random random;

        public double BestFitness { get {
                return GetAllSnakes().Max(x => x.Fitness);
        }}
        public int RunningSnakes { get {
                return SnakeList.Count();
        } }

        public int GameRounds { get; private set; }

        /*Game strategy when game ends when noone touch apple for long time*/
        public Game(List<SnakeObject> snakes, Random random, bool mutate) {
            GameRounds = 0;
            this.random = random;
            if (mutate) {
                SnakeList = Mutator.MutateSnakesForMultiGame(snakes, random);
            } else {
                SnakeList = snakes;
            }
            Apples.Add(new Apple(SnakeList, random));
        }

        public Game(Random random) {

            GameRounds = 0;
            this.random = random;


            for (int i = 0; i < Settings.Default.NoSnakes; i++)
                SnakeList.Add(new SnakeObject(random));

            //set apple default canvas position
            //create new apple
            Apples.Add(new Apple(SnakeList, random));
        }
        /*END Game strategy when game ends when noone touch apple for long time*/

        private void MutateSnakes(List<SnakeObject> snakes, Random rng) {
            List<SnakeObject> newSnakes = new List<SnakeObject>();
            for(int i=0; i<Math.Round(Settings.Default.NoSnakes/2 + 0.1); i++)
                newSnakes.Add(new SnakeObject( rng, snakes[i].Brain));            
            
            for(int i=1; i<snakes.Count; i++) {
                NeuralNetwork brain = new NeuralNetwork(snakes[i - 1].Brain, snakes[i].Brain, rng);
                newSnakes.Add(new SnakeObject(rng, brain));
                newSnakes.Add(new SnakeObject(rng, brain));
            }
            snakes.Clear();

            Shuffle(newSnakes,rng);
            SnakeList = newSnakes.Take(Settings.Default.NoSnakes).ToList();
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

            if(SnakeList.Count == 0) {
                EndCausing = "all snakes are dead";
                Active = false;
            }
        }

        private void MoveSnakes() {
            try {
                object locker = new object();
                foreach (SnakeObject s in SnakeList) {
                    s.UseBrainToTurn(Apples, locker);
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
            if (Settings.Default.AppleTimeSpan > -1) {
                foreach (var A in Apples) {
                    A.LiveSpan++;
                    if (A.LiveSpan > Settings.Default.AppleTimeSpan) {
                        EndCausing = "Apple is too old";
                        this.Active = false;
                        return;
                    }
            }
            }

            if (Apples.Count == 0)
                Apples.Add(new Apple(SnakeList, random));
        }

        public List<SnakeObject> GetAllSnakes() {
            return SnakeList.Union(DeadSnakes).ToList();
        }

        private void DecideIfStillGame() {
            if (Settings.Default.NoRounds > -1) {
                if (RunningSnakes <= 1 || GameRounds > Settings.Default.NoRounds) {
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
