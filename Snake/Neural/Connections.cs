using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake.Neural {
    public class Connection : IInput {

        private Neuron SourceNeuron;
        public double Weight { get; private set; }

        public Connection(double weight, Neuron neuron) {
            Weight = weight;
            SourceNeuron = neuron;
        }

        public double GetValue() {
            return SourceNeuron.GetOutput() * Weight;
        }
    }
}
