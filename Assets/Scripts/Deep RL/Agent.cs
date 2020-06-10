using UnityEngine;
using System;

[Serializable]
public class Agent
{
    private AIPawn aiPawn;
    private DQN dqn;
    public Tuple<int, double[], double, bool>[] experienceBuffer; // Tuple that holds the Index of the last frame(used to calculate states), action, reward and done flag 
    public int bufferIndex; // Keeps track of the current index of the buffer "Count"
    public int bufferCount; // Tracks the size of the buffer
    public bool isExploit = false;

    // Converts date to an int that can be used as the seed for RNG
    public int DateToInt(DateTime dateTime)
    {
        return dateTime.Year + dateTime.Month + dateTime.Day + dateTime.Hour + dateTime.Minute + dateTime.Second + dateTime.Millisecond;
    }
    // Initialize a new agent with an empty experienceBuffer
    public void Init_Agent(AIPawn p, DQN d)
    {
        // TODO: Load the model the agent will use  
        aiPawn = p;
        dqn = d;
        int seed = DateToInt(DateTime.Now);
        UnityEngine.Random.InitState(seed);
        experienceBuffer = new Tuple<int, double[], double, bool>[GameManager.instance.settings.expBufferSize];
        bufferIndex = 0;
        bufferCount = 0;
    }

    // Performs an action based on inputs from a boolean array
    public double[] PerformAction(double[] state, float eps, int actQty)
    {
        // Input the state and return an action using Epsilon Greedy function (Explore and Exploit)
        double[] action = EpsilonGreedy(state, eps, actQty);

        // TODO: Delete Debug Code when done
        //Debug.Log("No Movement: " + action[0] + " Forward: " + action[1] + " Back: " + action[2] + " Right: " + action[3] + " Left: " + action[4]);
        //Debug.Log("State 1: " + state[0] + " State 2: " + state[9] + " State 3: " + state[19] + " State 4: " + state[29] + " State 5: " + state[39]);

        // Convert action to binary which is used by the pawn
        bool[] bAction = BinaryAction(action);

        // Accept action outputs and apply them to respective pawn functions
        aiPawn.NoMovement(bAction[0]);
        aiPawn.MoveForward(bAction[1]);
        aiPawn.MoveBack(bAction[2]);
        aiPawn.MoveRight(bAction[3]);
        aiPawn.MoveLeft(bAction[4]);
        
        //aiPawn.RotateRight(bAction[4]);
        //aiPawn.RotateLeft(bAction[5]);
        return action;
    }
    // Get an action based on state, or small chance (epsilon) for a random action
    public double[] EpsilonGreedy(double[] state, float eps, int actQty) // States are passed to neural network and returns action
    {
        // The probability for a random action is based on epsilon. Epsilon will gradually reduce which means the agent's behavior will become less random over time. *Exploration vs. Exploitation
        if (UnityEngine.Random.value < eps)
        {
            // Random action
            isExploit = false;
            return GameManager.instance.math.Softmax(RandomAction(actQty));
        }
        else
        {
            // Action via neural net
            isExploit = true;

            return dqn.mainNet.FeedForward(state);
        }
    }
    // The strongest action will be true, the rest will be false. Used to move the agent
    public bool[] BinaryAction(double[] action)
    {
        bool[] bAction = new bool[action.Length];
        int maxIndex = 0;
        for (int i = 1; i < bAction.Length; i++) // Loop through each action
        {
            if (action[maxIndex] > action[i]) // If the action 
            {
                bAction[maxIndex] = true;
                bAction[i] = false;
            }
            else
            {
                bAction[maxIndex] = false;
                bAction[i] = true;
                maxIndex = i;
            }  
        }
        return bAction;
    }

    // Create a random action
    public double[] RandomAction(int actQty)
    {
        double[] randAction = new double[actQty]; // Create a new float array to hold the action values

        // Loop through the array and add a random number to each index
        for (int i = 0; i < randAction.Length; i++)
        {
            randAction[i] = (double)UnityEngine.Random.value; // Just picked an arbitrary range which should not matter much for a random action. *Remember inclusive/exclusive with float variables
        }
        return randAction; // Return random actions array
    }
    // Buffer that stores (s, a, r, s') tuples 
    public void ExperienceReplay(int idx, double[] action, double reward, bool done) // The state and next state are stored as one float array called frameBuffer.
    {
        Tuple<int, double[], double, bool> nTuple = new Tuple<int, double[], double, bool>(idx, action, reward, done); // Create a new tuple with the data passed in

        experienceBuffer[bufferIndex] = nTuple; // Add the new tuple to the experienceBuffer

        // The bufferCount will be set to the max of bufferCount and bufferIndex + 1. This tracks the total size of the buffer and will remain at the max once the buffer is filled
        bufferCount = Mathf.Max(bufferCount, bufferIndex + 1);

        // bufferIndex is set to the modulus of bufferIndex + 1. This will reset current to 0 when the buffer is full. (This represents the current index to fill with new tuples)
        bufferIndex = (bufferIndex + 1) % GameManager.instance.settings.expBufferSize;
    }
    // Get a mini-batch of tuples from the experience buffer to train the agent
    public Tuple<int, double[], double, bool>[] GetMiniBatch(int mbs)
    {
        Tuple<int, double[], double, bool>[] miniBatch = new Tuple<int, double[], double, bool>[mbs];

        for (int i = 0; i < miniBatch.Length; i++) // Loop through the mini-batch tuple
        {
            start:
            int rand = UnityEngine.Random.Range(0, bufferCount); // Get a random buffer index to add to the mini-batch

            if (experienceBuffer[rand].Item4 == true) // Skip any memories marked with a done flag
            {
                goto start;
            }
            miniBatch[i] = experienceBuffer[rand]; // Add the random memory to the mini-batch
        }
        return miniBatch; // Return the completed mini-batch
    }
}
