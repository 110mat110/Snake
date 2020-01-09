using Snake.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake.Neural {
    public class NeuralNetwork {
        public List<Input> inputs = new List<Input>();
        public List<List<IInput>> Connections = new List<List<IInput>>();
        public List<List<Neuron>> NeuronLayers = new List<List<Neuron>>();
        int NoInputs = Settings.Default.Walls ? 12 : 8;
        const int NoOutputs = 2;


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
                    List<IInput> localConnections = new List<IInput>();
                    for(int j=0; j<NeuronLayers[layer].Count; j++){
                        localConnections.Add(new Connection((randomizer.NextDouble()*2)-1, NeuronLayers[layer][j], i, j ));
                    }
                    neurons.Add(new Neuron(localConnections));
                    connections.AddRange(localConnections);
                }
                Connections.Add(connections);
                NeuronLayers.Add(neurons);
            }
        }

        public NeuralNetwork(NeuralNetwork n1, NeuralNetwork n2, Random rng) {
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
                for (int i = 0; i < NoHiddenNeurons[layer]; i++) {
                    List<IInput> localConnections = new List<IInput>();
                    for (int j = 0; j < NeuronLayers[layer].Count; j++) {
                        localConnections.Add(new Connection(MutateWeight((Connection)n1.Connections[layer][i*j], (Connection)n2.Connections[layer][i*j], rng, Settings.Default.MutateRatio), NeuronLayers[layer][j],i,j));
                    }
                    neurons.Add(new Neuron(localConnections));
                    connections.AddRange(localConnections);
                }
                Connections.Add(connections);
                NeuronLayers.Add(neurons);
            }
        }

        private double MutateWeight(Connection c1, Connection c2, Random randomizer, double MutateRatio) {
            double ratio = randomizer.NextDouble();
            if (ratio > 0.5 + (MutateRatio / 2)) return c1.Weight;
            if (ratio < 0.5 - (MutateRatio / 2)) return c2.Weight;
            return randomizer.NextDouble();
        }

        public List<double> DoNeuralStuff(List<double> inputs) {
            if (inputs.Count != this.inputs.Count) throw new IndexOutOfRangeException("input length is not same than input count");

            for(int i=0; i< inputs.Count; i++) {
                this.inputs[i].Value = inputs[i];
            }

            List<double> output = new List<double>();
            for (int i = 0; i < NeuronLayers[NeuronLayers.Count-1].Count; i++)
                output.Add(NeuronLayers[NeuronLayers.Count - 1][i].GetOutput());

            return output;
        }

        public List<double> DoNeuralStuff(List<bool> inputs) {
            if (inputs.Count != this.inputs.Count) throw new IndexOutOfRangeException("input length is not same than input count");

            for (int i = 0; i < inputs.Count; i++) {
                this.inputs[i].Value = inputs[i] ? 1 : 0;
            }

            List<double> output = new List<double>();
            for (int i = 0; i < NeuronLayers[NeuronLayers.Count - 1].Count; i++)
                output.Add(NeuronLayers[NeuronLayers.Count - 1][i].GetOutput());

            return output;
        }
    }
}
