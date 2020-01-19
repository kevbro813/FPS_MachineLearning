using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NeuralNetwork
{
    public int[] neuralLayers; // Contains the number of neurons in each layer
    public double[][] neuronsMatrix; // Contains all the neurons, organized in layers
    public double[][] nodeSignals; // Matrix to hold all the node signals
    public double[][][] weightsMatrix; // Contains all weights, organized by layers of neurons
    public double[][][] firstMoment; // First moment matrix used with adam optimizer
    public double[][][] secondMoment; // Second moment matrix used with adam optimize
    public float fitness; // Fitness level represents the grade of the current network
    public double bias = 1; // Value of the bias nodes
    public int t = 0; // TimeStep counter used in optimize function

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

        neuronsMatrix = InitNeuronsMatrix(true); // Initialize neuron matrix
        weightsMatrix = InitWeightsMatrix(true); // Initialize weight matrix, isRandom set to true for random weights
    }

    // Creates a Neuron Matrix [layer][neuron]
    private double[][] InitNeuronsMatrix(bool isBiased)
    {
        // Create an empty neuron list matrix.
        List<double[]> neurons = new List<double[]>(); 

        for (int lay = 0; lay < neuralLayers.Length; lay++) // For each neural layer...
        {
            // Add an array where each element represents a neuron. neuralLayers array is used to construct this
            neurons.Add(new double[neuralLayers[lay]]);

            if (isBiased)
            {
                neurons[lay][neurons[lay].Length - 1] = bias; // Set the last node in each layer to the bias (default = 1)
            }
        }
        return neurons.ToArray(); // Convert the list of neurons to an array
    }
    // Creates a Weights Matrix [layer][neuron][weight]
    private double[][][] InitWeightsMatrix(bool isRandom)
    {
        // Create a new list to contain all the weights. The list is organized in layers, with each 2D float array 
        // representing a layer. All the layers are temporarily contained in this list, then converted to an array
        List<double[][]> weightsMatrix = new List<double[][]>(); 

        // Iterate through each layer... (start with first hidden layer because the input layer does not have weights)
        for (int lay = 1; lay < neuralLayers.Length; lay++) 
        {
            // Create a weight matrix for each layer
            List<double[]> layersMatrix = new List<double[]>(); 

            // Determine how many neurons there are in the previous layer 
            // This represents the number of weights required for each neuron
            int neuronsInPreviousLayer = neuralLayers[lay - 1];

            // For each neuron in the current layer...
            for (int neu = 0; neu < neuronsMatrix[lay].Length - 1; neu++) // Do not include the bias node
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
                    else // If isWeights is false, then the gradient is initialized as 0
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
    public double[] FeedForward(float[] inputs) // Input = float
    {
        // Iterate through all the inputs and add them to the first layer (input layer) in the neuronsMatrix
        for (int ipt = 0; ipt < inputs.Length; ipt++)
        {
            neuronsMatrix[0][ipt] = inputs[ipt]; 
        }
        // Iterate through each layer (excluding input layer)
        for (int lay = 1; lay < neuralLayers.Length; lay++)
        {
            // Iterate through each neuron in the current layer
            for (int neu = 0; neu < neuronsMatrix[lay].Length - 1; neu++) // Do not include the bias node
            {
                double neuronValue = 0f; // Create a new value for the neuron. Changing this value can set a static bias

                // Iterate through each neuron in the previous layer that the current neuron is connected to
                for (int wt = 0; wt < neuronsMatrix[lay - 1].Length; wt++)
                {
                    // Set neuronValue by adding it to the neuron's weight that it is connected to and multiply by the value in the previous neuron
                    neuronValue += weightsMatrix[lay - 1][neu][wt] * neuronsMatrix[lay - 1][wt];
                }

                // Applies Tanh function to give a value between -1 and 1. Max function can be used for ReLU
                neuronsMatrix[lay][neu] = (double)Math.Max(0, neuronValue); //Math.Tanh(neuronValue);
            }
        }

        return neuronsMatrix[neuronsMatrix.Length - 1]; // Return the last layer
    }
    // Train the Agent using the target neural network
    public bool Train(Tuple<int, double[], float, bool>[] expBuffer, double[][][] parameters, double[][] neurons, DQN dqn, Environment env, Agent agent) // Frame buffer, action, reward, isDone are passed in
    {
        // Get a random batch from the experience buffer
        Tuple<int, double[], float, bool>[] miniBatch = agent.GetMiniBatch(expBuffer);

        // Create arrays and matrices to hold tuple contents
        float[][] states = new float[dqn.miniBatchSize][];
        float[][] nextStates = new float[dqn.miniBatchSize][];
        double[][] actions = new double[dqn.miniBatchSize][];
        float[] rewards = new float[dqn.miniBatchSize];
        bool[] dones = new bool[dqn.miniBatchSize];

        double[][][] grads = InitWeightsMatrix(false); // Create a matrix for the gradients
        double[][] signals = InitNeuronsMatrix(false); // Create a matrix for the node signals

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
                grads = CalculateGradients(grads, signals, neurons, parameters, actions, errors, i); // Calculate gradients for a single mini-batch set
            }
        }
        // Update model 
        weightsMatrix = Optimize(weightsMatrix, grads, dqn); // Minimize loss

        Debug.Log(avgCost);
        return false;
    }
    // Optimize function adjusts the parameters (neural network weights) using the adam optimizer algorithm
    public double[][][] Optimize(double[][][] parameters, double[][][] gradients, DQN dqn) // This function will return the updated weights
    {
        if (t == 0)
        {
            // Initialize two weight matrices to 0
            firstMoment = InitWeightsMatrix(false); 
            secondMoment = InitWeightsMatrix(false);
        }

        if (!dqn.isConverged)
        {
            t++; // Increment timestep

            // TODO: Learning rate decay settings
            //double decay = 0.001;
            //learningRate = learningRate * (1 / (1 + decay * t));// Math.Sqrt(1 - Math.Pow(beta2, t)) / (1 - Math.Pow(beta1, t)); // Calculate learning rate 

            for (int lay = parameters.Length - 1; lay >= 0; lay--) // ******   Iterate backwards through all the layers except the first one which is the input layer
            {
                for (int neu = 0; neu < parameters[lay].Length; neu++)
                {
                    for (int pn = 0; pn < parameters[lay][neu].Length; pn++)
                    {
                        double gradient = gradients[lay][neu][pn] / dqn.miniBatchSize; // TODO: Check if this is correct

                        firstMoment[lay][neu][pn] = dqn.beta1 * firstMoment[lay][neu][pn] + (1 - dqn.beta1) * gradient; // First moment calculation
                        secondMoment[lay][neu][pn] = dqn.beta2 * secondMoment[lay][neu][pn] + (1 - dqn.beta2) * Math.Pow(gradient, 2); // Second moment calculation

                        // Calculate deltaWeight that is used to update the weight by the specified amount
                        double deltaWeight = dqn.learningRate * firstMoment[lay][neu][pn] / (Math.Sqrt(secondMoment[lay][neu][pn]) + dqn.epsilonHat);
                        parameters[lay][neu][pn] = parameters[lay][neu][pn] - deltaWeight; // Update weight
                    }
                }
            }
            // TODO: Determine if neural networks have converged, if true then end loop
        }
        return parameters;
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
            selected_Actions[i] = dqn.agent.AxisMaxAction(actions[i]);
        }

        // Huber Loss Algorithm - uses a quadratic function when close to optimal and linear when far from optimal (above or below delta). This speeds up training with lower over correction risk
        for (int i = 0; i < dqn.miniBatchSize; i++) // For each item in the batch
        {
            costs[i] = new double[dqn.actionQty];

            for (int j = 0; j < dqn.actionQty; j++) // For every possible action...
            {
                if (Math.Abs(selected_Actions[i][j] - targets[i][j]) <= delta) // If the error is small then...
                {
                    // Use a Quadratic function
                    costs[i][j] = 0.5f * Math.Pow(selected_Actions[i][j] - targets[i][j], 2);
                }
                else
                {
                    // Use a Linear function
                    costs[i][j] = delta * Math.Abs(selected_Actions[i][j] - targets[i][j] - 0.5f * Math.Pow(delta, 2));
                }
            }
        }
        return costs;
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
    // Calculate the derivative for reLu
    private double ReLuDerivative(double value)
    {
        double deriv;
        if (value > 0) // If value is greater than 0
        {
            deriv = 1; // return 1
        }
        else
        {
            deriv = 0; // return 0
        }
        return deriv;
    }
    // TODO: Softmax Derivative
    private double SoftmaxDerivative(double value)
    {
        return value;
    }
    //TODO: Sigmoid Derivative
    private double SigmoidDerivative(double value)
    {
        return value;
    }
    private double[][][] CalculateGradients(double[][][] grads, double[][] signals, double[][] neurons, double[][][] parameters, double[][] actions, double[] errors, int i)
    {
        int l = parameters.Length - 1; // Parameter matrix layer (Used to calculate last gradient layer)
        int m = neurons.Length - 3; // Neuron/Signal matrix layer (Used to calculate all gradient layers besides the last layer)

        // Signals match up with neurons and grads match with parameters

        // Calculate signals of last neuron layer (Error * Derivative)
        for (int neu = 0; neu < signals[signals.Length - 1].Length; neu++) // Loop through each neuron in the last signal layer
        {
            signals[signals.Length - 1][neu] = -errors[neu] * ReLuDerivative(actions[i][neu]); // Set the signal value
        }

        // Calculate gradients of last weights layer (last layer signal * previous layer neuron output)

        // Loop through each neuron in the last layer (represents the outbound connection of a weight in the last layer, and is how they are grouped)
        for (int neu = 0; neu < grads[grads.Length - 1].Length; neu++)
        {
            // Loop through all the weights (each weight connects to a neuron in the previous layer, this is how the weights are indexed according to the previous neuron it is attached to)
            for (int pn = 0; pn < grads[grads.Length - 1][neu].Length; pn++)
            {
                grads[grads.Length - 1][neu][pn] += neurons[neurons.Length - 2][pn] * signals[signals.Length - 1][neu]; // Gradient = (neuron output for current layer) * (next layer's signal)
            }
        }

        // Continue with all other layers

        // Calculate signals
        for (int lay = signals.Length - 2; lay > 0; lay--) // Iterate through all the signal layers beginning with second to last, stop on index 1 because index 0 signals are not required (this is the input layer)
        {
            double sum = 0;
            for (int neu = 0; neu < signals[lay + 1].Length; neu++) // Iterate through each node in the signal layer
            {
                // To calculate the sum of all 
                for (int pn = 0; pn < parameters[l].Length; pn++) // Iterate through all the previous neurons
                {
                    for (int nn = 0; nn < signals[lay + 1].Length - 1; nn++) // Iterate through the neurons in the next layer (Represents outbound weight/node pairs), skip the bias node 
                    {
                        // Sum the products of the next layer signal "layer + 1" with the corresponding weight. The sum is used to calculate the signals in the current "layer"
                        sum += parameters[l][nn][pn] * signals[lay + 1][nn];
                    }
                }
                signals[lay][neu] = ReLuDerivative(neurons[lay][neu]) * sum; // Multiply the sum by the derivative to get the node signal
            }
            l--; // decrement l for next layer iteration
        }

        // Calculate weight gradients
        for (int lay = grads.Length - 2; lay >= 0; lay--) // Begin with the second to last gradient layer. Important to include gradients for layer 0 with the >=.
        {
            for (int neu = 0; neu < grads[lay].Length; neu++) // Iterate through all the neurons in the gradient layer
            {
                for (int pn = 0; pn < grads[lay][neu].Length; pn++) // Iterate through all the previous neurons which correspond to a weight
                {
                    grads[lay][neu][pn] += neurons[m][pn] * signals[m + 1][neu]; // Gradient is the product of the neuron's output and the signal from the next layer (the two ends a weight is connected to)
                }
            }
            m--; // Decrement neuron/signal layer
        }

        return grads;
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
        double[][] mQValues = new double[dqn.miniBatchSize][]; // Q values after passing to MaxAction()
        double[][] ts = new double[dqn.miniBatchSize][]; // Targets

        // Get next QValues from target neurnal network
        for (int i = 0; i < dqn.miniBatchSize; i++)
        {
            qValues[i] = dqn.targetNet.FeedForward(ns[i]);
        }

        // Get the Max Action (All actions but the strongest is set to 0)
        for (int i = 0; i < dqn.miniBatchSize; i++)
        {
            mQValues[i] = dqn.agent.AxisMaxAction(qValues[i]); // Pass nextQValues to DetermineAction to only return the strongest action values
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
                ts[i][j] = r[i] + (d * dqn.gamma * mQValues[i][j]); // Targets = reward + (d * gamma * mQValue)
            }
        }
        return ts;
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
