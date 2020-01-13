using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Agent : MonoBehaviour
{
    public AIPawn aiPawn;
    public DQN dqn;
    public Environment env;
    public Tuple<int, double[], float, bool>[] experienceBuffer; // Tuple that holds the Index of the last frame(used to calculate states), action, reward and done flag
    Tuple<int, double[], float, bool> nTuple;
    public int bufferIndex = 0; // Keeps track of the current index of the buffer "Count"
    public int expBufferSize = 1000; // The maximum size of the buffer (Can be viewed as the agent's memory)
    public int actionQty; // The number of actions the agent can perform
    public int miniBatchSize = 32; // Size of the mini-batch used to train the agent
    public float gamma = 0.95f; // TODO: What should gamma be set to?
    public double learningRate = 0.001d;
    public float beta1 = 0.9f;
    public float beta2 = 0.999f;
    public double epsilonHat = Math.Pow(10, -5); // Hyperparameter that I have seen set between 10^-8 and 10^-5 (AKA 1e-8 and 1e-5), also 1 or 0.1 have been suggested
    public double[][][] firstMoment;
    public double[][][] secondMoment;
    public int t = 0; // TimeStep
    bool isConverged = false;
    public int bufferCount = 0;
    
    private void Start()
    {
        env = GetComponent<Environment>();
        aiPawn = GetComponent<AIPawn>();
        dqn = GetComponent<DQN>();
        int seed = DateToInt(DateTime.Now);
        UnityEngine.Random.InitState(seed);
        experienceBuffer = new Tuple<int, double[], float, bool>[expBufferSize];
        
    }
    // Converts date to an int that can be used as the seed for RNG
    public int DateToInt(DateTime dateTime)
    {
        return dateTime.Year + dateTime.Month + dateTime.Day + dateTime.Hour + dateTime.Minute + dateTime.Second + dateTime.Millisecond;
    }
    // Initialize a new agent with an empty experienceBuffer
    public void InitAgent()
    {
        // TODO: Load the model the agent will use   
    }

    // Get an action based on state, or small chance (epsilon) for a random action
    public double[] EpsilonGreedy(float[] state, float eps) // States are passed to neural network and returns action
    {
        // The probability for a random action is based on epsilon. Epsilon will gradually reduce which means the agent's behavior will become less random over time. *Exploration vs. Exploitation
        if (UnityEngine.Random.value < eps) // Python: Random value needs to match np.random.random() - returns a half-open interval [0.0, 1.0)
        {
            // Random action
            // Python: return random.choice(self.K) - return a random element from the 1-D array
            Debug.Log("Explore");
            return RandomAction();
        }
        else
        {
            // Action via neural net
            // Python: return np.argmax(self.predict([x])[0]) - return the highes
            Debug.Log("Exploit");
            return dqn.mainNet.FeedForward(state);
        }
    }
    // Performs an action based on inputs from a boolean array
    public void PerformAction(bool[] action)
    {
        // Accept action outputs and apply them to respective pawn functions
        aiPawn.MoveForward(action[0]);
        aiPawn.MoveBack(action[1]);
        aiPawn.MoveRight(action[2]);
        aiPawn.MoveLeft(action[3]);
        aiPawn.RotateRight(action[4]);
        aiPawn.RotateLeft(action[5]);
        aiPawn.NoMovement(action[6]);
    }
    // Buffer that stores (s, a, r, s') tuples 
    public void ExperienceReplay(int lastFrameIdx, double[] action, float reward, bool done) // The state and next state are stored as one float array called frameBuffer.
    {
        nTuple = new Tuple<int, double[], float, bool>(lastFrameIdx, action, reward, done);

        experienceBuffer[bufferIndex] = nTuple; // Add the new tuple to the experienceBuffer

        // The bufferCount will be set to the max of bufferCount and bufferIndex + 1. This tracks the total size of the buffer and will remain at the max once the buffer is filled
        bufferCount = Mathf.Max(bufferCount, bufferIndex + 1);

        // bufferIndex is set to the modulus of bufferIndex + 1. This will reset current to 0 when the buffer is full. (This represents the current index to fill with new tuples)
        bufferIndex = (bufferIndex + 1) % expBufferSize;
    }
    // Get a mini-batch of tuples from the experience buffer to train the agent
    public Tuple<int, double[], float, bool>[] GetMiniBatch(Tuple<int, double[], float, bool>[] exp_Buffer)
    {
        Tuple<int, double[], float, bool>[] mb = new Tuple<int, double[], float, bool>[miniBatchSize];

        for (int i = 0; i < miniBatchSize; i++)
        {
            start:
            int rand = UnityEngine.Random.Range(0, bufferCount);

            if (exp_Buffer[rand].Item4 == true)
            {
                goto start;
            }
            mb[i] = exp_Buffer[rand];
        }
        return mb;
    }

    // The strongest action will be true, the rest will be false. Used to move the agent
    public bool[] BinaryAction(double[] action)
    {
        bool[] bAction = new bool[actionQty];
        int indexMax = 0;
        for (int i = 0; i < actionQty - 1; i++)
        {
            if (action != null)
            {
                if (action[indexMax] > action[i + 1])
                {
                    bAction[indexMax] = true;
                    bAction[i + 1] = false;
                }
                else
                {
                    bAction[indexMax] = false;
                    bAction[i + 1] = true;
                    indexMax = i + 1;
                }  
            }
        }
        return bAction;
    }
    // Returns the strongest action, the rest will be set to 0. Used for selected_Actions and QValues
    public double[] MaxAction(double[] action)
    {
        double[] mAction = new double[actionQty];
        int indexMax = 0;
        for (int i = 0; i < actionQty - 1; i++)
        {
            if (action != null)
            {
                if (action[indexMax] > action[i + 1])
                {
                    mAction[indexMax] = action[indexMax];
                    mAction[i + 1] = 0;
                }
                else
                {
                    mAction[indexMax] = 0;
                    mAction[i + 1] = action[i + 1];
                    indexMax = i + 1;
                }
            }
        }
        return mAction;
    }
    // Create a random action
    public double[] RandomAction()
    {
        double[] randAction = new double[actionQty]; // Create a new float array to hold the action values

        // Loop through the array and add a random number to each index
        for (int i = 0; i < randAction.Length; i++)
        {
            randAction[i] = UnityEngine.Random.value; // Just picked an arbitrary range which should not matter much for a random action. *Remember inclusive/exclusive with float variables
        }
        return randAction; // Return random actions array
    }

    ////////// EVERYTHING AFTER THIS IS BROKEN \\\\\\\\\\\

    // Train the Agent using the target neural network
    public void Train(Tuple<int, double[], float, bool>[] expBuffer, double[][][] parameters, double[][][] gradients, double[][] nSignals, double[][] neuronsMatrix) // Frame buffer, action, reward, isDone are passed in
    {
        // Get a random batch from the experience buffer
        Tuple<int, double[], float, bool>[] miniBatch = GetMiniBatch(expBuffer);

        // Create array of states and nextStates
        float[][] states = new float[miniBatchSize][];
        float[][] nextStates = new float[miniBatchSize][];
        double[][] actions = new double[miniBatchSize][];
        float[] rewards = new float[miniBatchSize];
        bool[] dones = new bool[miniBatchSize];

        double[][][] grads = gradients;
        double[][] signals = nSignals;
        double[][] neurons = neuronsMatrix;

        // Unpack Tuples
        for (int i = 0; i < miniBatchSize; i++)
        {
            if (expBuffer[i] != null)
            {
                states[i] = env.GetState(env.frameBuffer, miniBatch[i].Item1 - 1);
                nextStates[i] = env.GetState(env.frameBuffer, miniBatch[i].Item1);
                actions[i] = miniBatch[i].Item2;
                rewards[i] = miniBatch[i].Item3;
                dones[i] = miniBatch[i].Item4;
            }
        }

        double[][] targets = CalculateTargets(nextStates, rewards, dones);  
        double cost = Cost(actions, targets); // Calculate loss

        for (int i = 0; i < miniBatchSize; i++) // For each tuple in the mini-batch
        {
            if (targets[i] != null)
            {
                double[] errors = CalculateErrors(targets, actions, i);
                grads = CalculateGradients(grads, signals, neurons, parameters, actions, errors, i);
            }          
        }
        Debug.Log(grads[0][1][3]);

        // Update model 
        Optimize(dqn.mainNet.weightsMatrix, grads); // Minimize loss

    }
    public double[][][] CalculateGradients(double[][][] grads, double[][] signals, double[][] neurons, double[][][] parameters, double[][] actions, double[] errors, int i)
    {
        int l = parameters.Length - 1;
        int m = neurons.Length - 3;

        // Calculate signals of last neuron layer
        for (int n = 0; n < signals[signals.Length - 1].Length; n++)
        {
            signals[signals.Length - 1][n] = errors[n] * ReLuDerivative(actions[i][n]);
        }

        // Calculate gradients of last weights layer
        for (int neuron = 0; neuron < grads[grads.Length - 1].Length; neuron++)
        {
            for (int pNeuron = 0; pNeuron < neurons[grads.Length - 2].Length; pNeuron++)
            {
                grads[grads.Length - 1][neuron][pNeuron] += neurons[grads.Length - 2][pNeuron] * signals[signals.Length - 1][neuron];
            }
        }

        // Continue with all other layers
        for (int layer = signals.Length - 2; layer > 0; layer--) // Iterate through all the neuron layers beginning with second to last
        {
            double sum = 0;

            for (int currentNode = 0; currentNode < signals[layer].Length; currentNode++) // Iterate through all the neurons in the layer
            {
                for (int pn = 0; pn < parameters[l].Length; pn++)
                {
                    for (int nextNode = 0; nextNode < signals[layer + 1].Length; nextNode++) // Iterate through the neurons in the next layer (Represents outbound weight/node pairs)
                    {
                        sum += signals[layer + 1][nextNode] * parameters[l][layer + 1][pn];
                    }
                }
            }
            for (int node = 0; node < signals[layer + 1].Length; node++)
            {
                // Calculate signals
                signals[layer][node] = ReLuDerivative(neurons[layer][node]) * sum;
            }
            l--;
        }

        // Calculate weight gradients
        for (int layer = grads.Length - 2; layer >= 0; layer--)
        {
            for (int neuron = 0; neuron < grads[layer].Length; neuron++)
            {
                for (int pNeuron = 0; pNeuron < neurons[m].Length; pNeuron++)
                {
                    grads[layer][neuron][pNeuron] += neurons[m][pNeuron] * signals[m][neuron];
                }
            }
            m--;
        }
        return grads;
    }
    public double[] CalculateErrors(double[][] targs, double[][] acts, int index)
    {
        double[] errs = new double[actionQty]; 
        for (int j = 0; j < actionQty; j++) // For every possible action...
        {
            errs[j] = acts[index][j] - targs[index][j];
        }
        return errs;
    }
    // TODO: Calculate Loss (AKA Cost) using Huber Loss function
    public double Cost(double[][] actions, double[][] targets) // PYTHON: cost = tf.reduce_mean(tf.losses.huber_loss(self.G, selected_action_values))
    {
        float delta = 1; // TODO: What is a valid delta value?
        double[][] selected_Actions = new double[miniBatchSize][]; // Argmax of actions
        double[][] costs = new double[miniBatchSize][]; // Losses for each target/action in the batch
        double avgCost = 0; // Average loss for each action

        // Get the max action for each item in the batch
        for (int i = 0; i < miniBatchSize; i++)
        {
            selected_Actions[i] = MaxAction(actions[i]);
        }

        // Huber Loss Algorithm - uses a quadratic function when close to optimal and linear when far from optimal (above or below delta). This speeds up training with lower over correction risk
        for (int i = 0; i < miniBatchSize; i++) // For each item in the batch
        {
            if (targets[i] != null) // Null check for targets and selected_Actions arrays
            {
                costs[i] = new double[actionQty];

                for (int j = 0; j < actionQty; j++) // For every possible action...
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
        }
        double totLoss = 0;
        for (int i = 0; i < miniBatchSize; i++) // Average the losses for each action
        {
            if (costs[i] != null)
            {
                for (int j = 0; j < actionQty; j++)
                {
                    if (costs[j] != null)
                    {
                        totLoss += costs[i][j];
                    }
                }
                avgCost = totLoss / miniBatchSize;
            }
        }
        return avgCost;
    }

    public double[][] CalculateTargets(float[][] ns, float[] r, bool[] ds)
    {
        // Calculate targets      
        double[][] QValues = new double[miniBatchSize][];

        //Python: next_Qs = target_model.predict(next_states)
        for (int i = 0; i < miniBatchSize; i++)
        {
            if (ns[i] != null)
            {
                QValues[i] = dqn.targetNet.FeedForward(ns[i]);
            }
        }

        double[][] mQValues = new double[miniBatchSize][];

        //Python: next_Q = np.amax(next_Qs, axis = 1) - Returns an array containing the max value for each row 
        for (int i = 0; i < miniBatchSize; i++)
        {
            if (QValues[i] != null)
            {
                mQValues[i] = MaxAction(QValues[i]); // Pass nextQValues to DetermineAction to only return the strongest action values
            }
        }

        //Python: targets = rewards + np.invert(dones).astype(np.float32) * gamma * next_Q
        double[][] ts = new double[miniBatchSize][];

        for (int i = 0; i < miniBatchSize; i++)
        {
            if (mQValues[i] != null)
            {
                ts[i] = new double[actionQty];

                for (int j = 0; j < actionQty; j++)
                {
                    float d = 0;
                    if (ds[i] == true)
                    {
                        d = 0f; // Done being true should return 0
                    }
                    else
                    {
                        d = 1f;
                    }
                    ts[i][j] = r[i] + (d * gamma * mQValues[i][j]);
                }
            }
        }
        return ts;
    }

    // TODO: Optimize function
    public double[][][] Optimize(double[][][] parameters, double[][][] gradients) // This function will return the updated weights
    {
        if (t == 0)
        {
            firstMoment = InitWeightMatrixZeros();
            secondMoment = InitWeightMatrixZeros();
        }

        if (!isConverged)
        {
            t++; // Increment timestep
            double decay = 0.001;

            learningRate = learningRate * (1 / (1 + decay * t)); //Math.Sqrt(1 - Math.Pow(beta2, t)) / (1 - Math.Pow(beta1, t)); // Calculate learning rate    

            for (int layer = parameters.Length - 1; layer > 0; layer--) // ******   Iterate backwards through all the layers except the first one which is the input layer
            { 
                for (int neuron = 0; neuron < parameters[layer].Length; neuron++)
                {
                    for (int weight = 0; weight < parameters[layer][neuron].Length; weight++)
                    {
                        double gradient = gradients[layer][neuron][weight];
                        firstMoment[layer][neuron][weight] = beta1 * firstMoment[layer][neuron][weight] + (1 - beta1) * gradient;
                        secondMoment[layer][neuron][weight] = beta2 * secondMoment[layer][neuron][weight] + (1 - beta2) * Math.Pow(gradient, 2);

                        // Calculate deltaWeight that is used to update the weight by the specified amount
                        double deltaWeight = learningRate * firstMoment[layer][neuron][weight] / (Math.Sqrt(secondMoment[layer][neuron][weight]) + epsilonHat);
                        parameters[layer][neuron][weight] = parameters[layer][neuron][weight] + deltaWeight; // Update weight
                    }   
                }
            }
            // TODO: Determine if neural networks have converged, if true then end loop
        }
        return parameters;
    }
    public double ReLuDerivative(double activation)
    {
        double deriv;
        if (activation > 0)
        {
            deriv = 1;
        }
        else
        {
            deriv = 0;
        }
        return deriv;
    }

    public double[][][] InitWeightMatrixZeros()
    {
        List<double[][]> layers = new List<double[][]>();

        for (int layer = 1; layer < dqn.layers.Length; layer++)
        {
            List<double[]> neurons = new List<double[]>();

            int neuronsInPreviousLayer = dqn.layers[layer - 1];

            for (int neuron = 0; neuron < dqn.mainNet.neuronsMatrix[layer].Length; neuron++)
            {
                double[] weights = new double[neuronsInPreviousLayer];

                for (int weight = 0; weight < neuronsInPreviousLayer; weight++)
                {
                    weights[weight] = 0;
                }

                neurons.Add(weights);
            }

            layers.Add(neurons.ToArray());
        }
        return layers.ToArray();
    }
}
