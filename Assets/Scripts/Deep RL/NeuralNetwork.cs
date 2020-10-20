using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NeuralNetwork
{
    private int[] neuralNetStructure; // Contains the number of neurons in each layer
    public Layer[] layers;

    /// <summary>
    /// Neural Network Constructor
    /// </summary>
    /// <param name="layers"></param>
    public NeuralNetwork(int[] layerTemplate) // Used to create network for DQN
    {
        this.neuralNetStructure = new int[layerTemplate.Length]; //  Create a new array of neural layers using the size of the layers parameter
        
        for (int i = 0; i < layerTemplate.Length; i++) // Iterate through each layer
            this.neuralNetStructure[i] = layerTemplate[i]; // Set the number of neurons in each layer

        layers = new Layer[layerTemplate.Length - 1];

        for (int i = 0; i < layers.Length; i++)
            layers[i] = new Layer(layerTemplate[i], layerTemplate[i + 1], i);
    }
    public NeuralNetwork(int[] layerTemplate, bool isActor) // Used to create network for PPO
    {
        this.neuralNetStructure = new int[layerTemplate.Length]; //  Create a new array of neural layers using the size of the layers parameter

        for (int i = 0; i < layerTemplate.Length; i++) // Iterate through each layer
            this.neuralNetStructure[i] = layerTemplate[i]; // Set the number of neurons in each layer

        layers = new Layer[layerTemplate.Length - 1];

        for (int i = 0; i < layers.Length; i++)
            layers[i] = new Layer(layerTemplate[i], layerTemplate[i + 1], i, isActor);
    }
    public double[] FeedForward(double[] inputs)
    {
        layers[0].FeedForward(inputs, 0);
        for (int i = 1; i < layers.Length; i++)
            layers[i].FeedForward(layers[i - 1].outputs, i);

        return layers[layers.Length - 1].outputs;
    }

    public void Backpropagation(double[] targets)
    {
        for (int i = layers.Length - 1; i >= 0; i--)
        {
            if (i == layers.Length - 1)
                layers[i].BackpropOutput(targets, i);
            else
                layers[i].BackpropHidden(layers[i + 1].gamma, layers[i + 1].weights, i);
        }
        UpdateWeightsAndBiases();
    }

    public void UpdateWeightsAndBiases()
    {
        for (int i = 0; i < layers.Length; i++)
            layers[i].Optimize(RLManager.instance.rlComponent ? RLManager.instance.rlComponent : RLManager.instance.rlComponentTester);
    }
}

[Serializable]
public class Layer
{
    private int inputQty;
    private int outputQty;
    public double[] outputs;
    public double[] inputs;
    public double[][] weights;
    public double[][] weightsDelta;
    public double[] gamma;
    public double[] error;
    public double[] biases;
    public int t;
    private double[,] firstMoment;
    private double[,] secondMoment;
    private double[] firstMomentBias;
    private double[] secondMomentBias;
    private double learningRate;
    private double beta1;
    private double beta2;
    private double epsilonHat;

    public Settings.LayerActivations activation;

    public Layer(int numberOfInputs, int numberOfOutputs, int layerIndex, bool isActor)
    {
        Init_Net_Variables(numberOfInputs, numberOfOutputs);
        // if actorNet
        if (isActor)
        {
            activation = RLManager.instance.settings.actorActivations[layerIndex];
            learningRate = RLManager.instance.settings.actorLearningRate;
        }
        else
        {
            // if criticNet
            activation = RLManager.instance.settings.criticActivations[layerIndex];
            learningRate = RLManager.instance.settings.criticLearningRate;
        }
        Init_Weights();
        Init_Biases();
    }
    public Layer(int numberOfInputs, int numberOfOutputs, int layerIndex)
    {
        Init_Net_Variables(numberOfInputs, numberOfOutputs);
        activation = RLManager.instance.settings.activations[layerIndex];
        learningRate = RLManager.instance.settings.learningRate;
        Init_Weights();
        Init_Biases();
    }

    public void Init_Net_Variables(int numberOfInputs, int numberOfOutputs)
    {
        this.inputQty = numberOfInputs;
        this.outputQty = numberOfOutputs;
        outputs = new double[numberOfOutputs];
        inputs = new double[numberOfInputs];
        weights = new double[numberOfOutputs][];
        weightsDelta = new double[numberOfOutputs][];
        for (int i = 0; i < weights.Length; i++)
        {
            weights[i] = new double[numberOfInputs];
            weightsDelta[i] = new double[numberOfInputs];
        }
        gamma = new double[numberOfOutputs];
        error = new double[numberOfOutputs];
        biases = new double[numberOfOutputs];
        beta1 = RLManager.instance.settings.beta1;
        beta2 = RLManager.instance.settings.beta2;
        epsilonHat = RLManager.instance.settings.epsilonHat;
        // Adam Optimizer
        t = 0;
        firstMoment = new double[numberOfOutputs, numberOfInputs];
        secondMoment = new double[numberOfOutputs, numberOfInputs];
        firstMomentBias = new double[numberOfOutputs];
        secondMomentBias = new double[numberOfOutputs];
    }

    public void Init_Biases()
    {
        for (int i = 0; i < biases.Length; i++)
            biases[i] = (double)UnityEngine.Random.Range(-0.5f, 0.5f); // Set random weight for biases
    }
    public void Init_Weights()
    {
        for (int i = 0; i < outputQty; i++)
        {
            for (int j = 0; j < inputQty; j++)
                weights[i][j] = (double)UnityEngine.Random.Range(-0.5f, 0.5f); // Set a random weight
        }
    }
    public double[] FeedForward(double[] inputs, int layer)
    {
        this.inputs = inputs;

        for (int i = 0; i < outputQty; i++)
        {
            outputs[i] = 0;
            for (int j = 0; j < inputQty; j++)
            {
                outputs[i] += inputs[j] * weights[i][j];
            }
            
            outputs[i] = ActivationFunctions(outputs[i] + biases[i]);
        }

        if (activation == Settings.LayerActivations.Softmax) // Apply softmax to output layer if necessary
            outputs = RLManager.math.Softmax(outputs);

        //Debug.Log(outputs[outputs.Length - 1]);
        return outputs;
    }

    public void Optimize(RLComponent rl)
    {
        t++; // This is reset to 0 each episode which will therefore reset the learning rate
        rl.currentLearningRate = learningRate * Math.Sqrt(1 - Math.Pow(beta2, t)) / (1 - Math.Pow(beta1, t));

        UpdateWeights(rl);
        UpdateBiases(rl);
    }
    private void UpdateWeights(RLComponent rl)
    {
        for (int i = 0; i < outputQty; i++)
        {
            for (int j = 0; j < inputQty; j++)
            {
                double gradient = weightsDelta[i][j];

                firstMoment[i, j] = (beta1 * firstMoment[i, j]) + (1 - beta1) * gradient;
                secondMoment[i, j] = (beta2 * secondMoment[i, j]) + (1 - beta2) * (gradient * gradient);

                weights[i][j] += rl.currentLearningRate * firstMoment[i, j] / (Math.Sqrt(secondMoment[i, j]) + epsilonHat);
            }
        }
    }
    private void UpdateBiases(RLComponent rl)
    {
        for (int i = 0; i < outputQty; i++)
        {
            double biasGradient = gamma[i];

            firstMomentBias[i] = beta1 * firstMomentBias[i] + (1 - beta1) * biasGradient;
            secondMomentBias[i] = beta2 * secondMomentBias[i] + (1 - beta2) * (biasGradient * biasGradient);

            biases[i] += rl.currentLearningRate * firstMomentBias[i] / (Math.Sqrt(secondMomentBias[i]) + epsilonHat);
        }
    }
    public void BackpropOutput(double[] targets, int lay)
    {
        for (int i = 0; i < outputQty; i++)
            error[i] = targets[i] - outputs[i];

        for (int i = 0; i < outputQty; i++)
            gamma[i] = error[i] * ActivationDerivatives(outputs[i]);

        for (int i = 0; i < outputQty; i++)
        {
            for (int j = 0; j < inputQty; j++)
                weightsDelta[i][j] = gamma[i] * inputs[j];
        }
    }
    public void BackpropHidden(double[] gammaForward, double[][] weightsForward, int lay)
    {
        for (int i = 0; i < outputQty; i++)
        {
            gamma[i] = 0;

            for (int j = 0; j < gammaForward.Length; j++)
            {
                gamma[i] += gammaForward[j] * weightsForward[j][i];
            }

            gamma[i] *= ActivationDerivatives(outputs[i]);
        }

        for (int i = 0; i < outputQty; i++)
        {
            for (int j = 0; j < inputQty; j++)
                weightsDelta[i][j] = gamma[i] * inputs[j];
        }
    }
    public double ActivationFunctions(double value)
    {
        switch (activation)
        {
            case Settings.LayerActivations.Relu:
                return RLManager.math.Relu(value);
            case Settings.LayerActivations.LeakyRelu:
                return RLManager.math.LeakyRelu(value);
            case Settings.LayerActivations.Sigmoid:
                return RLManager.math.Sigmoid(value);
            case Settings.LayerActivations.Tanh:
                return RLManager.math.Tanh(value);
            case Settings.LayerActivations.Softmax: // Softmax relies on all of the output values to be calculated. Therefore, just return the value and softmax will be applied after all outputs are calculated
                return value;
            case Settings.LayerActivations.None:
                return value;
            default:
                return RLManager.math.Relu(value);
        }
    }
    public double ActivationDerivatives(double value)
    {
        switch (activation)
        {
            case Settings.LayerActivations.Relu:
                return RLManager.math.ReluDerivative(value);
            case Settings.LayerActivations.LeakyRelu:
                return RLManager.math.LeakyReluDerivative(value);
            case Settings.LayerActivations.Sigmoid:
                return RLManager.math.SigmoidDerivative(value);
            case Settings.LayerActivations.Tanh:
                return RLManager.math.TanhDerivative(value);
            case Settings.LayerActivations.Softmax:
                return RLManager.math.SoftmaxDerivative(value);
            case Settings.LayerActivations.None:
                return 1;
            default:
                return RLManager.math.ReluDerivative(value);
        }
    }
}

