using UnityEngine;
using System;
using UnityEditor;

[Serializable]
public class Agent
{
    private AIPawn aiPawn;
    private RLComponent rlComponent;
    private int expBufferSize; // Size of the experience buffer
    private int actionQty; // Number of actions
    public Tuple<int, int, double, bool>[] experienceBuffer; // Tuple that holds the Index of the last frame(used to calculate states), action, reward and done flag 
    public Tuple<int, double, double[], double, bool>[] ppoExperienceBuffer;
    public int bufferIndex; // Keeps track of the current index of the buffer "Count"
    public int bufferCount; // Tracks the size of the buffer
    public bool isExploit = true; // Is the agent's action explore or exploit (will change the color of the agent to red if exploit and white if explore)

    /// <summary>
    /// Converts date to an int that can be used as the seed for RNG
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    private int DateToInt(DateTime dateTime)
    {
        return dateTime.Year + dateTime.Month + dateTime.Day + dateTime.Hour + dateTime.Minute + dateTime.Second + dateTime.Millisecond;
    }
    /// <summary>
    /// Initialize a new agent with an empty experienceBuffer
    /// </summary>
    /// <param name="p"></param>
    /// <param name="rl"></param>
    /// <param name="actQty"></param>
    public void Init_Agent(AIPawn p, RLComponent rl, int actQty)
    {
        aiPawn = p;
        rlComponent = rl;
        int seed = DateToInt(DateTime.Now);
        UnityEngine.Random.InitState(seed);
        expBufferSize = RLManager.instance.settings.expBufferSize;
        experienceBuffer = new Tuple<int, int, double, bool>[expBufferSize];
        ppoExperienceBuffer = new Tuple<int, double, double[], double, bool>[expBufferSize];
        bufferIndex = 0;
        bufferCount = 0;
        actionQty = actQty;
    }
    /// <summary>
    /// Performs an action based on inputs from a boolean array, then returns the action's index to be stored in the experience buffer.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="eps"></param>
    /// <returns></returns>
    public int DQNAction(double[] state, float eps)
    {
        // Input the state and return an action using Epsilon Greedy function (Explore and Exploit)
        int action = EpsilonGreedy(state, eps);

        PerformAction(action); // Accept action outputs and apply them to respective pawn functions

        return action; // Return the index of the action taken
    }
    /// <summary>
    /// This method takes a softmax distribution and calculates an action based on the distribution and performs the action. It then returns the index of the action taken.
    /// </summary>
    /// <param name="dist"></param>
    /// <returns></returns>
    public int PPOAction(double[] dist)
    {
        int action = PPORandomAction(dist); // choose a random action based on the prediction probabilities random.choice(actionQty, prediction)

        PerformAction(action); // Accept action outputs and apply them to respective pawn functions

        return action; // Return the index of the action taken
    }
    /// <summary>
    /// Determines an action randomly, based on the softmax distribution (therefore it is not a normal distribution)
    /// </summary>
    /// <param name="distribution"></param>
    /// <returns></returns>
    private int PPORandomAction(double[] distribution)
    {
        double random = (double)UnityEngine.Random.Range(0f, 1f); // Random variable between 0 and 1
        for (int i = 0; i < actionQty; i++)
        {
            if (random < distribution[i]) // If the random number is less than the softmax distribution return the index
                return i; 
            random -= distribution[i]; // Otherwise, subtract the random number by the distribution and move on to the next action
        }

        return -1; // This should only be returned if there is a problem with softmax, i.e. values do not add up exactly to 1
    }
    /// <summary>
    /// Takes an integer representing the index of the action to be performed and performs the corresponding action.
    /// </summary>
    /// <param name="action"></param>
    private void PerformAction(int action)
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
        if (action == 3)
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
    /// <summary>
    /// Performs a random action or one determined by neural network. Epsilon is the percentage that the action is random and will decay over the training session to a minimum.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="eps"></param>
    /// <returns></returns>
    public int EpsilonGreedy(double[] state, float eps) // States are passed to neural network and returns action
    {
        // The probability for a random action is based on epsilon. Epsilon will gradually reduce which means the agent's behavior will become less random over time. *Exploration vs. Exploitation
        if (UnityEngine.Random.value < eps)
        {
            // Random action
            isExploit = false;
            return UnityEngine.Random.Range(0, actionQty);
        }
        else
        {
            // Action via neural net, the strongest, AKA ArgMax action
            isExploit = true;
            return RLManager.math.ArgMax(rlComponent.mainNet.FeedForward(state));
        }
    }
    /// <summary>
    /// Buffer that stores (s, a, r, s') tuples 
    /// </summary>
    /// <param name="idx"></param>
    /// <param name="action"></param>
    /// <param name="reward"></param>
    /// <param name="done"></param>
    public void ExperienceReplay(int idx, int action, double reward, bool done) // The state and next state are stored as one float array called frameBuffer.
    {
        Tuple<int, int, double, bool> nTuple = new Tuple<int, int, double, bool>(idx, action, reward, done); // Create a new tuple with the data passed in

        experienceBuffer[bufferIndex] = nTuple; // Add the new tuple to the experienceBuffer
    }
    /// <summary>
    /// Stores the data required for PPO training.
    /// </summary>
    /// <param name="actions"></param>
    /// <param name="rewards"></param>
    /// <param name="predictions"></param>
    /// <param name="values"></param>
    /// <param name="oneHot"></param>
    /// <param name="oldLog"></param>
    /// <param name="dones"></param>
    public void PPOExperience(int actions, double rewards, double[] predictions, double values, bool dones) 
    {
        double[] preds = new double[actionQty];
        for (int i = 0; i < actionQty; i++)
        {
            preds[i] = predictions[i]; // This is needed to store the prediction values properly in the tuple
        }
        // Create a new tuple with the data passed in
        Tuple<int, double, double[], double, bool> nTuple = new Tuple<int, double, double[], double, bool>(actions, rewards, preds, values, dones); 
        
        ppoExperienceBuffer[bufferIndex] = nTuple; // Add the new tuple to the experienceBuffer
    }
    /// <summary>
    /// Update the bufferCount and bufferIndex variables.
    /// </summary>
    public void UpdateExperienceBufferCounters()
    {
        // The bufferCount will be set to the max of bufferCount and bufferIndex + 1. This tracks the total size of the buffer and will remain at the max once the buffer is filled
        bufferCount = Mathf.Max(bufferCount, bufferIndex + 1);

        // bufferIndex is set to the modulus of bufferIndex + 1. This will reset current to 0 when the buffer is full. (This represents the current index to fill with new tuples)
        bufferIndex = (bufferIndex + 1) % expBufferSize;
    }
    /// <summary>
    /// Get a mini-batch of tuples from the experience buffer to train the agent
    /// </summary>
    /// <param name="miniBatchSize"></param>
    /// <param name="frameBufferIndex"></param>
    /// <param name="framesPerState"></param>
    /// <returns></returns>
    public Tuple<int, int, double, bool>[] GetMiniBatch(int miniBatchSize, int frameBufferIndex, int framesPerState) // mini batch size, frame buffer index, frames per state
    {
        Tuple<int, int, double, bool>[] miniBatch = new Tuple<int, int, double, bool>[miniBatchSize];

        for (int i = 0; i < miniBatch.Length; i++) // Loop through the mini-batch tuple
        {
            start:
            int rand = UnityEngine.Random.Range(0, bufferCount); // Get a random buffer index to add to the mini-batch

            // Avoid using old/new frames together. Do not use frames from frameBufferIndex to (frameBufferIndex + framesPerState)
            if (experienceBuffer[rand].Item1 > frameBufferIndex && experienceBuffer[rand].Item1 <= frameBufferIndex + framesPerState)
            {
                goto start; // Return to start and get a new random index
            }
            miniBatch[i] = experienceBuffer[rand]; // Add the random memory to the mini-batch
        }
        return miniBatch; // Return the completed mini-batch
    }
}
