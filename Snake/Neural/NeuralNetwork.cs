using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake.Neural {
    public class NeuralNetwork {
        public List<Input> inputs = new List<Input>();
        public List<Neuron> inputNeurons = new List<Neuron>();
        public List<IInput> FirstLayer = new List<IInput>();
        public List<Neuron> SecretLayer = new List<Neuron>();
        public List<IInput> SecondLayer = new List<IInput>();
        public List<Neuron> OutputLayer = new List<Neuron>();

        const int NoInputs = 6;
        const int NoOutputs = 4;
        public NeuralNetwork(int NoHiddenNeurons, Random randomizer) {

            for(int i =0; i<NoInputs; i++) {
                inputs.Add(new Input());
                inputNeurons.Add(new Neuron(new List<IInput>() { inputs[i] }));
            }

            for(int i = 0; i<NoHiddenNeurons; i++) {
                List<IInput> inputs = new List<IInput>();
                foreach(var inneuron in inputNeurons) {
                    inputs.Add(new Connection(randomizer.NextDouble(), inneuron));
                }
                SecretLayer.Add(new Neuron(inputs));
                FirstLayer.AddRange(inputs);
            }

            for (int i = 0; i < NoOutputs; i++) {
                List<IInput> inputs = new List<IInput>();
                foreach (var inneuron in SecretLayer) {
                    inputs.Add(new Connection(randomizer.NextDouble(), inneuron));
                }
                OutputLayer.Add(new Neuron(inputs));
                SecondLayer.AddRange(inputs);
            }
        }

        public NeuralNetwork(NeuralNetwork n1, NeuralNetwork n2) {
            Random rng = new Random();
            int NoHiddenNeurons = n1.SecretLayer.Count;

            for (int i = 0; i < NoInputs; i++) {
                inputs.Add(new Input());
                inputNeurons.Add(new Neuron(new List<IInput>() { inputs[i] }));
            }

            int j = 0;
            for (int i = 0; i < NoHiddenNeurons; i++) {
                List<IInput> inputs = new List<IInput>();
                foreach (var inneuron in inputNeurons) {
                    inputs.Add(new Connection(MutateWeight((Connection)n1.FirstLayer[j], (Connection)n2.FirstLayer[j], rng), inneuron));
                    j++;
                }
                SecretLayer.Add(new Neuron(inputs));
                FirstLayer.AddRange(inputs);
            }
            j = 0;
            for (int i = 0; i < NoOutputs; i++) {
                List<IInput> inputs = new List<IInput>();
                foreach (var inneuron in SecretLayer) {
                    inputs.Add(new Connection(MutateWeight((Connection)n1.SecondLayer[j], (Connection)n2.SecondLayer[j],rng), inneuron));
                    j++;
                }
                OutputLayer.Add(new Neuron(inputs));
                SecondLayer.AddRange(inputs);
            }
        }

        private double MutateWeight(Connection c1, Connection c2, Random randomizer) {
            double ratio = randomizer.NextDouble();
            return c1.Weight * ratio + c2.Weight * (1 - ratio) + randomizer.NextDouble()*0.05 - 0.025;
        }
        
        public List<double> DoNeuralStuff(List<double> inputs) {
            if (inputs.Count != this.inputs.Count) throw new IndexOutOfRangeException("input length is not same than input count");

            for(int i=0; i< inputs.Count; i++) {
                this.inputs[i].Value = inputs[i];
            }

            List<double> output = new List<double>();
            for (int i = 0; i < OutputLayer.Count; i++)
                output.Add(OutputLayer[i].GetOutput());

            return output;
        }
    }
}
