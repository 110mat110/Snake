﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake.Neural {
    public class Neuron {
        private List<IInput> Inputs;
        public Neuron(List<IInput> inputs) {
            this.Inputs = inputs;
        }

       public double GetOutput() {
            double x = 0;

            foreach (var i in Inputs)
                x += i.GetValue();
            return ActivationFunction(x);
       }

        private double ActivationFunction(double x) {
            return 1 / (1 + Math.Exp(x));
        }
    }
}