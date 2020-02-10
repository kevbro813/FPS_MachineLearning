using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NeuralNetwork
{
    public int[] neuralLayers; // Contains the number of neurons in each layer
    public double[][] neuronsActivated; // Contains all the neurons, organized in layers
    public double[][] neuronsSums; // 
    public double[][] nodeSignals; // Matrix to hold all the node signals
    public double[][] biases; //
    public double[][] biasGradients; //

    public double[][][] weightsMatrix; // Contains all weights, organized by layers of neurons
    public double[][][] firstMoment; // First moment matrix used with adam optimizer
    public double[][][] secondMoment; // Second moment matrix used with adam optimize
    public double[][] firstMomentBias;
    public double[][] secondMomentBias;
    public float fitness; // Fitness level represents the grade of the current network
    public int t = 0; // TimeStep counter used in optimize function

    //** FOR BATCH NORMALIZATION OF ALL ACTIVATED NODES
    public double[][] mainNormalized;
    public double[][] mainMeans;
    public double[][] mainVariances;
    public double[][] targetNormalized;
    public double[][] targetMeans;
    public double[][] targetVariances;

    // Neural Network Constructor
    public NeuralNetwork(int[] layers) // The number of layers are passed into this method using another array
    {
        // Create an array of neural layers with the size of the layers array being passed in
        this.neuralLayers = new int[layers.Length]; 

        // Loop through each layer
        for(int lay = 0; lay < layers.Length; lay++) 
        {
            // Set the number of neurons in each layer to the int value being passed in via the layers array
            this.neuralLayers[lay] = layers[lay];
        }

        neuronsActivated = InitNeuronsMatrix(neuralLayers); // Initialize neuron matrix to hold neuron values after they have gone through activation function
        biases = InitBiases(neuralLayers);
        biasGradients = InitBiases(neuralLayers);
        neuronsSums = InitNeuronsMatrix(neuralLayers); // Initialize neuron matrix to hold neuron sums (before they are normalized by activation function)
        weightsMatrix = InitWeightsMatrix(true, neuralLayers, neuronsActivated); // Initialize weight matrix, isRandom set to true for random weights

        mainNormalized = InitBiases(neuralLayers);
        mainMeans = InitBiases(neuralLayers);
        mainVariances = InitBiases(neuralLayers);
        targetNormalized = InitBiases(neuralLayers);
        targetMeans = InitBiases(neuralLayers);
        targetVariances = InitBiases(neuralLayers);
    }

    // Creates a Neuron Matrix [layer][neuron]
    private double[][] InitNeuronsMatrix(int[] layersArray)
    {
        // Create an empty neuron list matrix.
        List<double[]> neurons = new List<double[]>(); 

        for (int lay = 0; lay < layersArray.Length; lay++) // For each neural layer...
        {
            // Add an array where each element represents a neuron. neuralLayers array is used to construct this
            neurons.Add(new double[layersArray[lay]]);
        }
        return neurons.ToArray(); // Convert the list of neurons to an array
    }
    // Creates a bias matrix [layer][biases]
    private double[][] InitBiases(int[] layersArray)
    {
        // Create an empty matrix for the biases, one bias for each neuron. These will be added to the neuron before activation
        List<double[]> biases = new List<double[]>();

        for (int lay = 0; lay < layersArray.Length; lay++) // For each neural layer...
        {
            double[] bias = new double[layersArray[lay]];
            for (int b = 0; b < layersArray[lay]; b++)
            {
                bias[b] = UnityEngine.Random.Range(-0.5f, 0.5f);
            }
            biases.Add(bias);
        }
        return biases.ToArray(); // Convert the list of neurons to an array
    }
    // Creates a Weights Matrix [layer][neuron][weight]
    private double[][][] InitWeightsMatrix(bool isRandom, int[] layersArray, double[][] neurons)
    {
        // Create a new list to contain all the weights. The list is organized in layers, with each 2D float array 
        // representing a layer. All the layers are temporarily contained in this list, then converted to an array
        List<double[][]> weightsMatrix = new List<double[][]>(); 

        // Iterate through each layer... (start with first hidden layer because the input layer does not have weights)
        for (int lay = 1; lay < layersArray.Length; lay++) 
        {
            // Create a weight matrix for each layer
            List<double[]> layersMatrix = new List<double[]>(); 

            // Determine how many neurons there are in the previous layer 
            // This represents the number of weights required for each neuron
            int neuronsInPreviousLayer = layersArray[lay - 1];

            // For each neuron in the current layer...
            for (int neu = 0; neu < neurons[lay].Length; neu++)
            {
                // Create a new float array for each neuron in the previous layer
                double[] weight = new double[neuronsInPreviousLayer]; 

                // For each weight in the current neuron...
                for (int wt = 0; wt < neuronsInPreviousLayer; wt++)
                {
                    if (isRandom) // If isWeights is true then set random weight value
                    {
                        // Set the weight to a random value between -0.5 and 0.5
                        weight[wt] = UnityEngine.Random.Range(-0.5f, 0.5f);
                    }
                    else // If isWeights is false, then it is a gradient initialized as 0
                    {
                        // Set the gradient to 0
                        weight[wt] = 0;
                    }
                }
                // Add the neuron with new weights to the weight matrix for the current layer
                layersMatrix.Add(weight);
            }

            // Add the current layer to the list of all weights
            weightsMatrix.Add(layersMatrix.ToArray());
        }

        // Convert the weights list to an array once all neurons with weights have been iterated through
        return weightsMatrix.ToArray();
    }

    // Calculates neuron values given a set of inputs
    public double[] FeedForward(float[] inputs, int[] layers, string hiddenActivation, string outputActivation, double[][] nActs, double[][] nSums, double[][][] weights, bool isMainNet, DQN dqn)
    {

        // Iterate through all the inputs and add them to the first layer (input layer) in the neuronsMatrix
        for (int ipt = 0; ipt < inputs.Length; ipt++)
        {
            nActs[0][ipt] = inputs[ipt];
            nSums[0][ipt] = inputs[ipt];
        }
        // Iterate through each layer (excluding input layer)
        for (int lay = 1; lay < layers.Length; lay++)
        {
            // Iterate through each neuron in the current layer
            for (int neu = 0; neu < layers[lay]; neu++)
            {
                double neuronValue = 0.0; // Create a new value for the neuron. Changing this value can set a static bias

                // Iterate through each neuron in the previous layer that the current neuron is connected to
                for (int wt = 0; wt < layers[lay - 1]; wt++)
                {
                    // Set neuronValue by adding it to the neuron's weight that it is connected to and multiply by the activation value in the previous neuron
                    neuronValue += weights[lay - 1][neu][wt] * nActs[lay - 1][wt];     

                }
                nSums[lay][neu] = neuronValue + biases[lay - 1][neu]; // Save the neuronValue sum

                if (lay != layers.Length - 1)
                {
                    if (hiddenActivation == "sigmoid")
                    {
                        double n = Math.Exp(nSums[lay][neu]);
                        nActs[lay][neu] = n / (1.0d + n);
                    }
                    else if (hiddenActivation == "relu")
                    {
                        nActs[lay][neu] = (double)Math.Max(0.0, nSums[lay][neu]); //Max function can be used for ReLU
                    }
                }
                if (lay == layers.Length - 1)
                {
                    if (outputActivation == "softmax")
                    {
                        nActs[lay] = SoftmaxAction(nSums[lay]);
                    }
                    else if (outputActivation == "selected")
                    {
                        nActs[lay] = SelectedActions(nSums[lay]);
                    }
                }
            }
            neuronsSums = nSums;
        }      
        return nActs[nActs.Length - 1]; // Return the last layer
    }

    // Train the Agent using the target neural network
    public bool Train(Tuple<int, double[], float, bool>[] expBuffer, double[][][] parameters, double[][] neurons, DQN dqn, Environment env, Agent agent) // Frame buffer, action, reward, isDone are passed in
    {
        // Get a random batch from the experience buffer
        Tuple<int, double[], float, bool>[] miniBatch = agent.GetMiniBatch(expBuffer, dqn.miniBatchSize);

        // Create arrays and matrices to hold tuple contents
        float[][] states = new float[dqn.miniBatchSize][];
        float[][] nextStates = new float[dqn.miniBatchSize][];
        double[][] actions = new double[dqn.miniBatchSize][];
        float[] rewards = new float[dqn.miniBatchSize];
        bool[] dones = new bool[dqn.miniBatchSize];
    
        double[][] signals = InitNeuronsMatrix(neuralLayers); // Create a matrix for the node signals

        // Unpack Tuples
        for (int i = 0; i < dqn.miniBatchSize; i++)
        {
            if (miniBatch[i] != null)
            {
                states[i] = env.GetState(env.frameBuffer, miniBatch[i].Item1 - 1); // Get state using the frameBuffer index stored as item one in the tuple, minus one for current state
                nextStates[i] = env.GetState(env.frameBuffer, miniBatch[i].Item1); // Get the next state using the frameBuffer index
                actions[i] = miniBatch[i].Item2; // Actions are stored as item 2 in the tuple
                rewards[i] = miniBatch[i].Item3; // Rewards are item 3
                dones[i] = miniBatch[i].Item4; // Done flags indicate if the state is a terminal state (boolean)
            }
        }
        double[][] targets = CalculateTargets(nextStates, rewards, dones, dqn); // Calculate the targets for the entire mini-batch
        double[][] costs = Cost(actions, targets, dqn); // Calculate loss using Huber Loss
        double avgCost = AverageCost(dqn, costs); // Average cost
        
        for (int i = 0; i < dqn.miniBatchSize; i++) // For each tuple in the mini-batch
        {
            if (targets[i] != null) // Null check targets matrix
            {
                double[] errors = CalculateErrors(targets, actions, i, dqn); // Calculate the errors for a single mini-batch set
                double[][][] grads = CalculateGradients(signals, neurons, parameters, actions, errors, i, dqn); // Calculate gradients for a single tuple
                // Update model                                                                          
                weightsMatrix = Optimize(weightsMatrix, grads, dqn); // Minimize loss
            }
        }
        Debug.Log(weightsMatrix[0][2][3]);
        return false;
    }
    // Calculate Loss (AKA Cost) using Huber Loss function
    private double[][] Cost(double[][] actions, double[][] targets, DQN dqn)
    {
        float delta = 1; // Delta is the threshold that will switch the cost function from linear to quadratic
        double[][] selected_Actions = new double[dqn.miniBatchSize][]; // Argmax of actions
        double[][] costs = new double[dqn.miniBatchSize][]; // Losses for each target/action in the batch

        // Get the max action for each item in the batch
        for (int i = 0; i < dqn.miniBatchSize; i++)
        {
            selected_Actions[i] = SelectedActions(actions[i]); // Pass nextQValues to AxisMaxAction to only return the strongest action values
        }
        // Huber Loss Algorithm - uses a quadratic function when close to optimal and linear when far from optimal (above or below delta). This speeds up training with lower over correction risk
        for (int i = 0; i < dqn.miniBatchSize; i++) // For each item in the batch
        {
            costs[i] = new double[dqn.actionQty];

            for (int j = 0; j < dqn.actionQty; j++) // For every possible action...
            {
                double err = selected_Actions[i][j] - targets[i][j];
                if (Math.Abs(err) <= delta) // If the error is small then...
                {
                    // Use a Quadratic function
                    costs[i][j] = 0.5f * err * err;
                }
                else
                {
                    // Use a Linear function
                    costs[i][j] = delta * Math.Abs(err - 0.5f * (delta * delta));
                }
            }
        }
        return costs;
    }
    // Optimize function adjusts the parameters (neural network weights) using the adam optimizer algorithm
    public double[][][] Optimize(double[][][] parameters, double[][][] gradients, DQN dqn) // This function will return the updated weights
    {
        if (t == 0)
        {
            // Initialize two weight matrices to 0
            firstMoment = InitWeightsMatrix(false, neuralLayers, neuronsActivated); 
            secondMoment = InitWeightsMatrix(false, neuralLayers, neuronsActivated);
            firstMomentBias = InitNeuronsMatrix(neuralLayers);
            secondMomentBias = InitNeuronsMatrix(neuralLayers);
        }

        if (!dqn.isConverged)
        {
            t++; // Increment timestep

            // Learning rate decay settings
            dqn.currentLearningRate = dqn.learningRate * Math.Sqrt(1 - Math.Pow(dqn.beta2, t)) / (1 - Math.Pow(dqn.beta1, t));

            for (int lay = parameters.Length - 1; lay >= 0; lay--) // 
            {
                for (int neu = 0; neu < parameters[lay].Length; neu++)
                {
                    for (int pn = 0; pn < parameters[lay][neu].Length; pn++)
                    {
                        double gradient = gradients[lay][neu][pn]; // TODO: Check if this is correct
                        if (Math.Abs(gradient) > dqn.gradientThreshold)
                        {
                            gradient = gradient * dqn.gradientThreshold / Math.Abs(gradient); // Clip gradient to prevent exploding and vanashing gradients
                        }
                        
                        firstMoment[lay][neu][pn] = (dqn.beta1 * firstMoment[lay][neu][pn]) + (1 - dqn.beta1) * gradient; // First moment calculation
                        secondMoment[lay][neu][pn] = (dqn.beta2 * secondMoment[lay][neu][pn]) + (1 - dqn.beta2) * (gradient * gradient); // Second moment calculation

                        // Calculate deltaWeight that is used to update the weight by the specified amount
                        double deltaWeight = dqn.currentLearningRate * firstMoment[lay][neu][pn] / (Math.Sqrt(secondMoment[lay][neu][pn]) + dqn.epsilonHat);
                        parameters[lay][neu][pn] += deltaWeight; // Update weight
                    }
                }
            }
            for (int lay = biases.Length - 1; lay > 0; lay--)
            {
                for (int bias = 0; bias < biases[lay].Length; bias++)
                {
                    double gradient = biasGradients[lay][bias];
                    if (Math.Abs(gradient) > dqn.gradientThreshold)
                    {
                        gradient = gradient * dqn.gradientThreshold / Math.Abs(gradient); // Clip gradient to prevent exploding and vanashing gradients
                    }

                    firstMomentBias[lay][bias] = dqn.beta1 * firstMomentBias[lay][bias] + (1 - dqn.beta1) * gradient; // First moment calculation
                    secondMomentBias[lay][bias] = dqn.beta2 * secondMomentBias[lay][bias] + (1 - dqn.beta2) * (gradient * gradient); // Second moment calculation

                    double biasDelta = dqn.currentLearningRate * firstMomentBias[lay][bias] / (Math.Sqrt(secondMomentBias[lay][bias]) + dqn.epsilonHat);
                    biases[lay][bias] += biasDelta;
                }
            }
            // TODO: Determine if neural networks have converged, if true then end loop
        }
        
        return parameters;
    }
    // Calculate Gradients for weights and biases
    private double[][][] CalculateGradients(double[][] signals, double[][] neurons, double[][][] parameters, double[][] actions, double[] errors, int i, DQN dqn)
    {
        double[][][] grads = InitWeightsMatrix(false, neuralLayers, neuronsActivated); 
        double[][] biasGrads = InitNeuronsMatrix(neuralLayers);

        // Signals match up with neurons and grads match with parameters

        // Calculate signals of last neuron layer (Error * Derivative)
        for (int neu = 0; neu < neuralLayers[neuralLayers.Length - 1]; neu++) // Loop through each neuron in the last signal layer
        {
            signals[signals.Length - 1][neu] = errors[neu] * OutputDerivative(actions[i][neu], dqn.outputActivation); // Set the signal value
        }

        // Calculate gradients of last weights layer (last layer signal * previous layer neuron output)

        // Loop through each neuron in the last layer (represents the outbound connection of a weight in the last layer, and is how they are grouped)
        for (int neu = 0; neu < neuralLayers[neuralLayers.Length - 1]; neu++)
        {
            biasGrads[neuralLayers.Length - 2][neu] = signals[neuralLayers.Length - 1][neu];
            // Loop through all the weights (each weight connects to a neuron in the previous layer, this is how the weights are indexed according to the previous neuron it is attached to)
            for (int pn = 0; pn < neuralLayers[neuralLayers.Length - 2]; pn++)
            {
                grads[neuralLayers.Length - 2][neu][pn] = signals[neuralLayers.Length - 1][neu] * neurons[neuralLayers.Length - 2][pn] ; // Gradient = (neuron output for current layer) * (next layer's signal)
            }
        }
        // Continue with all other layers

        for (int lay = signals.Length - 2; lay > 0; lay--) // Iterate through all the signal layers beginning with second to last, stop on index 1 because index 0 signals are not required (this is the input layer)
        {
            // Calculate signals
            for (int neu = 0; neu < neuralLayers[lay]; neu++) // Iterate through each node in the signal layer
            {
                signals[lay][neu] = 0.0;
                // To calculate the sum of all 
                for (int pn = 0; pn < signals[lay + 1].Length; pn++) // Iterate through all the previous neurons
                {
                    signals[lay][neu] = signals[lay + 1][pn] * parameters[lay][pn][neu];
                }
                signals[lay][neu] *= HiddenDerivative(neurons[lay][neu], dqn.hiddenActivation); // Multiply the sum by the derivative to get the node signal
            }

            // Calculate Gradients
            for (int neu = 0; neu < neuralLayers[lay]; neu++) // Iterate through layer outputs
            {
                biasGrads[lay - 1][neu] = signals[lay][neu];
                for (int pn = 0; pn < neuralLayers[lay - 1]; pn++) // Iterate through layer inputs
                {
                    grads[lay - 1][neu][pn] = signals[lay][neu] * neurons[lay - 1][pn] ; // Gradient is the product of the neuron's output and the signal from the next layer (the two ends a weight is connected to)    
                }
            }
        }

        biasGradients = biasGrads;
        return grads;
    }
    // Normalize values
    public double Normalize(double data, int count, bool isMainNet, int lay, int node)
    {
        double totalSqrdDiff;
        double stdDev;
        if (isMainNet)
        {
            mainMeans[lay][node] = UpdateMean(count, mainMeans[lay][node], data);
            // Calculate the squared difference for the new datapoint
            double sqrdDiff = SquaredDifference(data, mainMeans[lay][node]);
            if (count == 1)
            {
                totalSqrdDiff = sqrdDiff;
            }
            else
            {
                // Calculate total squared difference from variance
                totalSqrdDiff = mainVariances[lay][node] * count;

                // Update the total squared difference
                totalSqrdDiff += sqrdDiff;
            }
            // Calculate Variance and Standard Deviation
            mainVariances[lay][node] = Variance(totalSqrdDiff, count);

            stdDev = StdDeviation(mainVariances[lay][node]);

            return ZScore(data, mainMeans[lay][node], stdDev);
        }
        else
        {
            targetMeans[lay][node] = UpdateMean(count, targetMeans[lay][node], data);
            // Calculate the squared difference for the new datapoint
            double sqrdDiff = SquaredDifference(data, targetMeans[lay][node]);
            if (count == 1)
            {
                totalSqrdDiff = sqrdDiff;
            }
            else
            {
                // Calculate total squared difference from variance
                totalSqrdDiff = targetVariances[lay][node] * count;

                // Update the total squared difference
                totalSqrdDiff += sqrdDiff;
            }

            // Calculate Variance and Standard Deviation
            targetVariances[lay][node] = Variance(totalSqrdDiff, count);

            stdDev = StdDeviation(targetVariances[lay][node]);

            // Normalize the current state values
            return ZScore(data, targetMeans[lay][node], stdDev);
        }
    }
    // Recalculates the mean by including the newest datapoint
    public double UpdateMean(int sampleSize, double mean, double dp)
    {
        if (sampleSize == 1) // If sample size = 1 then the mean is equal to the datapoint
        {
            mean = dp;
        }
        else
        {
            double sampleTotal = mean * (sampleSize - 1); // Calculate the total from the old mean
            sampleTotal += dp; // Add the new datapoint to the sample total
            mean = sampleTotal / sampleSize; // Recalculate the mean
        }
        return mean;
    }
    // Find the squared difference, used to calculate variance
    public double SquaredDifference(double dp, double mean)
    {
        double diff = dp - mean;
        double sqrdDiff = diff * diff;
        return sqrdDiff;
    }
    // Calculate the variance
    public double Variance(double totalSqrdDiff, double count)
    {
        double variance = totalSqrdDiff / count;
        return variance;
    }
    // Calculate the standard deviation
    public double StdDeviation(double variance)
    {
        double stdDev = Math.Sqrt(variance);
        return stdDev;
    }
    // Scalar function (calculates zScore)
    public double ZScore(double dp, double mean, double stdDev)
    {
        // Calculate a state's Z-score = (data point - mean) / standard deviation
        double zScore = (dp - mean) / (stdDev + .00001);

        return zScore;
    }
    
    private double[] CalculateErrors(double[][] targets, double[][] actions, int index, DQN dqn)
    {
        double[] errors = new double[dqn.actionQty];
        for (int j = 0; j < dqn.actionQty; j++) // For every possible action...
        {
            errors[j] = actions[index][j] - targets[index][j]; // Calculate the error
        }
        return errors;
    }
    private double[][] CalculateTargets(float[][] ns, float[] r, bool[] ds, DQN dqn)
    {
        // Calculate targets      
        double[][] qValues = new double[dqn.miniBatchSize][]; // Raw Q Values returned from target network
        double[][] sQValues = new double[dqn.miniBatchSize][]; // Q values after passing to MaxAction()
        double[][] ts = new double[dqn.miniBatchSize][]; // Targets

        // Get next QValues from target neurnal network
        for (int i = 0; i < dqn.miniBatchSize; i++)
        {
            qValues[i] = dqn.targetNet.FeedForward(ns[i], neuralLayers, dqn.hiddenActivation, dqn.outputActivation, neuronsActivated, neuronsSums, weightsMatrix, false, dqn);
        }

        // Get the Max Action (All actions but the strongest is set to 0)
        for (int i = 0; i < dqn.miniBatchSize; i++)
        {
            sQValues[i] = SelectedActions(qValues[i]); // Pass nextQValues to AxisMaxAction to only return the strongest action values
        }

        // Calculate targets  
        for (int i = 0; i < dqn.miniBatchSize; i++)
        {
            ts[i] = new double[dqn.actionQty]; // Create array for a single set of targets

            for (int j = 0; j < dqn.actionQty; j++)
            {
                float d = 0;
                if (ds[i] == true)
                {
                    d = 0f; // Done being true should return 0 (this will return the current reward)
                }
                else
                {
                    d = 1f; // If not done, then calculate the targets
                }
                ts[i][j] = r[i] + (d * dqn.gamma * sQValues[i][j]); // Targets = reward + (d * gamma * mQValue)
            }
        }
        return ts;
    }
    private double AverageCost(DQN dqn, double[][] costs)
    {
        double avgCost = 0; // Average loss for each action
        double totLoss = 0; // Variable to hold total loss, used to average

        // Calculate average the losses for each action
        for (int i = 0; i < dqn.miniBatchSize; i++)
        {
            for (int j = 0; j < dqn.actionQty; j++)
            {
                totLoss += costs[i][j]; // Sum costs of 
            }
            avgCost = totLoss / dqn.miniBatchSize; // Calculate average loss
        }
        return avgCost;
    }
    // Returns the strongest action, the rest will be set to 0. Used for selected_Actions and QValues
    public double[] SelectedActions(double[] action)
    {
        double[] sAction = new double[action.Length];
        int indexMax = 0;

        for (int i = 0; i < action.Length - 1; i++)
        {
            if (action[indexMax] > action[i + 1])
            {
                sAction[indexMax] = action[indexMax];
                sAction[i + 1] = 0;
            }
            else
            {
                sAction[indexMax] = 0;
                sAction[i + 1] = action[i + 1];
                indexMax = i + 1;
            }
        }
        return sAction;
    }
    // Softmax action outputs
    public double[] SoftmaxAction(double[] action)
    {
        double[] softAction = new double[action.Length];
        double sum = 0.0;

        for (int i = 0; i < action.Length; i++)
        {
            sum += Math.Exp(action[i] + biases[biases.Length - 1][i]);
        }

        for (int i = 0; i < action.Length; i++)
        {
            softAction[i] = Math.Exp(action[i] + biases[biases.Length - 1][i]) / sum;
        }

        return softAction;
    }
    // Calculate the derivative for reLu
    private double ReLuDerivative(double value)
    {
        double deriv;
        if (value > 0) // If value is greater than 0
        {
            deriv = 1.0; // return 1
        }
        else
        {
            deriv = 0.0; // return 0
        }
        return deriv;
    }
    // TODO: Softmax Derivative
    private double SoftmaxDerivative(double value)
    {
        return (1 - value) * value;
    }
    //TODO: Sigmoid Derivative
    private double SigmoidDerivative(double value)
    {
        double x = 1 / (1 + Math.Exp(-value));
        return x * (1 - x);
    }
    private double HiddenDerivative(double value, string derivative)
    {
        double d = 0;
        if (derivative == "relu")
        {
            d = ReLuDerivative(value);
        }
        else if (derivative == "sigmoid")
        {
            d = SigmoidDerivative(value);
        }
        return d;
    }
    private double OutputDerivative(double value, string derivative)
    {
        double d = 0;
        if (derivative == "softmax")
        {
            d = SoftmaxDerivative(value);

        }
        else if (derivative == "axismax")
        {
            // TODO: Need axismax derivative
        }
        return d;
    }
    // Mutation function will apply a mutation to weight values based on chance.
    public void Mutate()
    {
        // Iterate through all the layers besides the input layer.
        for (int layer = 1; layer < weightsMatrix.Length; layer++)
        {
            // Iterate through all the neurons.
            for (int neuron = 0; neuron < weightsMatrix[layer].Length; neuron++)
            {
                // Iterate through all the weights.
                for (int weight = 0; weight < weightsMatrix[layer][neuron].Length; weight++)
                {
                    // Create a float weight and set it to the current weight.
                    double weightValue = weightsMatrix[layer][neuron][weight];

                    float randomNumber = UnityEngine.Random.Range(0f, 1000f); // Generate a random number

                    // **Mutations**
                    if (randomNumber <= 2f) // Mutation #1 (0.2% chance)
                    {
                        weightValue *= -1f; // Flip the sign
                    }
                    else if (randomNumber <= 4f) // Mutation #2 (0.2% chance)
                    {
                        weightValue = UnityEngine.Random.Range(-0.5f, 0.5f); // Find another weight between -0.5 and 0.5
                    }
                    else if (randomNumber <= 6f) // Mutation #3 (0.2% chance)
                    {
                        float factor = UnityEngine.Random.Range(0f, 1f) + 1f; // Increase weight by 0% - 100%
                        weightValue *= factor;
                    }
                    else if (randomNumber <= 8f) // Mutation #4 (0.2% chance)
                    {
                        float factor = UnityEngine.Random.Range(-1f, 0f) - 1f; // Decrease weight by 0% - 100%
                        weightValue *= factor;
                    }
                    weightsMatrix[layer][neuron][weight] = weightValue; // Set the current weight to the mutated weight.
                }
            }
        }
    }
    public void SexualReproduction(NeuralNetwork netA, NeuralNetwork netB, NeuralNetwork netC)
    {
        // Pass in two neural nets
        // Iterate through all the layers besides the input layer.
        for (int layer = 1; layer < netC.weightsMatrix.Length; layer++)
        {
            // Iterate through all the neurons.
            for (int neuron = 0; neuron < netC.weightsMatrix[layer].Length; neuron++)
            {
                // Iterate through all the weights.
                for (int weight = 0; weight < netC.weightsMatrix[layer][neuron].Length; weight++)
                {
                    // Create a float weight and set it to the current weight.
                    double weightValue = netC.weightsMatrix[layer][neuron][weight];

                    float randomNumber = UnityEngine.Random.Range(0f, 1000f); // Generate a random number

                    // Reproduction
                    if (randomNumber < 900) // Parent A passes on weight
                    {
                        Debug.Log("Passed Parent A");
                        // Net A weight
                        weightValue = netA.weightsMatrix[layer][neuron][weight];
                    }
                    else if (randomNumber <= 1000) // Parent B passes on weight
                    {
                        Debug.Log("Passed Parent B");
                        // Net B weight
                        weightValue = netB.weightsMatrix[layer][neuron][weight];
                    }

                    netC.weightsMatrix[layer][neuron][weight] = weightValue; // Set the weight
                }
            }
        }
    }
}
