using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class NeuralNetwork
{
    #region Variables
    private int[] neuralNetStructure; // Contains the number of neurons in each layer
    public Layer[] layers;

    public double[] frameMeans;
    public double[] zScores;
    public double[] totalSqrdDiff;
    public int sampleSize;
    #endregion

    #region Neural Network Constructor and Initialization
    /// <summary>
    /// Neural Network Constructor for DQN.
    /// </summary>
    /// <param name="layers"></param>
    public NeuralNetwork(int[] layerTemplate)
    {
        Init_Normalization(layerTemplate[0]);

        this.neuralNetStructure = new int[layerTemplate.Length]; //  Create a new array of neural layers using the size of the layers parameter
        
        for (int i = 0; i < layerTemplate.Length; i++) // Iterate through each layer
            this.neuralNetStructure[i] = layerTemplate[i]; // Set the number of neurons in each layer

        layers = new Layer[layerTemplate.Length - 1]; // Initialize output layer

        for (int i = 0; i < layers.Length; i++) // Initialize hidden layers
            layers[i] = new Layer(layerTemplate[i], layerTemplate[i + 1], i);
    }
    /// <summary>
    /// Neural network constructor for PPO (actor/critic).
    /// </summary>
    /// <param name="layerTemplate"></param>
    /// <param name="isActor"></param>
    public NeuralNetwork(int[] layerTemplate, bool isActor) // Used to create network for PPO
    {
        Init_Normalization(layerTemplate[0]);

        this.neuralNetStructure = new int[layerTemplate.Length]; //  Create a new array of neural layers using the size of the layers parameter

        for (int i = 0; i < layerTemplate.Length; i++) // Iterate through each layer
            this.neuralNetStructure[i] = layerTemplate[i]; // Set the number of neurons in each layer

        layers = new Layer[layerTemplate.Length - 1]; // Initialize output layer

        for (int i = 0; i < layers.Length; i++) // Initialize hidden layers
            layers[i] = new Layer(layerTemplate[i], layerTemplate[i + 1], i, isActor);
    }
    /// <summary>
    /// Initialize arrays and variables used for normalizing inputs.
    /// </summary>
    /// <param name="inputsPerState"></param>
    private void Init_Normalization(int inputsPerState)
    {
        sampleSize = 0;
        frameMeans = new double[inputsPerState];
        zScores = new double[inputsPerState];
        totalSqrdDiff = new double[inputsPerState];
    }
    #endregion

    #region FeedForward Net
    /// <summary>
    /// Forward pass of data through the neural network.
    /// </summary>
    /// <param name="inputs"></param>
    /// <returns></returns>
    public double[] FeedForward(double[] inputs)
    {
        //layers[0].FeedForward(NormalizeState(inputs, layers[0].inputs.Length)); 
        layers[0].FeedForward(inputs); // Feed forward inputs through the first hidden layer

        // Pass data through each hidden layer and end with output layer
        for (int i = 1; i < layers.Length; i++) 
            layers[i].FeedForward(layers[i - 1].outputs);

        return layers[layers.Length - 1].outputs; // Return the output layer
    }
    #endregion

    #region Backpropagation Net
    /// <summary>
    /// Backpropagation with only the targets as a parameter is used with DQN networks.
    /// Backpropagation will improve the neural network by adjusting the weights and biases.
    /// </summary>
    /// <param name="targets"></param>
    public void Backpropagation(double[] targets)
    {
        for (int i = layers.Length - 1; i >= 0; i--) // Iterate backwards through the network
        {
            if (i == layers.Length - 1)
                layers[i].BackpropOutput(targets); // Backprop weights and biases connected to output layer
            else
                layers[i].BackpropHidden(layers[i + 1].gamma, layers[i + 1].weights); // Backprop hidden layers
        }
        UpdateWeightsAndBiases(); // Use the gradients to optimize the network (Uses Adam Optimizer)
    }
    /// <summary>
    /// Backpropagation with errors and actions as parameters is used with PPO (Somewhat of a Policy Gradient Method)
    /// </summary>
    /// <param name="error"></param>
    /// <param name="actions"></param>
    public void Backpropagation(double[] error, int[] actions)
    {
        for (int i = layers.Length - 1; i >= 0; i--) // Iterate backwards through the network
        {
            if (i == layers.Length - 1)
                layers[i].BackpropOutput(error, actions); // Backprop weights and biases connected to output layer
            else
                layers[i].BackpropHidden(layers[i + 1].gamma, layers[i + 1].weights); // Backprop hidden layers
        }
        UpdateWeightsAndBiases(); // Use the gradients to optimize the network (Uses Adam Optimizer)
    }
    /// <summary>
    /// Update the weights and biases of the network using gradient descent. (Adam Optimizer)
    /// </summary>
    public void UpdateWeightsAndBiases()
    {
        for (int i = 0; i < layers.Length; i++) // Loop through each layer and optimize
            layers[i].Optimize();
    }
    #endregion

    #region Normalize State Inputs
    /// <summary>
    /// This method will normalize the inputs passed into the feed forward net. Uses sample mean.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="stateSize"></param>
    /// <returns></returns>
    public double[] NormalizeState(double[] state, int stateSize)
    {
        sampleSize++;
        double[] variances = new double[stateSize];
        double[] stdDeviations = new double[stateSize];

        for (int i = 0; i < stateSize; i++)
        {
            frameMeans[i] = RLManager.math.UpdateMean(sampleSize, frameMeans[i], state[i]);
            totalSqrdDiff[i] += RLManager.math.SquaredDifference(state[i], frameMeans[i]);
            variances[i] = RLManager.math.Variance(totalSqrdDiff[i], sampleSize);
            stdDeviations[i] = RLManager.math.StdDeviation(variances[i]);
            zScores[i] = RLManager.math.ZScore(state[i], frameMeans[i], stdDeviations[i]);
        }

        return zScores;
    }
    #endregion
}

/// <summary>
/// Layer class is initialized for each layer of a neural network Hidden/Output (does not include the Input layer).
/// </summary>
[Serializable]
public class Layer
{
    #region Neural Network Layer Variables
    private int inputQty; // Number of inputs to the layer
    private int outputQty; // Number of outputs from the layer
    public double[] outputs; // Array to store outputs (After activation function)
    public double[] inputs; // Array to store the inputs (values passed into the layer)
    private double[][] weightsDelta; // weightsdelta are the gradients before being passed through the Adam Optimizer
    public double[] gamma; // Not to be confused with the discount rate gamma, this gamma is used in backprop and = error * (activation function derivative)
    public double[] error; // The error used to begin backpropagation (only used in output layer)
    public double[][] weights; // 2-D array to store the network's weights
    public double[] biases; // Network biases (Constant that acts like the y-intercept)

    // The following are required for the Adam Optimizer
    private int t; // Tracks epochs and used to adjust learning rate with Adam Optimizer
    private double[,] firstMoment;
    private double[,] secondMoment;
    private double[] firstMomentBias;
    private double[] secondMomentBias;
    private double beta1;
    private double beta2;
    private double epsilonHat;
    private double learningRate; // Starting learning rate
    private double currentLearningRate; // Current learning rate calculated using Adam Optimizer

    public Settings.LayerActivations activation; // The activation function used on the current layer
    #endregion

    #region Layer Constructor and Initialization Methods
    /// <summary>
    /// Layer constructor for PPO algorithm (actor/critic)
    /// </summary>
    /// <param name="numberOfInputs"></param>
    /// <param name="numberOfOutputs"></param>
    /// <param name="layerIndex"></param>
    /// <param name="isActor"></param>
    public Layer(int numberOfInputs, int numberOfOutputs, int layerIndex, bool isActor)
    {
        Init_Net_Variables(numberOfInputs, numberOfOutputs); // Initialize to starting values
        
        // Set the learning rate and activation functions for the layer
        if (isActor) // if actorNet
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
        // Initialize weights and biases for the layer
        Init_Weights();
        Init_Biases();
    }
    /// <summary>
    /// Layer constructor for DQN.
    /// </summary>
    /// <param name="numberOfInputs"></param>
    /// <param name="numberOfOutputs"></param>
    /// <param name="layerIndex"></param>
    public Layer(int numberOfInputs, int numberOfOutputs, int layerIndex)
    {
        Init_Net_Variables(numberOfInputs, numberOfOutputs); // Initialize to starting values

        // Set the learning rate and activation functions for the layer
        activation = RLManager.instance.settings.dqnActivations[layerIndex];
        learningRate = RLManager.instance.settings.dqnLearningRate;

        // Initialize weights and biases for the layer
        Init_Weights();
        Init_Biases();
    }
    /// <summary>
    /// Initialize the layer's arrays and variables to their starting values.
    /// </summary>
    /// <param name="numberOfInputs"></param>
    /// <param name="numberOfOutputs"></param>
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
        currentLearningRate = 0;
    }
    /// <summary>
    /// Initialize biases for the layer using a random value between -0.5 and 0.5.
    /// </summary>
    public void Init_Biases()
    {
        for (int i = 0; i < biases.Length; i++)
            biases[i] = (double)UnityEngine.Random.Range(-0.5f, 0.5f); // Set random weight for biases
    }
    /// <summary>
    /// Initialize weights for the layer using a random value between -0.5 and 0.5.
    /// </summary>
    public void Init_Weights()
    {
        for (int i = 0; i < outputQty; i++)
        {
            for (int j = 0; j < inputQty; j++)
                weights[i][j] = (double)UnityEngine.Random.Range(-0.5f, 0.5f); // Set a random weight
        }
    }
    #endregion

    #region FeedForward Layer
    /// <summary>
    /// Feed forward the input data through the layer and return outputs.
    /// </summary>
    /// <param name="inputs"></param>
    /// <returns></returns>
    public double[] FeedForward(double[] inputs)
    {
        this.inputs = inputs; // Store inputs

        for (int i = 0; i < outputQty; i++) // Loop through each weight
        {
            outputs[i] = 0; // Reset output
            for (int j = 0; j < inputQty; j++) 
            {
                outputs[i] += inputs[j] * weights[i][j]; // Each output represents a neuron value = Sum(inputs * weights), for each respective neuron
            }
            
            outputs[i] = ActivationFunctions(outputs[i] + biases[i]); // Pass the neuron value through an activation function
        }

        if (activation == Settings.LayerActivations.Softmax) // This is used for softmax on the output layer only
            outputs = RLManager.math.Softmax(outputs);

        return outputs;
    }
    #endregion

    #region Backpropagation Layer
    /// <summary>
    /// DQN backpropagation. Takes an array of targets as a parameter (The size of the target array should = actionQty).
    /// </summary>
    /// <param name="targets"></param>
    public void BackpropOutput(double[] targets)
    {
        /* Loop through each action and calculate the error (This can also be target - output, but UpdateWeights() and UpdateBiases()
         must also be changed since this would create gradient ascent, thus increasing loss) */
        for (int i = 0; i < outputQty; i++)
            error[i] = outputs[i] - targets[i]; // error = output - target = actual - desired = Y - Y^hat

        // Calculate gamma = error * derivative of the activation function with respect to the output
        for (int i = 0; i < outputQty; i++)
            gamma[i] = error[i] * ActivationDerivatives(outputs[i]);

        // Calculate weightsDelta = gamma * inputs (Continuing the chain rule)
        for (int i = 0; i < outputQty; i++)
        {
            for (int j = 0; j < inputQty; j++)
                weightsDelta[i][j] = gamma[i] * inputs[j];
        }
    }
    /// <summary>
    /// PPO Backpropagation. Takes an array of errors and the array of one hot actions. One hot actions are required to determine derivative.
    /// </summary>
    /// <param name="errors"></param>
    /// <param name="actions"></param>
    public void BackpropOutput(double[] errors, int[] actions)
    {
        for (int i = 0; i < outputQty; i++)
        {
            // For PPO this is the output of the objective function. We use the negative of the log probabilities since we want to run gradient ascent with the PPO actor network.
            error[i] = errors[i]; 
        }
        for (int i = 0; i < outputQty; i++)
        {
            if (actions[i] == 0) // If action is encoded to zero
            {
                gamma[i] = error[i] * outputs[i];
            }
            else // If action is encoded to 1 (will only be one of the actions)
            {
                gamma[i] = error[i] * ActivationDerivatives(outputs[i]); // gamma = error * (output - 1)
            }
        }
        // Calculate weightsDelta = gamma * inputs (Continuing the chain rule)
        for (int i = 0; i < outputQty; i++)
        {
            for (int j = 0; j < inputQty; j++)
                weightsDelta[i][j] = gamma[i] * inputs[j];
        }
    }
    /// <summary>
    /// Backpropagation for hidden layers. Can be used with PPO and DQN
    /// </summary>
    /// <param name="gammaForward"></param>
    /// <param name="weightsForward"></param>
    public void BackpropHidden(double[] gammaForward, double[][] weightsForward)
    {
        for (int i = 0; i < outputQty; i++) // Iterate through each weight
        {
            gamma[i] = 0;

            for (int j = 0; j < gammaForward.Length; j++)
            {
                // Calculate current layer gamma using the prior layer's gamma (which is actually the next index since this is backpropagation)
                gamma[i] += gammaForward[j] * weightsForward[j][i]; 
            }

            gamma[i] *= ActivationDerivatives(outputs[i]); // Multiply gamma by the derivative of the activation function
        }
        // Calculate weightsDelta = gamma * input (Continuing the chain rule)
        for (int i = 0; i < outputQty; i++)
        {
            for (int j = 0; j < inputQty; j++)
                weightsDelta[i][j] = gamma[i] * inputs[j];
        }
    }
    /// <summary>
    /// Runs Adam Optimizer.
    /// </summary>
    public void Optimize()
    {
        t++; // This is reset to 0 each episode which will therefore reset the learning rate
        // Calculate the current learning rate
        currentLearningRate = learningRate * Math.Sqrt(1 - Math.Pow(beta2, t)) / (1 - Math.Pow(beta1, t));

        UpdateWeights();
        UpdateBiases();
    }
    /// <summary>
    /// Update the network's weights using gamma with Adam Optimizer
    /// </summary>
    private void UpdateWeights()
    {
        for (int i = 0; i < outputQty; i++) // Iterate through each weight
        {
            for (int j = 0; j < inputQty; j++)
            {
                double gradient = weightsDelta[i][j]; // Set the gradient
                // Main part of Adam Optimizer
                firstMoment[i, j] = (beta1 * firstMoment[i, j]) + (1 - beta1) * gradient; // First moment bias calculation
                secondMoment[i, j] = (beta2 * secondMoment[i, j]) + (1 - beta2) * (gradient * gradient); // Second moment bias calculation
                weights[i][j] -= currentLearningRate * firstMoment[i, j] / (Math.Sqrt(secondMoment[i, j]) + epsilonHat); // Update weights using gradient descent
            }
        }
    }
    /// <summary>
    /// Update the network's biases using gamma with Adam Optimizer
    /// </summary>
    private void UpdateBiases()
    {
        for (int i = 0; i < outputQty; i++) // Iterate through each bias
        {
            double biasGradient = gamma[i]; // Set the gradient
            // Main part of Adam Optimizer
            firstMomentBias[i] = beta1 * firstMomentBias[i] + (1 - beta1) * biasGradient; // First moment bias calculation
            secondMomentBias[i] = beta2 * secondMomentBias[i] + (1 - beta2) * (biasGradient * biasGradient); // Second moment bias calculation
            biases[i] -= currentLearningRate * firstMomentBias[i] / (Math.Sqrt(secondMomentBias[i]) + epsilonHat); // Update biases using gradient descent
        }
    }
    #endregion

    #region Activation Functions and Derivatives
    /// <summary>
    /// This method will take a value, pass it through an activation function and return the output.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
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
            // Softmax relies on all of the output values to be calculated. Therefore, just return the value and softmax will be applied after all outputs are calculated
            case Settings.LayerActivations.Softmax: 
                return value;
            case Settings.LayerActivations.Linear:
                return value;
            default:
                return RLManager.math.Relu(value);
        }
    }
    /// <summary>
    /// This method will take a value and calculate and return the activation function derivative.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
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
            case Settings.LayerActivations.Linear:
                return 1;
            default:
                return RLManager.math.ReluDerivative(value);
        }
    }
    #endregion
}

