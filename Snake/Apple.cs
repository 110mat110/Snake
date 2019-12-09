using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Snake {
    class Apple {
        //Posoiton of apple
        public Point Position { get; set; }
        //constructor with multiple parameters. It is for generation of apple
        public Apple(List<SnakeBlock> Snake, int gameWidth, int gameHeight) {
            GenerateNewPosition(Snake, gameWidth, gameHeight);
        }

        public bool CheckIfSnakeEats(List<SnakeBlock> Snake) {
            //If snake head is in same position, that means, that snake can eat apple
            return Point.Equals(Snake[0].ActualPosition, Position);
        }

        public void GenerateNewPosition(List<SnakeBlock> Snake, int gameWidth, int gameHeight) {
            //Create randomizer
            var rnd = new Random();
            bool goodPosition = true;
            //random generate new positions. If there is snake on this position, pick another position
            do {
                goodPosition = true;
                //generate new random point
                Point newPos = new Point(rnd.Next(gameWidth -1 ), rnd.Next(gameHeight -1));
                //chcek all snake points, if there is collision
                foreach(var sp in Snake) {
                    //If there is, raise flag
                    if (Equals(sp, newPos)) {
                        goodPosition = false;
                        //No need to check further, one colision is enough
                        break;
                    }
                }
                //If position is good, then add this position to our apple
                if (goodPosition)
                    Position = newPos;
            } while (!goodPosition);

        }
    }
}
