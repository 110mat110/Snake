using Snake.Neural;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Snake {
    public class Game {
        private List<SnakeObject> SnakeList = new List<SnakeObject>();
        private List<SnakeObject> DeadSnakes = new List<SnakeObject>();
        private List<Apple> Apples = new List<Apple>();
        public int GameWidth { get; private set; }
        public int GameHeight { get; private set; }
        public int BestFitness { get {
                return SnakeList.Union(DeadSnakes).Max(x => x.Fitness);
        }}
        public int RunningSnakes { get {
                return SnakeList.Count();
            } }

        public int GameRounds { get; private set; }
        private int NoSnakes;
        private static Random rng = new Random();

        public Game(int gameWidth, int gameHeight, int NoSnakes) {
            GameWidth = gameWidth;
            GameHeight = gameHeight;
            this.NoSnakes = NoSnakes;
            GameRounds = 0;

            var random = new Random();
            for (int i = 0; i < NoSnakes; i++)
                SnakeList.Add(new SnakeObject(GameWidth, GameHeight, random));


            //set apple default canvas position
            //create new apple
            Apples.Add(new Apple(SnakeList, GameWidth, GameHeight));
        }

        public Game(int gameWidth, int gameHeight, int NoSnakes, List<SnakeObject> snakes) {
            GameWidth = gameWidth;
            GameHeight = gameHeight;
            this.NoSnakes = NoSnakes;
            GameRounds = 0;

            MutateSnakes(NoSnakes, snakes);

            Apples.Add(new Apple(SnakeList, GameWidth, GameHeight));
        }

        private void MutateSnakes(int noSnakes, List<SnakeObject> snakes) {
            List<SnakeObject> newSnakes = new List<SnakeObject>();
            /*foreach(var s in snakes) {
                newSnakes.Add(new SnakeObject(GameWidth, GameHeight, rng, s.Brain));            
            }*/
            for(int i=1; i<snakes.Count; i++) {
                NeuralNetwork brain = new NeuralNetwork(snakes[i - 1].Brain, snakes[i].Brain);
                newSnakes.Add(new SnakeObject(GameWidth, GameHeight, rng, brain));
                newSnakes.Add(new SnakeObject(GameWidth, GameHeight, rng, brain));
            }
            snakes.Clear();

            Shuffle(newSnakes);
            SnakeList = newSnakes.Take(noSnakes).ToList();
        }

        private static void Shuffle<T>(IList<T> list) {
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }


        /*There you have to move snake and do everything*/
        public void GameTick() {
            GameRounds++;
            //Move all snakes
            MoveSnakes();

            //And then detect colisions
            HandleColisions();

            //Handle apple
            HandleApple();
        }

        private void MoveSnakes() {
            foreach (SnakeObject s in SnakeList) {
                s.UseBrainToMove(SnakeList, Apples);
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
                Apples.Add(new Apple(SnakeList, GameWidth, GameHeight));
        }

        public List<SnakeObject> GetAllSnakes() {
            return SnakeList.Union(DeadSnakes).ToList();
        }
    }
}
