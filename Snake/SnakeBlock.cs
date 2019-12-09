using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Snake {
    public class SnakeBlock {

        public Point ActualPosition { get; private set; }
        public Point LastPosition { get; private set; }

        public SnakeBlock() {

        }

        public SnakeBlock(Point position) {
            ActualPosition = position;
            LastPosition = position;
        }

        public void Move(Point position) {
            LastPosition = ActualPosition;
            ActualPosition = position;
        }
    }
}
