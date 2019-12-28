using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Agent : MonoBehaviour
{
    public AIPawn aiPawn;
    public DQN dqn;
    public Environment env;
    public Tuple<int, float[], float, bool>[] experienceBuffer; // Tuple that holds the Index of the last frame(used to calculate states), action, reward and done flag
    public int bufferIndex = 0; // Keeps track of the current index of the buffer "Count"
    public const int EXP_BUFFER_SIZE = 1000; // The maximum size of the buffer (Can be viewed as the agent's memory)
    public const int FRAMES_PER_STATE = 5;
    public int actionQty; // The number of actions the agent can perform
    public const int MINI_BATCH_SIZE = 32; // Size of the mini-batch used to train the agent
    public float gamma = 0.95f; // TODO: What should gamma be set to?

    public int bufferCount = 0;
    private void Start()
    {
        env = GetComponent<Environment>();
        aiPawn = GetComponent<AIPawn>();
        dqn = GetComponent<DQN>();
        int seed = DateToInt(DateTime.Now);
        UnityEngine.Random.InitState(seed);
        experienceBuffer = new Tuple<int, float[], float, bool>[EXP_BUFFER_SIZE];     
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
        actionQty = dqn.mLayers[dqn.mLayers.Length - 1];
    }

    /* Function similar to np.argmax in python. Except, rather than returning the argmax for all actions,
    this function will return the argmax between movement pairs (forward/reverse, right/left, rotateRight/rotateLeft)
    This way the agent can have simultaneous movements, but it will not allow the agent to move forward and reverse at the same time
    It also makes the movements binary rather than a variable that affects speed. Each movement pair also has a "stopped" action, and the greatest input
    between the three will be the active action */
    public bool[] BinaryAction(float[] action)
    {
        bool[] bAction = new bool[action.Length];

        //action[0] = forward, action[1] = Reverse, action[2] = no vertical, action[3] = right, action[4] = left, action[5] = no lateral, action[6] = rotate right, action[7] = rotate left, action[8] no rotation

        if (action[2] > action[0] && action[2] > action[1]) // If No vertical is greater than forward or back...
        {
            // STOP VERTICAL
            bAction[0] = false;
            bAction[1] = false;
            bAction[2] = true;
        }
        else if (action[0] > action[1]) // If forward is greater than back...
        {
            //MOVE FORWARD
            bAction[0] = true;
            bAction[1] = false;
            bAction[2] = false;
        }
        else // If back is the greatest
        {
            // MOVE BACKWARD
            bAction[0] = false;
            bAction[1] = true;
            bAction[2] = false;
        }
        if (action[5] > action[3] && action[5] > action[4]) // If No Lateral is the greatest...
        {
            // NO LATERAL MOVEMENT
            bAction[3] = false;
            bAction[4] = false;
            bAction[5] = true;
        }
        else if (action[3] > action[4]) // If right is the greatest input...
        {
            // MOVE RIGHT
            bAction[3] = true;
            bAction[4] = false;
            bAction[5] = false;
        }
        else // If left is the greatest...
        {
            // MOVE LEFT
            bAction[3] = false;
            bAction[4] = true;
            bAction[5] = false;
        }
        if (action[8] > action[6] && action[8] > action[7]) // If No rotation is the greatest input...
        {
            // NO ROTATION
            bAction[6] = false;
            bAction[7] = false;
            bAction[8] = true;
        }
        else if (action[4] < action[5]) // If rotate right...
        {
            // ROTATE RIGHT
            bAction[6] = true;
            bAction[7] = false;
            bAction[8] = false;
        }
        else // If rotate left...
        {
            // ROTATE LEFT
            bAction[6] = false;
            bAction[7] = true;
            bAction[8] = false;
        }
        return bAction;
    }
    public float[] TrimAction(float[] action)
    {
        float[] tAction = new float[action.Length];

        //action[0] = forward, action[1] = Reverse, action[2] = no vertical, action[3] = right, action[4] = left, action[5] = no lateral, action[6] = rotate right, action[7] = rotate left, action[8] no rotation

        if (action[2] > action[0] && action[2] > action[1]) // If No vertical is greater than forward or back...
        {
            // STOP VERTICAL
            tAction[0] = 0;
            tAction[1] = 0;
            tAction[2] = action[2];
        }
        else if (action[0] > action[1]) // If forward is greater than back...
        {
            //MOVE FORWARD
            tAction[0] = action[0];
            tAction[1] = 0;
            tAction[2] = 0;
        }
        else // If back is the greatest
        {
            // MOVE BACKWARD
            tAction[0] = 0;
            tAction[1] = action[1];
            tAction[2] = 0;
        }
        if (action[5] > action[3] && action[5] > action[4]) // If No Lateral is the greatest...
        {
            // NO LATERAL MOVEMENT
            tAction[3] = 0;
            tAction[4] = 0;
            tAction[5] = action[5];
        }
        else if (action[3] > action[4]) // If right is the greatest input...
        {
            // MOVE RIGHT
            tAction[3] = action[3];
            tAction[4] = 0;
            tAction[5] = 0;
        }
        else // If left is the greatest...
        {
            // MOVE LEFT
            tAction[3] = 0;
            tAction[4] = action[4];
            tAction[5] = 0;
        }
        if (action[8] > action[6] && action[8] > action[7]) // If No rotation is the greatest input...
        {
            // NO ROTATION
            tAction[6] = 0;
            tAction[7] = 0;
            tAction[8] = action[8];
        }
        else if (action[4] < action[5]) // If rotate right...
        {
            // ROTATE RIGHT
            tAction[6] = action[6];
            tAction[7] = 0;
            tAction[8] = 0;
        }
        else // If rotate left...
        {
            // ROTATE LEFT
            tAction[6] = 0;
            tAction[7] = action[7];
            tAction[8] = 0;
        }
        return tAction;
    }
    public float[] ArgmaxAction(float[] action)
    {
        float[] amAction = new float[action.Length];
        for (int i = 0; i < action.Length - 1; i++)
        {
            if (action[i] < action[i + 1])
            {
                amAction[i] = 0;
                amAction[i + 1] = action[i + 1];
            }
        }
        return amAction;
    }
    // Create a random action
    public float[] RandomAction()
    {
        float[] randAction = new float[actionQty]; // Create a new float array to hold the action values

        // Loop through the array and add a random number to each index
        for (int i = 0; i < randAction.Length; i++)
        {
            randAction[i] = UnityEngine.Random.Range(0, 4); // Just picked an arbitrary range which should not matter much for a random action. *Remember inclusive/exclusive with float variables
        }
        return randAction; // Return random actions array
    }

    // Get an action based on state, or small chance (epsilon) for a random action
    public float[] EpsilonGreedy(float[] state, float eps) // States are passed to neural network and returns action
    {
        // The probability for a random action is based on epsilon. Epsilon will gradually reduce which means the agent's behavior will become less random over time. *Exploration vs. Exploitation
        if (UnityEngine.Random.value < eps) // Python: Random value needs to match np.random.random() - returns a half-open interval [0.0, 1.0)
        {
            // Random action
            // Python: return random.choice(self.K) - return a random element from the 1-D array
            return RandomAction();
        }
        else
        {
            // Action via neural net
            // Python: return np.argmax(self.predict([x])[0]) - return the highes
            return dqn.mainNet.FeedForward(state);
        }
    }
    // Performs an action based on inputs from a boolean array
    public void PerformAction(bool[] action)
    {
        // Accept action outputs and apply them to respective pawn functions
        aiPawn.B_MoveForward(action[0]);
        aiPawn.B_MoveBack(action[1]);
        aiPawn.B_NoVertical(action[2]);
        aiPawn.B_MoveRight(action[3]);
        aiPawn.B_MoveLeft(action[4]);
        aiPawn.B_NoLateral(action[5]);
        aiPawn.B_RotateRight(action[6]);
        aiPawn.B_RotateLeft(action[7]);
        aiPawn.B_NoRotation(action[8]);
    }
    // Buffer that stores (s, a, r, s') tuples 
    public void ExperienceReplay(int lastFrameIdx, float[] action, float reward, bool done) // The state and next state are stored as one float array called frameBuffer.
    {
        experienceBuffer[bufferIndex] = Tuple.Create(lastFrameIdx, action, reward, done); // Add the new tuple to the experienceBuffer

        // The bufferCount will be set to the max of bufferCount and bufferIndex + 1. This tracks the total size of the buffer and will remain at the max once the buffer is filled
        bufferCount = Mathf.Max(bufferCount, bufferIndex + 1);

        // bufferIndex is set to the modulus of bufferIndex + 1. This will reset current to 0 when the buffer is full. (This represents the current index to fill with new tuples)
        bufferIndex = (bufferIndex + 1) % EXP_BUFFER_SIZE;
    }
    // Get a mini-batch of tuples from the experience buffer to train the agent
    public Tuple<int, float[], float, bool>[] GetMiniBatch(Tuple<int, float[], float, bool>[] exp_Buffer)
    {
        Tuple<int, float[], float, bool>[] mb = new Tuple<int, float[], float, bool>[MINI_BATCH_SIZE];

        for (int i = 0; i < MINI_BATCH_SIZE; i++)
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
    // Train the Agent using the target neural network
    public bool Train(Tuple<int, float[], float, bool>[] expBuffer) // Frame buffer, action, reward, isDone are passed in
    {
        // Get a random batch from the experience buffer
        Tuple<int, float[], float, bool>[] miniBatch = GetMiniBatch(expBuffer);

        // Create array of states and nextStates
        float[][] states = new float[MINI_BATCH_SIZE][];
        float[][] nextStates = new float[MINI_BATCH_SIZE][];
        float[][] actions = new float[MINI_BATCH_SIZE][];
        float[] rewards = new float[MINI_BATCH_SIZE];
        bool[] dones = new bool[MINI_BATCH_SIZE];

        for (int i = 0; i < MINI_BATCH_SIZE; i++)
        {
            if (expBuffer[i] != null)
            {
                states[i] = env.GetState(env.frameBuffer, expBuffer[i].Item1);
                nextStates[i] = env.GetState(env.frameBuffer, expBuffer[i].Item1);
                actions[i] = expBuffer[i].Item2;
                rewards[i] = expBuffer[i].Item3;
                dones[i] = expBuffer[i].Item4;
            }
        }

        //****** EVERYTHING AFTER THIS IS UNDER CONSTRUCTION!!!

        // Calculate targets      
        float[][] nextQValues = new float[MINI_BATCH_SIZE][];

        //Python: next_Qs = target_model.predict(next_states)
        for (int i = 0; i < MINI_BATCH_SIZE; i++)
        {
            if (nextStates[i] != null)
            {
                nextQValues[i] = dqn.targetNet.FeedForward(nextStates[i]);
            }
        }
        //Python: next_Q = np.amax(next_Qs, axis = 1) - Returns an array containing the max value for each row
        // Pass nextQValues to DetermineAction to only return the strongest action values
        float[][] tNextQValues = new float[MINI_BATCH_SIZE][];
        for (int i = 0; i < MINI_BATCH_SIZE; i++)
        {
            if (nextQValues[i] != null)
            {
                tNextQValues[i] = TrimAction(nextQValues[i]);
            }
        }
        //Python: targets = rewards + np.invert(dones).astype(np.float32) * gamma * next_Q
        float[][] targets = new float[MINI_BATCH_SIZE][];

        for (int i = 0; i < MINI_BATCH_SIZE; i++)
        {
            if (targets[i] != null)
            {
                for (int j = 0; j < MINI_BATCH_SIZE; j++)
                {

                    float done = 0;
                    if (dones[i] == true)
                    {
                        done = 0f; // Done being true should return 0, this is the inverted value of the boolean variable which replaces (np.invert(dones).astype(np.float32)
                    }
                    else
                    {
                        done = 1f;
                    }
                    targets[i][j] = rewards[i] + (done * gamma * nextQValues[i][j]);
                }
            }
        }
        // Update model
        double[][] loss = UpdateModel(states, actions, targets); // TODO: Pass in nextQValues or actions?

        return true;
    }
    // TODO: Finish UpdateModel method
    public double[][] UpdateModel(float[][] states, float[][] actions, float[][] targets) // Python: def update(self, states, actions, targets):
    {
        double[][] loss = LossFunction(actions, targets);
        MinimizeFunction(loss, dqn.targetNet.weightsMatrix);

        return loss; // Return cost
    }
    // TODO: Calculate Loss (AKA Cost) using Huber Loss function
    public double[][] LossFunction(float[][] actions, float[][] targets)
    {
        // PYTHON: cost = tf.reduce_mean(tf.losses.huber_loss(self.G, selected_action_values))
        // x = selected_actions, y = targets
        float delta = 1; // TODO: Research what is a valid delta value
        double[][] loss = new double[MINI_BATCH_SIZE][];

        float[][] selected_Actions = new float[MINI_BATCH_SIZE][];
        for (int i = 0; i < MINI_BATCH_SIZE; i++)
        {
            selected_Actions[i] = ArgmaxAction(actions[i]);
        }
            

        // Huber Loss Algorithm
        for (int i = 0; i < MINI_BATCH_SIZE; i++) // TODO: Check if MINI_BATCH_SIZE is the correct max
        {
            if (selected_Actions[i] != null && targets[i] != null)
            {
                for (int j = 0; j < actionQty; j++)
                {
                    if (Math.Abs(targets[i][j] - selected_Actions[i][j] ) <= delta)
                    {
                        // Quadratic function
                        loss[i][j] = 0.5d * Math.Pow((targets[i][j] - selected_Actions[i][j]), 2);
                    }
                    else
                    {
                        // Linear function
                        loss[i][j] = delta * Math.Abs(targets[i][j] - selected_Actions[i][j]) - 0.5d * Math.Pow(delta, 2);
                    }
                }
            }
        }   
        return loss;
    } 

    // TODO: Create the selected actions that will be used to calculate cost
    public float[][] SelectedActions(float[][] actions)   
    {

        float[][] selectedActionValues = new float[EXP_BUFFER_SIZE][];
        // Python: self.predict_op * tf.one_hot(self.actions, K) // Get the highest value in each column and output the results in separate arrays, each representing a possible action
        return selectedActionValues;
    }
    public float CalculateGradient(float[][][] weightParams)
    {
        float gradient = 0;
        return gradient;
    }
    // TODO: Minimize cost using Adam optimizer
    public float[][][] MinimizeFunction(double[][] loss, float[][][] weights) // TODO: This function will return the updated parameters for optimization
    {
        float learningRate = 0.001f;
        float decay_1 = 0.9f;
        float decay_2 = 0.999f;
        int parameterQty = 10; // TODO: Set this properly
        double epsilonHat = Math.Pow(10, -5); // Hyperparameter that I have seen set between 10^-8 and 10^-5 (AKA 1e-8 and 1e-5), also 1 or 0.1 have been suggested
        double[][] learning_Rates = new double[EXP_BUFFER_SIZE][];
        float[][][] parameters = dqn.mainNet.weightsMatrix; // TODO: Parameters should be neural network weights
        double[][] firstMoment= new double[EXP_BUFFER_SIZE][];
        double[][] secondMoment = new double[EXP_BUFFER_SIZE][];
        for (int i = 0; i < parameterQty; i++)
        {
            firstMoment[0][i] = 0; // Initialize the first moment vector, index 0 as 0
            secondMoment[0][i] = 0; // Initialize the second moment vector, index 0 as 0
            //parameters[0][i] = 0; //Initialize the initial parameter vector as 0
        }
        int t = 0; // TimeStep
        bool isConverged = false;

        // TODO: Optimize algorithm by replacing the end with the suggested improvement on page two of the research paper
        while (!isConverged)
        {
            t++;
            if (t < EXP_BUFFER_SIZE) // Check that the current timestep is less than the experience buffer size
            {
                float gradient = CalculateGradient(parameters); // TODO: Delta 0ft (0t - 1) // Gradient = change in parameters given the previous parameters
                for (int i = 0; i < parameterQty; i++)
                {
                    if (learning_Rates[t] != null)
                    {
                        learning_Rates[t][i] = learningRate * Math.Sqrt(1 - Math.Pow(decay_2, t)) / (1 - Math.Pow(decay_1, t));
                    }
                }
                for (int i = 0; i < parameterQty; i++)
                {
                    if (firstMoment[t] != null && secondMoment[t] != null)
                    {
                        firstMoment[t][i] = decay_1 * firstMoment[t - 1][i] + (1 - decay_1) * gradient;
                        secondMoment[t][i] = decay_2 * secondMoment[t - 1][i] + (1 - decay_2) * Math.Pow(gradient, 2);
                    }
                }
                
                for (int i = 0; i < weights.Length; i++)
                {
                    for (int j = 0; j < weights[i].Length; j++)
                    {
                        for (int k = 0; k < weights[i][j].Length; k++)
                        {
                            if (parameters[i][j][k] != null)
                            {
                                //parameters[i][j][k] = parameters[i][j][k] - learning_Rates[t][i] * firstMoment[t][i] / (Math.Sqrt(secondMoment[t][i]) + epsilonHat);
                            }
                        }
                    }
                }
            }
            else // If the experience buffer is full, then reset to the beginning.
            {
                t = 0;
                isConverged = true; // TODO: Need to move this to a proper location, but this is required to not send the game into an infinite loop
                // TODO: Or end of training session.
            }
        }
        return parameters;
    }
}
