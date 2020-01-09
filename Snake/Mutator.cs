using Snake.Neural;
using Snake.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    public static class Mutator
    {
        public static List<SnakeObject> MutateForOneSnakeGame(List<SnakeObject> snakes, Random rng) {
            snakes = snakes.Where(x => x.Fitness > Settings.Default.AppleTimeSpan).OrderByDescending(x => x.Fitness).ToList();
            List<SnakeObject> bestSnakes = new List<SnakeObject>();
            List<SnakeObject> newSnakes = new List<SnakeObject>();
            if (snakes.Count != 0) {


                foreach (var snake in snakes.Take((int)(Settings.Default.NoGamesAtOnce * 0.1))) {
                    bestSnakes.Add(new SnakeObject(rng, snake.Brain));
                }


                for (int i = 1; i < snakes.Count / 2; i++) {
                    for (int j = 0; j < 3; j++) {
                        NeuralNetwork brain = new NeuralNetwork(snakes[i - 1].Brain, snakes[i].Brain, rng);
                        newSnakes.Add(new SnakeObject(rng, brain));
                    }
                }



                Shuffle(newSnakes, rng);
                newSnakes = newSnakes.Take(Settings.Default.NoGamesAtOnce - (int)(Settings.Default.NoGamesAtOnce * 0.1) - 2).ToList();

                newSnakes.InsertRange(0, bestSnakes);
            }
            for (int i = newSnakes.Count; i < Settings.Default.NoGamesAtOnce; i++)
                newSnakes.Add(new SnakeObject(rng));

            snakes.Clear();
            return newSnakes;
        }

        public static List<SnakeObject> MutateSnakesForMultiGame(List<SnakeObject> snakes, Random rng) {
            Shuffle(snakes, rng);

            List<SnakeObject> newSnakes = new List<SnakeObject>();
            for (int i = 0; i < Math.Round(Settings.Default.NoGamesAtOnce / 2 + 0.1); i++)
                newSnakes.Add(new SnakeObject(rng, snakes[i].Brain));

            for (int i = 1; i < snakes.Count; i++) {
                NeuralNetwork brain = new NeuralNetwork(snakes[i - 1].Brain, snakes[i].Brain, rng);
                newSnakes.Add(new SnakeObject(rng, brain));
                newSnakes.Add(new SnakeObject(rng, brain));
            }
            snakes.Clear();

            Shuffle(newSnakes, rng);
            return newSnakes.Take(Settings.Default.NoGamesAtOnce).ToList();
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
