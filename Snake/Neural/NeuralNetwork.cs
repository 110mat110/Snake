using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake.Neural {
    public class NeuralNetwork {
        public List<Input> inputs = new List<Input>();
        /*
        public List<Neuron> inputNeurons = new List<Neuron>();
        public List<IInput> FirstLayer = new List<IInput>();
        public List<Neuron> SecretLayer = new List<Neuron>();
        public List<IInput> SecondLayer = new List<IInput>();
        public List<Neuron> OutputLayer = new List<Neuron>();
        */
        public List<List<IInput>> Connections = new List<List<IInput>>();
        public List<List<Neuron>> NeuronLayers = new List<List<Neuron>>();
        const int NoInputs = 6;
        const int NoOutputs = 4;


        public NeuralNetwork(List<int> NoHiddenNeurons, Random randomizer) {
            NoHiddenNeurons.Add(NoOutputs);
            List<Neuron> inputNeurons = new List<Neuron>();
            for (int i = 0; i < NoInputs; i++) {
                inputs.Add(new Input());
                
                inputNeurons.Add(new Neuron(new List<IInput>() { inputs[i] }));
            }

            NeuronLayers.Add(inputNeurons);
            for (int layer = 0; layer < NoHiddenNeurons.Count; layer++) {
                List<Neuron> neurons = new List<Neuron>();
                List<IInput> connections = new List<IInput>();
                for (int i = 0; i < NoHiddenNeurons[layer]; i++) {
                    foreach (var neuron in NeuronLayers[layer]) {
                        connections.Add(new Connection(randomizer.NextDouble(), neuron));
                    }

                    neurons.Add(new Neuron(connections));

                }
                Connections.Add(connections);
                NeuronLayers.Add(neurons);
            }
        }

        public NeuralNetwork(NeuralNetwork n1, NeuralNetwork n2, Random rng, double MutateRatio) {
            List<int> NoHiddenNeurons = new List<int>();
            //first layer is input and that is generated separately
            for(int i=1; i<n1.NeuronLayers.Count; i++) {
                NoHiddenNeurons.Add(n1.NeuronLayers[i].Count);
            }

            List<Neuron> inputNeurons = new List<Neuron>();
            for (int i = 0; i < NoInputs; i++) {
                inputs.Add(new Input());

                inputNeurons.Add(new Neuron(new List<IInput>() { inputs[i] }));
            }

            NeuronLayers.Add(inputNeurons);
            for (int layer = 0; layer < NoHiddenNeurons.Count; layer++) {
                List<Neuron> neurons = new List<Neuron>();
                List<IInput> connections = new List<IInput>();
                int j = 0;
                for (int i = 0; i < NoHiddenNeurons[layer]; i++) {
                    foreach (var neuron in NeuronLayers[layer]) {
                        connections.Add(new Connection(MutateWeight((Connection)n1.Connections[layer][j], (Connection)n2.Connections[layer][j], rng, MutateRatio), neuron));
                    }

                    neurons.Add(new Neuron(connections));

                }
                Connections.Add(connections);
                NeuronLayers.Add(neurons);
            }
        }
        /*
        private NeuralNetwork(int NoHiddenNeurons, Random randomizer) {

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

        public NeuralNetwork(NeuralNetwork n1, NeuralNetwork n2, Random rng, double MutateRatio) {
            int NoHiddenNeurons = n1.SecretLayer.Count;

            for (int i = 0; i < NoInputs; i++) {
                inputs.Add(new Input());
                inputNeurons.Add(new Neuron(new List<IInput>() { inputs[i] }));
            }

            int j = 0;
            for (int i = 0; i < NoHiddenNeurons; i++) {
                List<IInput> inputs = new List<IInput>();
                foreach (var inneuron in inputNeurons) {
                    inputs.Add(new Connection(MutateWeight((Connection)n1.FirstLayer[j], (Connection)n2.FirstLayer[j], rng, MutateRatio), inneuron));
                    j++;
                }
                SecretLayer.Add(new Neuron(inputs));
                FirstLayer.AddRange(inputs);
            }
            j = 0;
            for (int i = 0; i < NoOutputs; i++) {
                List<IInput> inputs = new List<IInput>();
                foreach (var inneuron in SecretLayer) {
                    inputs.Add(new Connection(MutateWeight((Connection)n1.SecondLayer[j], (Connection)n2.SecondLayer[j],rng, MutateRatio), inneuron));
                    j++;
                }
                OutputLayer.Add(new Neuron(inputs));
                SecondLayer.AddRange(inputs);
            }
        }
        /*old
        private double MutateWeight(Connection c1, Connection c2, Random randomizer, double MutateRatio) {
            double ratio = randomizer.NextDouble();
            return c1.Weight * ratio + c2.Weight * (1 - ratio) + randomizer.NextDouble()* MutateRatio - MutateRatio/2;
        }
        */
        private double MutateWeight(Connection c1, Connection c2, Random randomizer, double MutateRatio) {
            double ratio = randomizer.NextDouble();
            if (ratio > 0.5 + (MutateRatio / 2)) return c1.Weight;
            if (ratio < 0.5 - (MutateRatio / 2)) return c2.Weight;
            return randomizer.NextDouble();
        }

        public List<double> DoNeuralStuff(List<double> inputs) {
            if (inputs.Count != this.inputs.Count) throw new IndexOutOfRangeException("input length is not same than input count");
            /*
            for (int i = 0; i < inputs.Count; i++) {
                Debug.Write(inputs[i] + " ");
            }

            Debug.Write(">> "); 
            */
            for(int i=0; i< inputs.Count; i++) {
                this.inputs[i].Value = inputs[i];
            }

            List<double> output = new List<double>();
            for (int i = 0; i < NeuronLayers[NeuronLayers.Count-1].Count; i++)
                output.Add(NeuronLayers[NeuronLayers.Count - 1][i].GetOutput());
            /*
            for(int i=0; i< output.Count; i++) {
                Debug.Write(output[i] + " ");
            }
            Debug.WriteLine("");
            */
            return output;
        }
    }
}
