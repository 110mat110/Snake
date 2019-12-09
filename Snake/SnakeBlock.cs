using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Snake {
    public class SnakeBlock {
        
        public Point ActualPosition { get; private set; } // Block actual position
        public Point LastPosition { get; private set; }   // Block last position for moving purpose
        //Constructor with position setup
        public SnakeBlock(Point position) {
            ActualPosition = position;
            LastPosition = position;
        }

        public void Move(Point newPosition) {
            //Set actual position as last valid
            LastPosition = ActualPosition;
            //And change actual postion to new
            ActualPosition = newPosition;
        }
    }
}
