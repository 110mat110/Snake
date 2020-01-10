using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake.Neural {
    public class Neuron {
        private List<IInput> Inputs;
        double bias = 0;
        public Neuron(List<IInput> inputs) {
            this.Inputs = inputs;
        }
        public double LastOutput = 0;

       public double GetOutput() {
            double x = 0;

            foreach (var i in Inputs)
                x += i.GetValue();
            x += bias;
            LastOutput = ActivationFunction(x);
            return LastOutput;
       }
        /*
        private double ActivationFunction(double x) {
            return 1 / (1 + Math.Exp(-0.75*x));
        }
        */
        private double ActivationFunction(double x) {
            return x > 0? 1 : 0;
        }
    }
}
