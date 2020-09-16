//using System;
//using System.Collections.Generic;
//using UnityEngine;

//[Serializable]
//public class NeuralNet
//{
//    private int[] neuralNetStructure; // Contains the number of neurons in each layer
//    private double[][] neuronSums; // Contains all neuron values before they are passed to activation function
//    private double[][] neuronsActivated; // Contains all neuron values when they are passed to activation function
//    private double[][] biases; // Contains all the biases for each neuron
//    private double[][] biasGradientSingle; // Contains the bias gradients for each neuron
//    private double[][][] weightsMatrix; // Contains all the weights for the neural network
//    private double[][][] weightsGradientSingle; // Contains all the gradients for the weights
//    private double[][] signals; // Signals for each neuron (Used in back prop)

//    public Settings.LayerActivations[] activationPerLayer;

//    private double[][][] weightsGradientSums;
//    private double[][] biasGradientSums;

//    // The following are used in the Adam Optimizer calculation
//    private double[][][] firstMoment;
//    private double[][][] secondMoment;
//    private double[][] firstMomentBias;
//    private double[][] secondMomentBias;

//    public int t = 0; // Timesteps (epochs)
//    public int tx = 0; // This is used to calculate learning rate decay
//    public double avgCost; // Average of the cost function for current episode (Huber Loss)
//    public double totalCost; // Total of the cost function over all episodes TODO: Display the average of this

//    public double[][][] GetWeightsMatrix()
//    {
//        return weightsMatrix;
//    }
//    public double[][] GetBiases()
//    {
//        return biases;
//    }

//    public double[][] GetNeuronSums()
//    {
//        return neuronSums;
//    }
//    public void SetWeightsMatrix(double[][][] weightsToCopy)
//    {
//        weightsMatrix = weightsToCopy;
//    }
//    public void SetBiases(double[][] biasesToCopy)
//    {
//        biases = biasesToCopy;
//    }
//    /// <summary>
//    /// Neural Network Constructor
//    /// </summary>
//    /// <param name="layers"></param>
//    public NeuralNet(int[] layers)
//    {
//        this.neuralNetStructure = new int[layers.Length]; //  Create a new array of neural layers using the size of the layers parameter

//        for (int lay = 0; lay < layers.Length; lay++) // Iterate through each layer
//        {
//            this.neuralNetStructure[lay] = layers[lay]; // Set the number of neurons in each layer 
//        }

//        //activationPerLayer = GameManager.instance.dqn.activations;

//        // Initialize all required Matrices
//        neuronsActivated = Init_Neurons_Matrix(); // Initialize neuron matrix to hold neuron values after they have gone through activation function
//        neuronSums = Init_Neurons_Matrix(); // Initialize neuron matrix to hold neuron sums (before they are normalized by activation function)
//        biases = Init_Bias_Matrix(); // Initialize a bias matrix
//        weightsMatrix = Init_Weights_Matrix(true); // Initialize weight matrix, isRandom set to true for random weights
//        signals = Init_Neurons_Matrix(); // Initialize a signal matrix, same structure as a neurons matrix
//        weightsGradientSingle = Init_Weights_Matrix(false); // Initialize a weights gradient, all to zero
//        weightsGradientSums = Init_Weights_Matrix(false); // Initialize a weights gradient, all to zero
//        biasGradientSingle = Init_Bias_Matrix(); // Initialize a gradient matrix for the biases
//        biasGradientSums = Init_Bias_Matrix();
//        activationPerLayer = GameManager.instance.settings.activations;
//    }
//    /// <summary>
//    /// Initialize a Neurons Matrix
//    /// </summary>
//    /// <returns></returns>
//    private double[][] Init_Neurons_Matrix()
//    {
//        List<double[]> neurons = new List<double[]>(); // Create an empty neurons matrix list

//        for (int lay = 0; lay < neuralNetStructure.Length; lay++) // Iterate through each layer
//        {
//            neurons.Add(new double[neuralNetStructure[lay]]); // Add an array where each neuron is an index in that array
//        }

//        return neurons.ToArray(); // Convert the neurons list to an array
//    }
//    /// <summary>
//    /// Initialize a Bias Matrix (Init all biases with a random value)
//    /// </summary>
//    /// <returns></returns>
//    private double[][] Init_Bias_Matrix()
//    {
//        List<double[]> biasMatrix = new List<double[]>(); // Create an empty list

//        for (int lay = 0; lay < neuralNetStructure.Length; lay++) // Iterate through each layer
//        {
//            double[] bias = new double[neuralNetStructure[lay]]; // Create a new 1-D array for each layer, size of the array based on netStructure

//            for (int b = 0; b < neuralNetStructure[lay]; b++) // Iterate through each bias
//            {
//                bias[b] = (double)UnityEngine.Random.Range(-0.5f, 0.5f); // Set the bias to a random float between -0.5 and 0.5
//            }

//            biasMatrix.Add(bias); // Add the bias array to the biases List
//        }
//        return biasMatrix.ToArray(); // Return list converted to an array
//    }
//    /// <summary>
//    /// Initialize a 3 dimensional weights matrix
//    /// </summary>
//    /// <param name="isRandom"></param>
//    /// <returns></returns>
//    private double[][][] Init_Weights_Matrix(bool isRandom)
//    {
//        List<double[][]> weightsMatrix = new List<double[][]>(); // Create a temporary 3-D list for the weights

//        for (int lay = 1; lay < neuralNetStructure.Length; lay++) // Iterate through each layer
//        {
//            List<double[]> layers = new List<double[]>(); // Create a new list for each layer

//            int neuronsInPreviousLayer = neuralNetStructure[lay - 1]; // Calculate the number of neurons in the previous layer (This is the number of weights that are connected to a specific neuron)

//            for (int neu = 0; neu < neuralNetStructure[lay]; neu++) // Iterate through each neuron
//            {
//                double[] weight = new double[neuronsInPreviousLayer]; // Create an array of weights for each neuron

//                for (int wt = 0; wt < neuronsInPreviousLayer; wt++) // Iterate through each weight
//                {
//                    if (isRandom) // If random (used for weights)
//                    {
//                        weight[wt] = (double)UnityEngine.Random.Range(-0.5f, 0.5f); // Set a random weight
//                    }
//                    else // If not random (used for firstMoment and secondMoment matrices in Adam Optimizer)
//                    {
//                        weight[wt] = 0d; 
//                    }
//                }

//                layers.Add(weight); // Add weight to layers
//            }

//            weightsMatrix.Add(layers.ToArray()); // Add the layers to the weightsMatrix
//        }

//        return weightsMatrix.ToArray(); // Return the weightsMatrix
//    }
//    public double ActivationFunctions(double value, int layer)
//    {
//        switch (activationPerLayer[layer])
//        {
//            case Settings.LayerActivations.Relu:
//                return GameManager.instance.math.Relu(value);
//            case Settings.LayerActivations.LeakyRelu:
//                return GameManager.instance.math.LeakyRelu(value);
//            case Settings.LayerActivations.Sigmoid:
//                return GameManager.instance.math.Sigmoid(value);
//            case Settings.LayerActivations.Tanh:
//                return GameManager.instance.math.Tanh(value);
//            case Settings.LayerActivations.Softmax: // Softmax relies on all of the output values to be calculated. Therefore, just return the value and softmax will be applied after all outputs are calculated
//                return value;
//            default:
//                return GameManager.instance.math.Relu(value);
//        }
//    }
//    public double ActivationDerivatives(double value, int layer)
//    {
//        switch (activationPerLayer[layer])
//        {
//            case Settings.LayerActivations.Relu:
//                return GameManager.instance.math.ReluDerivative(value);
//            case Settings.LayerActivations.LeakyRelu:
//                return GameManager.instance.math.LeakyReluDerivative(value);
//            case Settings.LayerActivations.Sigmoid:
//                return GameManager.instance.math.SigmoidDerivative(value);
//            case Settings.LayerActivations.Tanh:
//                return GameManager.instance.math.TanhDerivative(value);
//            case Settings.LayerActivations.Softmax:
//                return GameManager.instance.math.SoftmaxDerivative(value);
//            default:
//                return GameManager.instance.math.ReluDerivative(value);
//        }
//    }

//    /// <summary>
//    /// Feed inputs through the neural network and returns an output
//    /// </summary>
//    /// <param name="inputs"></param>
//    /// <returns></returns>
//    public double[] FeedForward(double[] inputs)
//    {
//        for (int i = 0; i < inputs.Length; i++) // TODO: This might cause an error
//        {
//            neuronsActivated[0][i] = inputs[i];// ActivationFunctions(inputs[i], 0); // Set the input layer with data passed in
//        }
//        for (int lay = 1; lay < neuralNetStructure.Length; lay++) // Iterate through each layer starting with the first hidden layer
//        {
//            for (int neu = 0; neu < neuronsActivated[lay].Length; neu++) // Iterate through each neuron in the layer
//            {
//                double neuronValue = 0.0d;

//                for (int wt = 0; wt < neuronsActivated[lay - 1].Length; wt++) // Iterate through each neuron in the previous layer
//                {
//                    neuronValue += weightsMatrix[lay - 1][neu][wt] * neuronsActivated[lay - 1][wt];
//                }

//                neuronSums[lay][neu] = neuronValue; //biases[lay][neu]; // Save the neuronSum, needed for back propagation 

//                neuronsActivated[lay][neu] = ActivationFunctions(neuronSums[lay][neu] + biases[lay][neu], lay);
//            }
//        }
//        if (activationPerLayer[activationPerLayer.Length - 1] == Settings.LayerActivations.Softmax) // Apply softmax to output layer if necessary
//        {
//            neuronsActivated[neuronsActivated.Length - 1] = GameManager.instance.math.Softmax(neuronsActivated[neuronsActivated.Length - 1]);
//        }

//        return neuronsActivated[neuronsActivated.Length - 1]; // Return the output layer
//    }

//    public void SumGradients()
//    {
//        for (int lay = 0; lay < weightsGradientSums.Length; lay++)
//        {
//            for (int neu = 0; neu < weightsGradientSums[lay].Length; neu++)
//            {
//                for (int wt = 0; wt < weightsGradientSums[lay][neu].Length; wt++)
//                {
//                    weightsGradientSums[lay][neu][wt] += weightsGradientSingle[lay][neu][wt];
//                }
//            }
//        }

//        for (int lay = 0; lay < biasGradientSums.Length; lay++)
//        {
//            for (int neu = 0; neu < biasGradientSums[lay].Length; neu++)
//            {
//                biasGradientSums[lay][neu] += biasGradientSingle[lay][neu];
//            }
//        }
//    }
//    /// <summary>
//    /// Trains the neural network
//    /// </summary>
//    /// <param name="expBuffer"></param>
//    /// <param name="weights"></param>
//    /// <param name="neurons"></param>
//    /// <param name="dqn"></param>
//    /// <param name="env"></param>
//    /// <param name="agent"></param>
//    /// <returns></returns>
//    public bool Train(DQN dqn, Environment env, Agent agent)
//    {
//        int miniBatchSize = GameManager.instance.settings.miniBatchSize; // Size of the mini batch used to train the target net
//        int actionQty = dqn.actionQty; // Number of possible actions the agent can perform (AKA number of outputs)

//        // This Tuple array consists of one mini batch (random sample from experience replay buffer)
//        Tuple<int, double[], double, bool>[] miniBatch = agent.GetMiniBatch(miniBatchSize);

//        double[][] states = new double[miniBatchSize][]; // States (AKA inputs)
//        double[][] nextStates = new double[miniBatchSize][]; // Next States (The resulting state after an action)
//        double[][] actions = new double[miniBatchSize][]; // Actions performed
//        double[] rewards = new double[miniBatchSize]; // Reward for the action
//        bool[] dones = new bool[miniBatchSize]; // Boolean to indicate if the current mini batch is done (This is true on the last frame of an episode to prevent it from being used for training)

//        // Unpack mini batch
//        for (int i = 0; i < miniBatchSize; i++)
//        {
//            if (miniBatch[i] != null)
//            {
//                states[i] = env.GetState(miniBatch[i].Item1 - 1); // State ends with the second to last frame in the set
//                nextStates[i] = env.GetState(miniBatch[i].Item1); // Next state ends with the last frame
//                actions[i] = miniBatch[i].Item2;
//                rewards[i] = miniBatch[i].Item3;
//                dones[i] = miniBatch[i].Item4;
//            }
//        }

//        for (int i = 0; i < miniBatchSize; i++) // Iterate through each mini batch
//        {
//            double[] targets = CalculateTargets(nextStates[i], rewards[i], dones[i], dqn); // Calculate the targets for the mini batch
//            double costs = Cost(actions[i], targets, actionQty); // Calculate the cost (Huber Loss) // TODO: FIX THIS TO DISPLAY COST PROPERLY
//            avgCost = costs; //GameManager.instance.math.AverageCost(dqn, costs); // Calculate average cost to display on HUD

//            if (targets != null) // Do not begin training until targets are set
//            {
//                double[] errors = CalculateErrors(targets, actions[i], actionQty); // Calculate errors for the batch

//                CalculateGradients(actions, errors, i, dqn); // Calculate gradients for the batch
//                SumGradients();
//            }
//        }

//        Optimize(dqn); // Update the weights
//        Debug.Log("No Move: " + neuronsActivated[neuronsActivated.Length - 1][0] + "  Forward: " + neuronsActivated[neuronsActivated.Length - 1][1] + "  Back: " + neuronsActivated[neuronsActivated.Length - 1][2] +
//        "  Right: " + neuronsActivated[neuronsActivated.Length - 1][3] + "  Left: " + neuronsActivated[neuronsActivated.Length - 1][4]);
//        //Debug.Log(weightsMatrix[0][2][3]);
//        return false;
//    }

//    // Calculate the Targets for each mini batch
//    private double[] CalculateTargets(double[] nextStates, double reward, bool done, DQN dqn)
//    {
//        //int mbs = GameManager.instance.settings.miniBatchSize; 
//        int aq = dqn.actionQty; 

//        double[] qValues = dqn.targetNet.FeedForward(nextStates);

//        double[] targets = new double[aq];

//        for (int i = 0; i < aq; i++)
//        {
//            double d = done == true ? 0f : 1f;
//            targets[i] = reward + (d * GameManager.instance.settings.gamma * qValues[i]);
//        }

//        return targets;
//    }

//    private double Cost(double[] actions, double[] targets, int actionQty)
//    {
//        double cost = 0;
        
//        for (int i = 0; i < actionQty; i++)
//        {
//            double error = actions[i] - targets[i];
//            cost += (error * error);
//        }

//        cost = cost / 2;

//        //float delta = 1.0f;
//        //double[] selected_Actions = new double[actionQty];
//        //double[] costs = new double[actionQty];

//        //selected_Actions = GameManager.instance.math.SelectedActions(actions);

//        //costs = new double[actionQty];
//        //for (int i = 0; i < actionQty; i++)
//        //{
//        //    double error = selected_Actions[i] - targets[i];
//        //    if (Math.Abs(error) <= delta)
//        //    {
//        //        costs[i] = 0.5f * error * error;
//        //    }
//        //    else
//        //    {
//        //        costs[i] = delta * Math.Abs(error - 0.5f * (delta * delta));
//        //    }
//        //}
        
//        return cost;
//    }

//    private double[] CalculateErrors(double[] targets, double[] actions, int actionQty)
//    {
//        double[] errors = new double[actionQty];
//        for (int i = 0; i < actionQty; i++)
//        {
//            errors[i] = actions[i] - targets[i]; // TODO: Check which should be subtracted
//        }
//        return errors;
//    }

//    private void CalculateGradients(double[][] actions, double[] errors, int i, DQN dqn)
//    {
//        // Calculate signal for output layer
//        for (int neu = 0; neu < neuralNetStructure[neuralNetStructure.Length - 1]; neu++) // Iterate through each neuron in the output layer
//        {
//            // Calculate the signal (node deltas) for the output layer
//            signals[signals.Length - 1][neu] = errors[neu] * ActivationDerivatives(actions[i][neu], neuralNetStructure.Length - 1);

//            biasGradientSingle[neuralNetStructure.Length - 2][neu] = signals[neuralNetStructure.Length - 1][neu];
//        }

//        // Calculate gradients for last weights layer
//        for (int neu = 0; neu < neuralNetStructure[neuralNetStructure.Length - 1]; neu++) // Iterate through each neuron in the output layer
//        {
//            for (int pn = 0; pn < neuralNetStructure[neuralNetStructure.Length - 2]; pn++) // Iterate through each neuron in the previous layer
//            {
//                weightsGradientSingle[neuralNetStructure.Length - 2][neu][pn] = signals[neuralNetStructure.Length - 1][neu] * neuronSums[neuralNetStructure.Length - 2][pn];
//            }
//        }

//        // Calculate signal for hidden layers
//        for (int lay = neuralNetStructure.Length - 2; lay > 0; lay--) // Iterate backwards through the neural network beginning with the last hidden layer
//        {
//            for (int neu = 0; neu < neuralNetStructure[lay]; neu++)
//            {
//                signals[lay][neu] = 0.0d;

//                for (int nn = 0; nn < signals[lay + 1].Length; nn++)
//                {
//                    signals[lay][neu] += signals[lay + 1][nn] * weightsMatrix[lay][nn][neu];
//                }
//                signals[lay][neu] *= ActivationDerivatives(neuronsActivated[lay][neu], lay);

//                biasGradientSingle[lay][neu] = signals[lay][neu];
//            }
//        }

//        // Calculate gradients for all other weights layers
//        for (int lay = weightsGradientSums.Length - 2; lay >= 0; lay--)
//        {
//            for (int neu = 0; neu < weightsGradientSums[lay].Length; neu++)
//            {
//                for (int pn = 0; pn < weightsGradientSums[lay][neu].Length; pn++)
//                {
//                    weightsGradientSingle[lay][neu][pn] = signals[lay + 1][neu] * neuronSums[lay][pn];
//                }
//            }
//        }
//    }

//    private void Optimize(DQN dqn)
//    {
//        if (t == 0)
//        {
//            firstMoment = Init_Weights_Matrix(false);
//            secondMoment = Init_Weights_Matrix(false);
//            firstMomentBias = Init_Neurons_Matrix();
//            secondMomentBias = Init_Neurons_Matrix();
//        }

//        if (!dqn.isConverged)
//        {
//            t++;
//            tx++; // This is reset to 0 each episode which will therefore reset the learning rate
//            dqn.currentLearningRate = GameManager.instance.settings.learningRate * Math.Sqrt(1 - Math.Pow(GameManager.instance.settings.beta2, tx)) / (1 - Math.Pow(GameManager.instance.settings.beta1, tx));
//            //Debug.Log(dqn.currentLearningRate);
//            for (int lay = weightsMatrix.Length - 1; lay >= 0; lay--)
//            {
//                for (int neu = 0; neu < weightsMatrix[lay].Length; neu++)
//                {
//                    for (int pn = 0; pn < weightsMatrix[lay][neu].Length; pn++)
//                    {
//                        double gradient = weightsGradientSums[lay][neu][pn];
//                        if (Math.Abs(gradient) > GameManager.instance.settings.gradientThreshold)
//                        {
//                            gradient *= GameManager.instance.settings.gradientThreshold / Math.Abs(gradient); // Clip gradient to prevent exploding and vanishing gradients
//                        }

//                        firstMoment[lay][neu][pn] = (GameManager.instance.settings.beta1 * firstMoment[lay][neu][pn]) + (1 - GameManager.instance.settings.beta1) * gradient;
//                        secondMoment[lay][neu][pn] = (GameManager.instance.settings.beta2 * secondMoment[lay][neu][pn]) + (1 - GameManager.instance.settings.beta2) * (gradient * gradient);

//                        double deltaWeight = gradient * GameManager.instance.settings.learningRate; //dqn.currentLearningRate * firstMoment[lay][neu][pn] / (Math.Sqrt(secondMoment[lay][neu][pn]) + GameManager.instance.settings.epsilonHat);
//                        weightsMatrix[lay][neu][pn] -= deltaWeight;
//                    }
//                }
//            }
//            for (int lay = biasGradientSums.Length - 1; lay > 0; lay--)
//            {
//                for (int bias = 0; bias < biasGradientSums[lay].Length; bias++)
//                {
//                    double biasGradient = biasGradientSums[lay][bias];

//                    if (Math.Abs(biasGradient) > GameManager.instance.settings.gradientThreshold)
//                    {
//                       biasGradient *= GameManager.instance.settings.gradientThreshold / Math.Abs(biasGradient); // Clip gradient to prevent exploding and vanishing gradients
//                    }

//                    firstMomentBias[lay][bias] = GameManager.instance.settings.beta1 * firstMomentBias[lay][bias] + (1 - GameManager.instance.settings.beta1) * biasGradient;
//                    secondMomentBias[lay][bias] = GameManager.instance.settings.beta2 * secondMomentBias[lay][bias] + (1 - GameManager.instance.settings.beta2) * (biasGradient * biasGradient);

//                    double biasDelta = biasGradient * GameManager.instance.settings.learningRate; //dqn.currentLearningRate * firstMomentBias[lay][bias] / (Math.Sqrt(secondMomentBias[lay][bias]) + GameManager.instance.settings.epsilonHat);
//                    biases[lay][bias] -= biasDelta;
//                }
//            }
//        }
//    }

//    // Mutation function will apply a mutation to weight values based on chance
//    public void Mutate()
//    {
//        // Iterate through all the layers besides the input layer.
//        for (int layer = 1; layer < weightsMatrix.Length; layer++)
//        {
//            // Iterate through all the neurons.
//            for (int neuron = 0; neuron < weightsMatrix[layer].Length; neuron++)
//            {
//                // Iterate through all the weights.
//                for (int weight = 0; weight < weightsMatrix[layer][neuron].Length; weight++)
//                {
//                    // Create a float weight and set it to the current weight.
//                    double weightValue = weightsMatrix[layer][neuron][weight];

//                    float randomNumber = UnityEngine.Random.Range(0f, 1000f); // Generate a random number

//                    // **Mutations**
//                    if (randomNumber <= 2f) // Mutation #1 (0.2% chance)
//                    {
//                        weightValue *= -1f; // Flip the sign
//                    }
//                    else if (randomNumber <= 4f) // Mutation #2 (0.2% chance)
//                    {
//                        weightValue = UnityEngine.Random.Range(-0.5f, 0.5f); // Find another weight between -0.5 and 0.5
//                    }
//                    else if (randomNumber <= 6f) // Mutation #3 (0.2% chance)
//                    {
//                        float factor = UnityEngine.Random.Range(0f, 1f) + 1f; // Increase weight by 0% - 100%
//                        weightValue *= factor;
//                    }
//                    else if (randomNumber <= 8f) // Mutation #4 (0.2% chance)
//                    {
//                        float factor = UnityEngine.Random.Range(-1f, 0f) - 1f; // Decrease weight by 0% - 100%
//                        weightValue *= factor;
//                    }
//                    weightsMatrix[layer][neuron][weight] = weightValue; // Set the current weight to the mutated weight.
//                }
//            }
//        }
//    }







//    ///// <summary>
//    ///// Feed inputs through the neural network and returns an output
//    ///// </summary>
//    ///// <param name="inputs"></param>
//    ///// <returns></returns>
//    //public double[] FeedForward(double[] inputs)
//    //{
//    //    for (int i = 0; i < inputs.Length; i++) // TODO: This might cause an error
//    //    {
//    //        neuronsActivated[0][i] = inputs[i];// ActivationFunctions(inputs[i], 0); // Set the input layer with data passed in
//    //    }
//    //    for (int lay = 1; lay < neuralNetStructure.Length; lay++) // Iterate through each layer starting with the first hidden layer
//    //    {
//    //        for (int neu = 0; neu < neuronsActivated[lay].Length; neu++) // Iterate through each neuron in the layer
//    //        {
//    //            double neuronValue = 0.0d;

//    //            for (int wt = 0; wt < neuronsActivated[lay - 1].Length; wt++) // Iterate through each neuron in the previous layer
//    //            {
//    //                neuronValue += weightsMatrix[lay - 1][neu][wt] * neuronsActivated[lay - 1][wt];
//    //            }

//    //            neuronsSummed[lay][neu] = neuronValue; //biases[lay][neu]; // Save the neuronSum, needed for back propagation 

//    //            neuronsActivated[lay][neu] = ActivationFunctions(neuronsSummed[lay][neu] + biases[lay][neu], lay);
//    //        }
//    //    }
//    //    if (activationPerLayer[activationPerLayer.Length - 1] == Settings.LayerActivations.Softmax) // Apply softmax to output layer if necessary
//    //    {
//    //        neuronsActivated[neuronsActivated.Length - 1] = GameManager.instance.math.Softmax(neuronsActivated[neuronsActivated.Length - 1]);
//    //    }

//    //    return neuronsActivated[neuronsActivated.Length - 1]; // Return the output layer
//    //}

//    //public void SumGradients()
//    //{
//    //    for (int lay = 0; lay < weightsGradientSums.Length; lay++)
//    //    {
//    //        for (int neu = 0; neu < weightsGradientSums[lay].Length; neu++)
//    //        {
//    //            for (int wt = 0; wt < weightsGradientSums[lay][neu].Length; wt++)
//    //            {
//    //                weightsGradientSums[lay][neu][wt] += weightsGradientSingle[lay][neu][wt];
//    //            }
//    //        }
//    //    }

//    //    for (int lay = 0; lay < biasGradientSums.Length; lay++)
//    //    {
//    //        for (int neu = 0; neu < biasGradientSums[lay].Length; neu++)
//    //        {
//    //            biasGradientSums[lay][neu] += biasGradientSingle[lay][neu];
//    //        }
//    //    }
//    //}
//    ///// <summary>
//    ///// Trains the neural network
//    ///// </summary>
//    ///// <param name="expBuffer"></param>
//    ///// <param name="weights"></param>
//    ///// <param name="neurons"></param>
//    ///// <param name="dqn"></param>
//    ///// <param name="env"></param>
//    ///// <param name="agent"></param>
//    ///// <returns></returns>
//    //public bool Train(DQN dqn, Environment env, Agent agent)
//    //{
//    //    int miniBatchSize = GameManager.instance.settings.miniBatchSize; // Size of the mini batch used to train the target net
//    //    int actionQty = dqn.actionQty; // Number of possible actions the agent can perform (AKA number of outputs)

//    //    // This Tuple array consists of one mini batch (random sample from experience replay buffer)
//    //    Tuple<int, double[], double, bool>[] miniBatch = agent.GetMiniBatch(miniBatchSize);

//    //    double[][] states = new double[miniBatchSize][]; // States (AKA inputs)
//    //    double[][] nextStates = new double[miniBatchSize][]; // Next States (The resulting state after an action)
//    //    double[][] actions = new double[miniBatchSize][]; // Actions performed
//    //    double[] rewards = new double[miniBatchSize]; // Reward for the action
//    //    bool[] dones = new bool[miniBatchSize]; // Boolean to indicate if the current mini batch is done (This is true on the last frame of an episode to prevent it from being used for training)

//    //    // Unpack mini batch
//    //    for (int i = 0; i < miniBatchSize; i++)
//    //    {
//    //        if (miniBatch[i] != null)
//    //        {
//    //            states[i] = env.GetState(miniBatch[i].Item1 - 1); // State ends with the second to last frame in the set
//    //            nextStates[i] = env.GetState(miniBatch[i].Item1); // Next state ends with the last frame
//    //            actions[i] = miniBatch[i].Item2;
//    //            rewards[i] = miniBatch[i].Item3;
//    //            dones[i] = miniBatch[i].Item4;
//    //        }
//    //    }

//    //    for (int i = 0; i < miniBatchSize; i++) // Iterate through each mini batch
//    //    {
//    //        double[] targets = CalculateTargets(nextStates[i], rewards[i], dones[i], dqn); // Calculate the targets for the mini batch

//    //        if (targets != null) // Do not begin training until targets are set
//    //        {
//    //            cost = Cost(actions[i], targets, actionQty); // Calculate the cost
//    //            double[] errors = CalculateErrors(targets, actions[i], actionQty); // Calculate errors for the batch
//    //            CalculateGradients(actions, errors, i); // Calculate gradients for the batch
//    //            SumGradients();
//    //        }
//    //    }

//    //    Optimize(dqn); // Update the weights
//    //    Debug.Log("No Move: " + neuronsActivated[neuronsActivated.Length - 1][0] + "  Forward: " + neuronsActivated[neuronsActivated.Length - 1][1] + "  Back: " + neuronsActivated[neuronsActivated.Length - 1][2] +
//    //    "  Right: " + neuronsActivated[neuronsActivated.Length - 1][3] + "  Left: " + neuronsActivated[neuronsActivated.Length - 1][4]);
//    //    //Debug.Log(weightsMatrix[0][2][3]);
//    //    return false;
//    //}

//    //// Calculate the Targets for each mini batch
//    //private double[] CalculateTargets(double[] nextStates, double reward, bool done, DQN dqn)
//    //{
//    //    //int mbs = GameManager.instance.settings.miniBatchSize; 
//    //    int aq = dqn.actionQty;

//    //    double[] qValues = dqn.targetNet.FeedForward(nextStates);

//    //    double[] targets = new double[aq];

//    //    for (int i = 0; i < aq; i++)
//    //    {
//    //        double d = done == true ? 0d : 1d;
//    //        targets[i] = reward + (d * GameManager.instance.settings.gamma * qValues[i]);
//    //    }

//    //    return targets;
//    //}

//    //private void CalculateGradients(double[][] actions, double[] errors, int i)
//    //{
//    //    // Calculate signal for output layer
//    //    for (int neu = 0; neu < neuralNetStructure[neuralNetStructure.Length - 1]; neu++) // Iterate through each neuron in the output layer
//    //    {
//    //        // Calculate the signal (node deltas) for the output layer
//    //        signals[signals.Length - 1][neu] = errors[neu] * ActivationDerivatives(actions[i][neu], neuralNetStructure.Length - 1);

//    //        biasGradientSingle[neuralNetStructure.Length - 2][neu] = signals[neuralNetStructure.Length - 1][neu];
//    //    }

//    //    // Calculate gradients for last weights layer
//    //    for (int neu = 0; neu < neuralNetStructure[neuralNetStructure.Length - 1]; neu++) // Iterate through each neuron in the output layer
//    //    {
//    //        for (int wt = 0; wt < neuralNetStructure[neuralNetStructure.Length - 2]; wt++) // Iterate through each weight connected to a neuron in the previous layer
//    //        {
//    //            weightsGradientSingle[neuralNetStructure.Length - 2][neu][wt] = signals[neuralNetStructure.Length - 1][neu] * neuronsSummed[neuralNetStructure.Length - 2][wt];
//    //        }
//    //    }

//    //    // Calculate signal for hidden layers
//    //    for (int lay = neuralNetStructure.Length - 2; lay > 0; lay--) // Iterate backwards through the neural network beginning with the last hidden layer
//    //    {
//    //        for (int neu = 0; neu < neuralNetStructure[lay]; neu++)
//    //        {
//    //            signals[lay][neu] = 0.0d;

//    //            for (int nn = 0; nn < signals[lay + 1].Length; nn++)
//    //            {
//    //                signals[lay][neu] += signals[lay + 1][nn] * weightsMatrix[lay][nn][neu];
//    //            }
//    //            signals[lay][neu] *= ActivationDerivatives(neuronsActivated[lay][neu], lay);

//    //            biasGradientSingle[lay][neu] = signals[lay][neu];
//    //        }
//    //    }

//    //    // Calculate gradients for all other weights layers
//    //    for (int lay = weightsGradientSums.Length - 2; lay >= 0; lay--)
//    //    {
//    //        for (int neu = 0; neu < weightsGradientSums[lay].Length; neu++)
//    //        {
//    //            for (int pn = 0; pn < weightsGradientSums[lay][neu].Length; pn++)
//    //            {
//    //                weightsGradientSingle[lay][neu][pn] = signals[lay + 1][neu] * neuronsSummed[lay][pn];
//    //            }
//    //        }
//    //    }
//    //}
//    //private double Cost(double[] actions, double[] targets, int actionQty)
//    //{
//    //    double sum = 0;

//    //    for (int i = 0; i < actionQty; i++)
//    //    {
//    //        double error = actions[i] - targets[i];
//    //        sum += (error * error);
//    //    }

//    //    return sum / actionQty;
//    //}
//    //private double[] CalculateErrors(double[] targets, double[] actions, int actionQty)
//    //{
//    //    double[] errors = new double[actionQty];
//    //    for (int i = 0; i < actionQty; i++)
//    //    {
//    //        errors[i] = actions[i] - targets[i]; // TODO: Check which should be subtracted
//    //    }
//    //    return errors;
//    //}
//    //private void Optimize(DQN dqn)
//    //{
//    //    if (!dqn.isConverged)
//    //    {
//    //        t++;
//    //        for (int lay = weightsMatrix.Length - 1; lay >= 0; lay--)
//    //        {
//    //            for (int neu = 0; neu < weightsMatrix[lay].Length; neu++)
//    //            {
//    //                for (int pn = 0; pn < weightsMatrix[lay][neu].Length; pn++)
//    //                {
//    //                    weightsMatrix[lay][neu][pn] -= weightsGradientSums[lay][neu][pn] * GameManager.instance.settings.learningRate;
//    //                }
//    //            }
//    //        }
//    //        for (int lay = biasGradientSums.Length - 1; lay > 0; lay--)
//    //        {
//    //            for (int bias = 0; bias < biasGradientSums[lay].Length; bias++)
//    //            {
//    //                biases[lay][bias] -= biasGradientSums[lay][bias] * GameManager.instance.settings.learningRate;
//    //            }
//    //        }
//    //    }
//    //}
//    ///// <summary>
//    ///// Initialize a Neurons Matrix
//    ///// </summary>
//    ///// <returns></returns>
//    //private double[][] Init_Neurons_Matrix()
//    //{
//    //    List<double[]> neurons = new List<double[]>(); // Create an empty neurons matrix list

//    //    for (int lay = 0; lay < neuralNetStructure.Length; lay++) // Iterate through each layer
//    //    {
//    //        neurons.Add(new double[neuralNetStructure[lay]]); // Add an array where each neuron is an index in that array
//    //    }

//    //    return neurons.ToArray(); // Convert the neurons list to an array
//    //}
//    ///// <summary>
//    ///// Initialize a Bias Matrix (Init all biases with a random value)
//    ///// </summary>
//    ///// <returns></returns>
//    //private double[][] Init_Bias_Matrix()
//    //{
//    //    List<double[]> biasMatrix = new List<double[]>(); // Create an empty list

//    //    for (int lay = 0; lay < neuralNetStructure.Length; lay++) // Iterate through each layer
//    //    {
//    //        double[] bias = new double[neuralNetStructure[lay]]; // Create a new 1-D array for each layer, size of the array based on netStructure

//    //        for (int b = 0; b < neuralNetStructure[lay]; b++) // Iterate through each bias
//    //        {
//    //            bias[b] = (double)UnityEngine.Random.Range(-0.5f, 0.5f); // Set the bias to a random float between -0.5 and 0.5
//    //        }

//    //        biasMatrix.Add(bias); // Add the bias array to the biases List
//    //    }
//    //    return biasMatrix.ToArray(); // Return list converted to an array
//    //}
//    ///// <summary>
//    ///// Initialize a 3 dimensional weights matrix
//    ///// </summary>
//    ///// <param name="isRandom"></param>
//    ///// <returns></returns>
//    //private double[][][] Init_Weights_Matrix(bool isRandom)
//    //{
//    //    List<double[][]> weightsMatrix = new List<double[][]>(); // Create a temporary 3-D list for the weights

//    //    for (int lay = 1; lay < neuralNetStructure.Length; lay++) // Iterate through each layer
//    //    {
//    //        List<double[]> layers = new List<double[]>(); // Create a new list for each layer

//    //        int neuronsInPreviousLayer = neuralNetStructure[lay - 1]; // Calculate the number of neurons in the previous layer (This is the number of weights that are connected to a specific neuron)

//    //        for (int neu = 0; neu < neuralNetStructure[lay]; neu++) // Iterate through each neuron
//    //        {
//    //            double[] weight = new double[neuronsInPreviousLayer]; // Create an array of weights for each neuron

//    //            for (int wt = 0; wt < neuronsInPreviousLayer; wt++) // Iterate through each weight
//    //            {
//    //                if (isRandom) // If random (used for weights)
//    //                {
//    //                    weight[wt] = (double)UnityEngine.Random.Range(-0.5f, 0.5f); // Set a random weight
//    //                }
//    //                else // If not random (used for firstMoment and secondMoment matrices in Adam Optimizer)
//    //                {
//    //                    weight[wt] = 0d;
//    //                }
//    //            }

//    //            layers.Add(weight); // Add weight to layers
//    //        }

//    //        weightsMatrix.Add(layers.ToArray()); // Add the layers to the weightsMatrix
//    //    }

//    //    return weightsMatrix.ToArray(); // Return the weightsMatrix
//    //}
//    //public double ActivationFunctions(double value, int layer)
//    //{
//    //    switch (activationPerLayer[layer])
//    //    {
//    //        case Settings.LayerActivations.Relu:
//    //            return GameManager.instance.math.Relu(value);
//    //        case Settings.LayerActivations.LeakyRelu:
//    //            return GameManager.instance.math.LeakyRelu(value);
//    //        case Settings.LayerActivations.Sigmoid:
//    //            return GameManager.instance.math.Sigmoid(value);
//    //        case Settings.LayerActivations.Tanh:
//    //            return GameManager.instance.math.Tanh(value);
//    //        case Settings.LayerActivations.Softmax: // Softmax relies on all of the output values to be calculated. Therefore, just return the value and softmax will be applied after all outputs are calculated
//    //            return value;
//    //        default:
//    //            return GameManager.instance.math.Relu(value);
//    //    }
//    //}
//    //public double ActivationDerivatives(double value, int layer)
//    //{
//    //    switch (activationPerLayer[layer])
//    //    {
//    //        case Settings.LayerActivations.Relu:
//    //            return GameManager.instance.math.ReluDerivative(value);
//    //        case Settings.LayerActivations.LeakyRelu:
//    //            return GameManager.instance.math.LeakyReluDerivative(value);
//    //        case Settings.LayerActivations.Sigmoid:
//    //            return GameManager.instance.math.SigmoidDerivative(value);
//    //        case Settings.LayerActivations.Tanh:
//    //            return GameManager.instance.math.TanhDerivative(value);
//    //        case Settings.LayerActivations.Softmax:
//    //            return GameManager.instance.math.SoftmaxDerivative(value);
//    //        default:
//    //            return GameManager.instance.math.ReluDerivative(value);
//    //    }
//    //}
//    //public double[][][] GetWeightsMatrix()
//    //{
//    //    return weightsMatrix;
//    //}
//    //public double[][] GetBiases()
//    //{
//    //    return biases;
//    //}

//    //public double[][] GetNeuronSums()
//    //{
//    //    return neuronsSummed;
//    //}
//    //public void SetWeightsMatrix(double[][][] weightsToCopy)
//    //{
//    //    weightsMatrix = weightsToCopy;
//    //}
//    //public void SetBiases(double[][] biasesToCopy)
//    //{
//    //    biases = biasesToCopy;
//    //}
//    //// Mutation function will apply a mutation to weight values based on chance
//    //public void Mutate()
//    //{
//    //    // Iterate through all the layers besides the input layer.
//    //    for (int layer = 1; layer < weightsMatrix.Length; layer++)
//    //    {
//    //        // Iterate through all the neurons.
//    //        for (int neuron = 0; neuron < weightsMatrix[layer].Length; neuron++)
//    //        {
//    //            // Iterate through all the weights.
//    //            for (int weight = 0; weight < weightsMatrix[layer][neuron].Length; weight++)
//    //            {
//    //                // Create a float weight and set it to the current weight.
//    //                double weightValue = weightsMatrix[layer][neuron][weight];

//    //                float randomNumber = UnityEngine.Random.Range(0f, 1000f); // Generate a random number

//    //                // **Mutations**
//    //                if (randomNumber <= 2f) // Mutation #1 (0.2% chance)
//    //                {
//    //                    weightValue *= -1f; // Flip the sign
//    //                }
//    //                else if (randomNumber <= 4f) // Mutation #2 (0.2% chance)
//    //                {
//    //                    weightValue = UnityEngine.Random.Range(-0.5f, 0.5f); // Find another weight between -0.5 and 0.5
//    //                }
//    //                else if (randomNumber <= 6f) // Mutation #3 (0.2% chance)
//    //                {
//    //                    float factor = UnityEngine.Random.Range(0f, 1f) + 1f; // Increase weight by 0% - 100%
//    //                    weightValue *= factor;
//    //                }
//    //                else if (randomNumber <= 8f) // Mutation #4 (0.2% chance)
//    //                {
//    //                    float factor = UnityEngine.Random.Range(-1f, 0f) - 1f; // Decrease weight by 0% - 100%
//    //                    weightValue *= factor;
//    //                }
//    //                weightsMatrix[layer][neuron][weight] = weightValue; // Set the current weight to the mutated weight.
//    //            }
//    //        }
//    //    }
//    //}
//}
