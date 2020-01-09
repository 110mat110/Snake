using Snake.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake {
    public static class Sensors {
        public static List<double> FindAppleIn4Directions(List<Apple> apples, SnakeBlock head) {
            double[] inputs = new double[] { 0, 0, 0, 0 };
            if (apples.Where(x => (x.Position.X == head.ActualPosition.X) && x.Position.Y > head.ActualPosition.Y).Count() > 0)
                inputs[0] = head.ActualPosition.Y - apples.Where(x => (x.Position.X == head.ActualPosition.X) && x.Position.Y > head.ActualPosition.Y).First().Position.Y;
            if (apples.Where(x => (x.Position.X == head.ActualPosition.X) && x.Position.Y < head.ActualPosition.Y).Count() > 0)
                inputs[1] = apples.Where(x => (x.Position.X == head.ActualPosition.X) && x.Position.Y < head.ActualPosition.Y).First().Position.Y - head.ActualPosition.Y;
            if (apples.Where(x => (x.Position.Y == head.ActualPosition.Y) && x.Position.X > head.ActualPosition.X).Count() > 0)
                inputs[2] = head.ActualPosition.X - apples.Where(x => (x.Position.Y == head.ActualPosition.Y) && x.Position.X > head.ActualPosition.X).First().Position.X;
            if (apples.Where(x => (x.Position.Y == head.ActualPosition.Y) && x.Position.X < head.ActualPosition.X).Count() > 0)
                inputs[3] = apples.Where(x => (x.Position.Y == head.ActualPosition.Y) && x.Position.X < head.ActualPosition.X).First().Position.X - head.ActualPosition.X;
            return inputs.ToList();
        }

        public static List<double> FindSnakeOrientation(SnakeObject snake) {
            return new double[] {
               snake.Orientation==Orientation.Top ? 1 :0,
               snake.Orientation==Orientation.Down ? 1 :0,
               snake.Orientation==Orientation.Left ? 1 :0,
               snake.Orientation==Orientation.Right ? 1 :0
            }.ToList();
        }

        public static List<bool> FindBinaryAppleIn4Directions(List<Apple> apples, SnakeBlock head) {
            return ConvertDistToBool(FindAppleIn4Directions(apples, head));
        }

        public static List<double> FindWallIn4Directions(SnakeBlock head) {
            return new double[] {
                head.ActualPosition.X,
                Settings.Default.GameWidth - head.ActualPosition.X,
                head.ActualPosition.Y,
                Settings.Default.GameHeight - head.ActualPosition.Y,
            }.ToList();
        }

        public static List<double> FindAppleDirectionaly(List<Apple> apples, SnakeObject snake) {
            return TransferToDirectional(snake, FindAppleIn4Directions(apples, snake.CurrentSnakeBlocks[0]));
        }

        public static List<double> FindWallDirectionaly(SnakeObject snake) {
            return TransferToDirectional(snake, FindWallIn4Directions(snake.CurrentSnakeBlocks[0]));
        }

        private static List<bool> ConvertDistToBool(List<double> list) {
            List<bool> returnList = new List<bool>();
            foreach (var dist in list) {
                returnList.Add(dist <= 0);
            }
            return returnList;
        }
        private static void Shift<T>(List<T> list) {
            var LastItem = list[list.Count-1];
            for (int i=list.Count-1; i > 1; i--) {
                list[i] = list[i - 1];
            }
            list[0] = LastItem;
        }

        private static List<double> TransferToDirectional(SnakeObject snake, List<double> outputs) {
            for (int i = 0; i < (int)snake.Orientation; i++) {
                Shift(outputs);
            }
            return outputs;
        }
    }
}
