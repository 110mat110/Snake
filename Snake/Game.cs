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

        private Random random;
        public int BestFitness { get {
                return GetAllSnakes().Max(x => x.Fitness);
        }}
        public int RunningSnakes { get {
                return SnakeList.Count();
        } }

        public int GameRounds { get; private set; }

        public Game(int gameWidth, int gameHeight, int NoSnakes, Random random) {
            GameWidth = gameWidth;
            GameHeight = gameHeight;
            GameRounds = 0;
            this.random = random;
            for (int i = 0; i < NoSnakes; i++)
                SnakeList.Add(new SnakeObject(GameWidth, GameHeight, random));

            //set apple default canvas position
            //create new apple
            Apples.Add(new Apple(SnakeList, GameWidth, GameHeight, random));
        }

        public Game(int gameWidth, int gameHeight, int NoSnakes, List<SnakeObject> snakes, Random random, double MutateRatio) {
            GameWidth = gameWidth;
            GameHeight = gameHeight;
            GameRounds = 0;
            this.random = random;

            MutateSnakes(NoSnakes, snakes, random, MutateRatio);

            Apples.Add(new Apple(SnakeList, GameWidth, GameHeight, random));
        }

        private void MutateSnakes(int noSnakes, List<SnakeObject> snakes, Random rng, double MutateRatio) {
            List<SnakeObject> newSnakes = new List<SnakeObject>();
            for(int i=0; i<noSnakes/2; i++)
                newSnakes.Add(new SnakeObject(GameWidth, GameHeight, rng, snakes[i].Brain));            
            
            for(int i=1; i<snakes.Count; i++) {
                NeuralNetwork brain = new NeuralNetwork(snakes[i - 1].Brain, snakes[i].Brain, rng, MutateRatio);
                newSnakes.Add(new SnakeObject(GameWidth, GameHeight, rng, brain));
                newSnakes.Add(new SnakeObject(GameWidth, GameHeight, rng, brain));
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
                while (Active && GameRounds < MaxLoops) {
                    GameTick();
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
                foreach (SnakeObject s in SnakeList) {
                    s.UseBrainToMove(SnakeList, Apples);
                }
            } catch (Exception ex) {
                Debug.WriteLine(ex.Message);
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
            if (Apples.Count == 0)
                Apples.Add(new Apple(SnakeList, GameWidth, GameHeight, random));
        }

        public List<SnakeObject> GetAllSnakes() {
            return SnakeList.Union(DeadSnakes).ToList();
        }
    }
}
