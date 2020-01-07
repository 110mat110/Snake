using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake.Neural {
    public class Connection : IInput {

        private Neuron SourceNeuron;
        public double Weight { get; private set; }

        public int SourceNeuronIndex = 0;
        public int DestinationNeuronIndex = 0;

        public Connection(double weight, Neuron neuron, int sourceNeuronIndex, int destinationNeuronIndex) {
            Weight = weight;
            SourceNeuron = neuron;
            this.DestinationNeuronIndex = destinationNeuronIndex;
            this.SourceNeuronIndex = sourceNeuronIndex;
        }

        public double GetValue() {
            return SourceNeuron.GetOutput() * Weight;
        }
    }
}
