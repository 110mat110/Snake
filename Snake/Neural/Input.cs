using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake.Neural {
    public class Input : IInput {
        public double GetValue() { return Value; }
        public double Value;

        public Input() { }
    }
}
