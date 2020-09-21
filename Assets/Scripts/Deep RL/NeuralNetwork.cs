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
    public NeuralNetwork(int[] layerTemplate)
    {
        this.neuralNetStructure = new int[layerTemplate.Length]; //  Create a new array of neural layers using the size of the layers parameter
        for (int i = 0; i < layerTemplate.Length; i++) // Iterate through each layer
            this.neuralNetStructure[i] = layerTemplate[i]; // Set the number of neurons in each layer

        layers = new Layer[layerTemplate.Length - 1];

        for (int i = 0; i < layers.Length; i++)
        {
            layers[i] = new Layer(layerTemplate[i], layerTemplate[i + 1]);
        }
    }

    public double[] FeedForward(double[] inputs)
    {
        layers[0].FeedForward(inputs, 0);
        for (int i = 1; i < layers.Length; i++)
        {
            layers[i].FeedForward(layers[i - 1].outputs, i);
        }
        return layers[layers.Length - 1].outputs;
    }

    public void Backpropagation(double[] targets)
    {
        for (int i = layers.Length - 1; i >= 0; i--)
        {
            if (i == layers.Length - 1)
            {
                layers[i].BackpropOutput(targets, i);
            }
            else
            {
                layers[i].BackpropHidden(layers[i + 1].gamma, layers[i + 1].weights, i);
            }
        }
        UpdateWeightsAndBiases();
    }

    public void UpdateWeightsAndBiases()
    {
        for (int i = 0; i < layers.Length; i++)
        {
            layers[i].Optimize(GameManager.instance.dqn ? GameManager.instance.dqn : GameManager.instance.dqnTester);
        }
    }
    public bool Train(DQN dqn, Environment env, Agent agent)
    {
        int miniBatchSize = GameManager.instance.settings.miniBatchSize; // Size of the mini batch used to train the target net

        // This Tuple array consists of one mini batch (random sample from experience replay buffer)
        Tuple<int, int, double, bool>[] miniBatch = agent.GetMiniBatch(miniBatchSize, env.fbIndex, GameManager.instance.settings.framesPerState);

        double[][] states = new double[miniBatchSize][];
        double[][] nextStates = new double[miniBatchSize][]; // Next State (The resulting state after an action)
        int[] actions = new int[miniBatchSize]; // The action performed
        double[] rewards = new double[miniBatchSize]; // Reward for the action
        bool[] dones = new bool[miniBatchSize]; // Boolean to indicate if the current mini batch is done (This is true on the last frame of an episode to prevent it from being used for training)

        // Unpack mini batch
        for (int i = 0; i < miniBatchSize; i++)
        {
            if (miniBatch[i] != null)
            {
                states[i] = env.GetState(miniBatch[i].Item1 - 1);
                nextStates[i] = env.GetState(miniBatch[i].Item1); // Next state ends with the last frame
                actions[i] = miniBatch[i].Item2;
                rewards[i] = miniBatch[i].Item3;
                dones[i] = miniBatch[i].Item4;
            }
        }

        for (int i = 0; i < miniBatchSize; i++) // Iterate through each mini batch
        {
            double[] qState = FeedForward(states[i]); // Not sure if this is needed
            double[] qNextState = FeedForward(nextStates[i]); // Needed for argmax calculation
            int argMax = GameManager.instance.math.ArgMax(qNextState);
            double[] target = dqn.targetNet.CalculateTargets(nextStates[i], rewards[i], dones[i], dqn, argMax); // Calculate the targets for the mini batch

            if (target != null) // Do not begin training until targets are set
            {
                double[] acts = FeedForward(states[i]);
                Backpropagation(target);
                dqn.cost = Cost(acts, target, acts.Length);
            }
        }
        return false;
    }

    public double[] OneHotProduct(double[] oneHot, double[] actions)
    {
        double[] product = new double[actions.Length];

        for (int i = 0; i < actions.Length; i++)
        {
            product[i] = oneHot[i] * actions[i];
        }
        return product;
    }
    public double[] OneHot(double[] actions)
    {
        double[] oneHotActions = new double[actions.Length];
        int maxIndex = 0;

        for (int i = 1; i < actions.Length - 1; i++) // Loop through each action
        {
            if (actions[maxIndex] > actions[i]) // If the action 
            {
                oneHotActions[maxIndex] = 1;
                oneHotActions[i] = 0;
            }
            else
            {
                oneHotActions[maxIndex] = 0;
                oneHotActions[i] = 1;
                maxIndex = i;
            }
        }
        return oneHotActions;
    }
    public double Cost(double[] actions, double[] targets, int actionQty)
    {
        double sum = 0;

        for (int i = 0; i < actionQty; i++)
        {
            double error = targets[i] - actions[i];
            sum += (error * error);
        }

        return sum / actionQty;
    }
    // Calculate the Targets for each mini batch
    public double[] CalculateTargets(double[] nextStates, double reward, bool done, DQN dqn, int argMax)
    {
        int aq = dqn.actionQty;

        double[] qNextStateTarget = FeedForward(nextStates);

        //GameManager.instance.math.Amax(qValues);

        double[] targets = new double[aq];

        for (int i = 0; i < aq; i++)
        {
            if (done == true)
            {
                targets[i] = reward;
            }
            else
            {
                //double d = done == true ? 0d : 1d;
                targets[i] = reward + (GameManager.instance.settings.gamma * qNextStateTarget[argMax]);
            }
        }
        return targets;
    }
}

[Serializable]
public class Layer
{
    private int inputQty;
    private int outputQty;

    public double[] outputs;
    public double[] inputs;
    public double[,] weights;
    public double[,] weightsDelta;
    public double[] gamma;
    public double[] error;
    public double[] biases;
    public int t;
    private double[,] firstMoment;
    private double[,] secondMoment;
    private double[] firstMomentBias;
    private double[] secondMomentBias;

    public Settings.LayerActivations[] activationPerLayer;

    public Layer(int numberOfInputs, int numberOfOutputs)
    {
        this.inputQty = numberOfInputs;
        this.outputQty = numberOfOutputs;

        outputs = new double[numberOfOutputs];
        inputs = new double[numberOfInputs];
        weights = new double[numberOfOutputs, numberOfInputs];
        weightsDelta = new double[numberOfOutputs, numberOfInputs];
        gamma = new double[numberOfOutputs];
        error = new double[numberOfOutputs];
        biases = new double[numberOfOutputs];
        activationPerLayer = GameManager.instance.settings.activations;

        // Adam Optimizer
        t = 0;
        firstMoment = new double[numberOfOutputs, numberOfInputs];
        secondMoment = new double[numberOfOutputs, numberOfInputs];
        firstMomentBias = new double[numberOfOutputs];
        secondMomentBias = new double[numberOfOutputs];

        Init_Weights();
        Init_Biases(); // TODO: FIX BIAS NODES
    }

    public void Init_Biases()
    {
        for (int i = 0; i < biases.Length; i++)
        {
            biases[i] = (double)UnityEngine.Random.Range(-0.5f, 0.5f); // Set random weight for biases
        }
    }
    public void Init_Weights()
    {
        for (int i = 0; i < outputQty; i++)
        {
            for (int j = 0; j < inputQty; j++)
            {
                weights[i, j] = (double)UnityEngine.Random.Range(-0.5f, 0.5f); // Set a random weight
            }
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
                outputs[i] += inputs[j] * weights[i, j];
            }
            outputs[i] = ActivationFunctions(outputs[i] + biases[i], layer);
        }
        if (activationPerLayer[layer] == Settings.LayerActivations.Softmax) // Apply softmax to output layer if necessary
        {
            outputs = GameManager.instance.math.Softmax(outputs);
        }

        //Debug.Log(outputs[outputs.Length - 1]);
        return outputs;
    }

    public void Optimize(DQN dqn)
    {
        if (!dqn.isConverged)
        {
            t++; // This is reset to 0 each episode which will therefore reset the learning rate
            dqn.currentLearningRate = GameManager.instance.settings.learningRate * Math.Sqrt(1 - Math.Pow(GameManager.instance.settings.beta2, t)) / (1 - Math.Pow(GameManager.instance.settings.beta1, t));

            UpdateWeights(dqn);
            UpdateBiases(dqn);
        }
    }
    private void UpdateWeights(DQN dqn)
    {
        for (int i = 0; i < outputQty; i++)
        {
            for (int j = 0; j < inputQty; j++)
            {
                double gradient = weightsDelta[i, j];
                if (Math.Abs(gradient) > GameManager.instance.settings.gradientThreshold)
                {
                    gradient *= GameManager.instance.settings.gradientThreshold / Math.Abs(gradient); // Clip gradient to prevent exploding and vanishing gradients
                }

                firstMoment[i, j] = (GameManager.instance.settings.beta1 * firstMoment[i, j]) + (1 - GameManager.instance.settings.beta1) * gradient;
                secondMoment[i, j] = (GameManager.instance.settings.beta2 * secondMoment[i, j]) + (1 - GameManager.instance.settings.beta2) * (gradient * gradient);

                weights[i, j] += dqn.currentLearningRate * firstMoment[i, j] / (Math.Sqrt(secondMoment[i, j]) + GameManager.instance.settings.epsilonHat);
            }
        }
    }
    private void UpdateBiases(DQN dqn)
    {
        for (int i = 0; i < outputQty; i++)
        {
            double biasGradient = gamma[i];

            if (Math.Abs(biasGradient) > GameManager.instance.settings.gradientThreshold)
            {
                biasGradient *= GameManager.instance.settings.gradientThreshold / Math.Abs(biasGradient); // Clip gradient to prevent exploding and vanishing gradients
            }

            firstMomentBias[i] = GameManager.instance.settings.beta1 * firstMomentBias[i] + (1 - GameManager.instance.settings.beta1) * biasGradient;
            secondMomentBias[i] = GameManager.instance.settings.beta2 * secondMomentBias[i] + (1 - GameManager.instance.settings.beta2) * (biasGradient * biasGradient);

            biases[i] += dqn.currentLearningRate * firstMomentBias[i] / (Math.Sqrt(secondMomentBias[i]) + GameManager.instance.settings.epsilonHat);
        }
    }
    public void BackpropOutput(double[] targets, int lay)
    {
        for (int i = 0; i < outputQty; i++)
            error[i] = targets[i] - outputs[i];

        for (int i = 0; i < outputQty; i++)
            gamma[i] = error[i] * ActivationDerivatives(outputs[i], lay);

        for (int i = 0; i < outputQty; i++)
        {
            for (int j = 0; j < inputQty; j++)
                weightsDelta[i, j] = gamma[i] * inputs[j];
        }
    }
    public void BackpropHidden(double[] gammaForward, double[,] weightsForward, int lay)
    {
        for (int i = 0; i < outputQty; i++)
        {
            gamma[i] = 0;
            for (int j = 0; j < gammaForward.Length; j++)
                gamma[i] += gammaForward[j] * weightsForward[j, i];

            gamma[i] *= ActivationDerivatives(outputs[i], lay);
        }

        for (int i = 0; i < outputQty; i++)
        {
            for (int j = 0; j < inputQty; j++)
            {
                weightsDelta[i, j] = gamma[i] * inputs[j];
            }
        }
    }
    public double ActivationFunctions(double value, int layer)
    {
        switch (activationPerLayer[layer])
        {
            case Settings.LayerActivations.Relu:
                return GameManager.instance.math.Relu(value);
            case Settings.LayerActivations.LeakyRelu:
                return GameManager.instance.math.LeakyRelu(value);
            case Settings.LayerActivations.Sigmoid:
                return GameManager.instance.math.Sigmoid(value);
            case Settings.LayerActivations.Tanh:
                return GameManager.instance.math.Tanh(value);
            case Settings.LayerActivations.Softmax: // Softmax relies on all of the output values to be calculated. Therefore, just return the value and softmax will be applied after all outputs are calculated
                return value;
            default:
                return GameManager.instance.math.Relu(value);
        }
    }
    public double ActivationDerivatives(double value, int layer)
    {
        switch (activationPerLayer[layer])
        {
            case Settings.LayerActivations.Relu:
                return GameManager.instance.math.ReluDerivative(value);
            case Settings.LayerActivations.LeakyRelu:
                return GameManager.instance.math.LeakyReluDerivative(value);
            case Settings.LayerActivations.Sigmoid:
                return GameManager.instance.math.SigmoidDerivative(value);
            case Settings.LayerActivations.Tanh:
                return GameManager.instance.math.TanhDerivative(value);
            case Settings.LayerActivations.Softmax:
                return GameManager.instance.math.SoftmaxDerivative(value);
            default:
                return GameManager.instance.math.ReluDerivative(value);
        }
    }
}

