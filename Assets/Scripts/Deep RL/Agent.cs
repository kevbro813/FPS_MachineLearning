using UnityEngine;
using System;

[Serializable]
public class Agent
{
    private AIPawn aiPawn;
    private DQN dqn;
    public Tuple<int, int, double, bool>[] experienceBuffer; // Tuple that holds the Index of the last frame(used to calculate states), action, reward and done flag 
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
        experienceBuffer = new Tuple<int, int, double, bool>[GameManager.instance.settings.expBufferSize];
        bufferIndex = 0;
        bufferCount = 0;
    }

    // Performs an action based on inputs from a boolean array
    public int PerformAction(double[] state, float eps, int actQty)
    {
        // Input the state and return an action using Epsilon Greedy function (Explore and Exploit)
        int action = EpsilonGreedy(state, eps, actQty);

        MoveAgent(action);
        // Accept action outputs and apply them to respective pawn functions
        
        return action;
    }

    private void MoveAgent(int action)
    {
        if (action == 0)
        {
            if (isExploit)
            {
                Debug.Log("Idle");
            }
            aiPawn.NoMovement(true);
        }
        else if (action == 1)
        {
            if (isExploit)
            {
                Debug.Log("Forward");
            }
            aiPawn.MoveForward(true);
        }
        else if (action == 2)
        {
            if (isExploit)
            {
                Debug.Log("Back");
            }
            aiPawn.MoveBack(true);
        }
        else if (action == 3)
        {
            if (isExploit)
            {
                Debug.Log("Right");
            }
            aiPawn.MoveRight(true);
        }
        else if (action == 4)
        {
            if (isExploit)
            {
                Debug.Log("Left");
            }
            aiPawn.MoveLeft(true);
        }
        //aiPawn.RotateRight(bAction[4]);
        //aiPawn.RotateLeft(bAction[5]);
    }
    // Get an action based on state, or small chance (epsilon) for a random action
    public int EpsilonGreedy(double[] state, float eps, int actQty) // States are passed to neural network and returns action
    {
        // The probability for a random action is based on epsilon. Epsilon will gradually reduce which means the agent's behavior will become less random over time. *Exploration vs. Exploitation
        if (UnityEngine.Random.value < eps)
        {
            // Random action
            isExploit = false;
            return UnityEngine.Random.Range(0, actQty);
        }
        else
        {
            // Action via neural net
            isExploit = true;
            return GameManager.instance.math.ArgMax(dqn.mainNet.FeedForward(state));
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
    public void ExperienceReplay(int idx, int action, double reward, bool done) // The state and next state are stored as one float array called frameBuffer.
    {
        Tuple<int, int, double, bool> nTuple = new Tuple<int, int, double, bool>(idx, action, reward, done); // Create a new tuple with the data passed in

        experienceBuffer[bufferIndex] = nTuple; // Add the new tuple to the experienceBuffer
    }

    public void UpdateExperienceBufferCounters()
    {
        // The bufferCount will be set to the max of bufferCount and bufferIndex + 1. This tracks the total size of the buffer and will remain at the max once the buffer is filled
        bufferCount = Mathf.Max(bufferCount, bufferIndex + 1);

        // bufferIndex is set to the modulus of bufferIndex + 1. This will reset current to 0 when the buffer is full. (This represents the current index to fill with new tuples)
        bufferIndex = (bufferIndex + 1) % GameManager.instance.settings.expBufferSize;
    }
    // Get a mini-batch of tuples from the experience buffer to train the agent
    public Tuple<int, int, double, bool>[] GetMiniBatch(int miniBatchSize, int frameBufferIndex, int framesPerState) // mini batch size, frame buffer index, frames per state
    {
        Tuple<int, int, double, bool>[] miniBatch = new Tuple<int, int, double, bool>[miniBatchSize];

        for (int i = 0; i < miniBatch.Length; i++) // Loop through the mini-batch tuple
        {
            start:
            int rand = UnityEngine.Random.Range(0, bufferCount); // Get a random buffer index to add to the mini-batch

            //if (experienceBuffer[rand].Item4 == true) // Skip any memories marked with a done flag
            //{
            //    goto start;
            //}

            // TODO: CHECK THIS**** Avoid using old/new frames together. Do not use frames from frameBufferIndex to (frameBufferIndex + framesPerState)
            if (experienceBuffer[rand].Item1 > frameBufferIndex && experienceBuffer[rand].Item1 <= frameBufferIndex + framesPerState)
            {
                goto start;
            }
            miniBatch[i] = experienceBuffer[rand]; // Add the random memory to the mini-batch
        }
        return miniBatch; // Return the completed mini-batch
    }
}
