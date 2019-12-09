using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Snake {
    class Apple {

        public Point Position { get; set; }
        public int gameWidth { get; set; }
        public int gameHeight { get; set; }
        public Apple() {

        }

        public bool CheckIfSnakeEats(List<SnakeBlock> Snake) {
            return Point.Equals(Snake[0].ActualPosition, Position);
        }

        public void GenerateNewPosition(List<SnakeBlock> Snake) {
            var rnd = new Random();
            bool goodPosition = true;
            do {
                goodPosition = true;
                Point newPos = new Point(rnd.Next(gameWidth -1 ), rnd.Next(gameHeight -1));
                foreach(var sp in Snake) {
                    if (Equals(sp, newPos)) {
                        goodPosition = false;
                        break;
                    }
                }
                if (goodPosition)
                    Position = newPos;
            } while (!goodPosition);

        }
    }
}
