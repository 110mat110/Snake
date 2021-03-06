﻿using Snake.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Snake {
    public class Apple {
        //Posoiton of apple
        public Point Position { get; set; }
        //constructor with multiple parameters. It is for generation of apple
        public Apple(List<SnakeObject> Snake, Random rnd) {
            GenerateNewPosition(Snake, rnd);
        }
        public Apple(Point Position) {
            this.Position = Position;
        }


        public void GenerateNewPosition(List<SnakeObject> SnakeList, Random rnd) {
            bool goodPosition = true;
            //random generate new positions. If there is snake on this position, pick another position
            do {
                goodPosition = true;
                //generate new random point
                Point newPos = new Point(rnd.Next(Settings.Default.GameWidth -1 ), rnd.Next(Settings.Default.GameHeight -1));
                //chcek all snake points, if there is collision
                foreach(var snake in SnakeList) {
                    foreach (var sp in snake.CurrentSnakeBlocks) {

                        //If there is, raise flag
                        if (Equals(sp.ActualPosition, newPos)) {
                            goodPosition = false;
                            //No need to check further, one colision is enough
                            break;
                        }
                    }
                }
                //If position is good, then add this position to our apple
                if (goodPosition)
                    Position = newPos;
            } while (!goodPosition);

        }
    }
}
