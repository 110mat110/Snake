using Snake.Neural;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    public static class Mutator
    {
        public static List<SnakeObject> MutateForOneSnakeGame(int noGames, List<SnakeObject> snakes, Random rng, double MutateRatio, int GameWidth, int GameHeight, int penalty, double treshold) {
            //Shuffle(snakes, rng);
            List<SnakeObject> bestSnakes = new List<SnakeObject>();
            foreach(var snake in snakes.Take((int)(noGames * 0.1))) {
                bestSnakes.Add(new SnakeObject(GameWidth, GameHeight, rng, snake.Brain, penalty, treshold));
            }
            List<SnakeObject> newSnakes = new List<SnakeObject>();

            for (int i = 1; i < snakes.Count/2; i++) {
                for (int j = 0; j < 3; j++) {
                    NeuralNetwork brain = new NeuralNetwork(snakes[i - 1].Brain, snakes[i].Brain, rng, MutateRatio);
                    newSnakes.Add(new SnakeObject(GameWidth, GameHeight, rng, brain, penalty, treshold));
                }
            }
            snakes.Clear();

            Shuffle(newSnakes, rng);
            newSnakes = newSnakes.Take(noGames-(int)(noGames*0.1)).ToList();

            newSnakes.InsertRange(0, bestSnakes);
            return newSnakes;
        }

        public static List<SnakeObject> MutateSnakesForMultiGame(int noSnakes, List<SnakeObject> snakes, Random rng, double MutateRatio, int GameWidth, int GameHeight, int penalty, double treshold) {
            Shuffle(snakes, rng);

            List<SnakeObject> newSnakes = new List<SnakeObject>();
            for (int i = 0; i < Math.Round(noSnakes / 2 + 0.1); i++)
                newSnakes.Add(new SnakeObject(GameWidth, GameHeight, rng, snakes[i].Brain, penalty, treshold));

            for (int i = 1; i < snakes.Count; i++) {
                NeuralNetwork brain = new NeuralNetwork(snakes[i - 1].Brain, snakes[i].Brain, rng, MutateRatio);
                newSnakes.Add(new SnakeObject(GameWidth, GameHeight, rng, brain, penalty, treshold));
                newSnakes.Add(new SnakeObject(GameWidth, GameHeight, rng, brain, penalty, treshold));
            }
            snakes.Clear();

            Shuffle(newSnakes, rng);
            return newSnakes.Take(noSnakes).ToList();
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
    }
}
