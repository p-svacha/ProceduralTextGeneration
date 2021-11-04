using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NeuralNetwork
{

    public int[] layer;
    public Layer[] layers;
    public System.Random random;
    public int TestCounter;

    private ActivationFunction ActivationFunction;

    /// <summary>
    /// Instantiate a new random network with the given layers
    /// </summary>
    public NeuralNetwork(int[] layer, ActivationFunctionType af, float learningRate = 0.008f)
    {
        switch(af)
        {
            case ActivationFunctionType.Sigmoid:
                ActivationFunction = new AF_Sigmoid();
                break;
            case ActivationFunctionType.TanH:
                ActivationFunction = new AF_TanH();
                break;
        }

        random = new System.Random();
        this.layer = new int[layer.Length];
        for(int i = 0;i < layer.Length; i++) this.layer[i] = layer[i];

        layers = new Layer[layer.Length - 1];
        for(int i = 0; i < layers.Length; i++) layers[i] = new Layer(layer[i], layer[i + 1], ActivationFunction, learningRate);
    }

    /// <summary>
    /// Initialize a neural network with the given layers and weights
    /// </summary>
    public NeuralNetwork(List<int> layer, List<float[,]> weights, ActivationFunctionType af, float learningRate = 0.008f)
    {
        if (layer.Count - 1 != weights.Count) throw new Exception("Wrong amount of layer weights. Expected: " + (layer.Count - 1) + ", Actual: " + weights.Count);

        switch (af)
        {
            case ActivationFunctionType.Sigmoid:
                ActivationFunction = new AF_Sigmoid();
                break;
            case ActivationFunctionType.TanH:
                ActivationFunction = new AF_TanH();
                break;
        }

        random = new System.Random();
        this.layer = new int[layer.Count];
        for (int i = 0; i < layer.Count; i++) this.layer[i] = layer[i];

        layers = new Layer[layer.Count - 1];
        for (int i = 0; i < layers.Length; i++) layers[i] = new Layer(layer[i], layer[i + 1], ActivationFunction, learningRate, weights[i]);
    }

    /// <summary>
    /// Calculate the outputs of the neural network according to given inputs.
    /// </summary>
    /// <param name="inputs"></param>
    /// <returns></returns>
    public float[] FeedForward(float[] inputs)
    {
        layers[0].FeedForward(inputs);
        for(int i = 1; i < layers.Length; i++)
        {
            layers[i].FeedForward(layers[i - 1].outputs);
        }

        return layers[layers.Length - 1].outputs.ToArray();
    }

    public void BackPropagation(float[] expected)
    {
        for (int i = layers.Length - 1; i >= 0; i--)
        {
            if (i == layers.Length - 1)
            {
                layers[i].BackPropOutput(expected);
            }
            else
            {
                layers[i].BackPropHidden(layers[i + 1].gamma, layers[i + 1].weights);
            }
        }

        for (int i = 0; i < layers.Length; i++)
        {
            layers[i].UpdateWeights();
        }
    }

    public class Layer
    {
        public ActivationFunction ActivationFunction;

        static float LearningRate;

        int numberOfInputs; //# of neurons in previous layer
        int numberOfOutputs; //# of neurons in current layer

        private float[] inputs;
        public float[] outputs;
        public float[,] weights;
        private float[,] weightsDelta;
        public float[] gamma;
        private float[] error;
        private static System.Random random = new System.Random();


        public Layer(int numberOfInputs, int numberOfOutputs, ActivationFunction af, float learningRate, float[,] initWeights = null)
        {
            ActivationFunction = af;
            LearningRate = learningRate;
            this.numberOfInputs = numberOfInputs;
            this.numberOfOutputs = numberOfOutputs;

            inputs = new float[numberOfInputs];
            outputs = new float[numberOfOutputs];
            weightsDelta = new float[numberOfOutputs, numberOfInputs];
            gamma = new float[numberOfOutputs];
            error = new float[numberOfOutputs];

            if (initWeights == null)
            {
                weights = new float[numberOfOutputs, numberOfInputs];
                InitializeWeights();
            }
            else
            {
                if(initWeights.GetLength(0) != numberOfOutputs || initWeights.GetLength(1) != numberOfInputs)
                    throw new Exception("Layer initialization failed because weights do not match expected number of inputs/outputs: Expected: " + numberOfOutputs + "/" + numberOfInputs + ", Actual: " + initWeights.GetLength(0) + "/" + initWeights.GetLength(1)); ;

                weights = new float[numberOfOutputs, numberOfInputs];
                for (int i = 0; i < numberOfOutputs; i++)
                {
                    for (int j = 0; j < numberOfInputs; j++)
                    {
                        weights[i, j] = initWeights[i, j];
                    }
                }
            }
        }

        public void InitializeWeights()
        {
            for (int i = 0; i < numberOfOutputs; i++)
            {
                for (int j = 0; j < numberOfInputs; j++)
                {
                    weights[i, j] = (float)random.NextDouble() - 0.5f;
                }
            }
        }

        /// <summary>
        /// Calculate the outputs in each layer according to given inputs.
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public float[] FeedForward(float[] inputs)
        {
            this.inputs = inputs;

            for (int i = 0; i < numberOfOutputs; i++)
            {
                outputs[i] = 0;
                for (int j = 0; j < numberOfInputs; j++)
                {
                    outputs[i] += inputs[j] * weights[i, j];
                }

                outputs[i] = ActivationFunction.GetValue(outputs[i]);
            }

            return outputs;
        }

        /// <summary>
        /// Call this for back Propagation if this layer is an output layer.
        /// </summary>
        /// <param name="expected"></param>
        public void BackPropOutput(float[] expected)
        {
            for (int i = 0; i < numberOfOutputs; i++)
            {
                error[i] = outputs[i] - expected[i];
            }

            for (int i = 0; i < numberOfOutputs; i++)
            {
                gamma[i] = error[i] * ActivationFunction.GetDerivativeValue(outputs[i]);
            }

            for (int i = 0; i < numberOfOutputs; i++)
            {
                for (int j = 0; j < numberOfInputs; j++)
                {
                    weightsDelta[i, j] = gamma[i] * inputs[j];
                }
            }
        }

        /// <summary>
        /// Call this for back propagation if this is a hidden layer.
        /// </summary>
        public void BackPropHidden(float[] gammaForward, float[,] weightsForward)
        {
            for(int i = 0; i < numberOfOutputs; i++)
            {
                gamma[i] = 0;
                for(int j = 0; j < gammaForward.Length; j++)
                {
                    gamma[i] += gammaForward[j] * weightsForward[j, i];
                }
                gamma[i] *= ActivationFunction.GetDerivativeValue(outputs[i]);
            }

            for (int i = 0; i < numberOfOutputs; i++)
            {
                for (int j = 0; j < numberOfInputs; j++)
                {
                    weightsDelta[i, j] = gamma[i] * inputs[j];
                }
            }
        }

        /// <summary>
        /// Update the weights according to the weightsDelta
        /// </summary>
        public void UpdateWeights()
        {
            for(int i = 0; i < numberOfOutputs; i++)
            {
                for(int j = 0; j < numberOfInputs; j++)
                {
                    weights[i, j] -= weightsDelta[i, j] * LearningRate;
                }
            }
        }
    }
}
