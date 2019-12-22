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
    public bool[] DetermineAction(float[] action)
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
    public float[] GetAction(float[] state, float eps) // States are passed to neural network and returns action
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
            // Python: return np.argmax(self.predict([x])[0]) - return the highest
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
    //TODO: Train the Agent using the target neural network
    public bool Train(Tuple<int, float[], float, bool>[] expBuffer) // Frame buffer, action, reward, isDone are passed in
    {
        // Get a random batch from the experience buffer
        Tuple<int, float[], float, bool>[] miniBatch = GetMiniBatch(expBuffer);

        // Create array of states and nextStates
        float[][] states = new float[MINI_BATCH_SIZE][];
        float[][] nextStates = new float[MINI_BATCH_SIZE][];
        
        for (int i = 0; i < MINI_BATCH_SIZE; i++)
        {
            if (expBuffer[i] != null)
            {
                states[i] = env.GetState(env.frameBuffer, expBuffer[i].Item1);
                nextStates[i] = env.GetState(env.frameBuffer, expBuffer[i].Item1);
            }
        }

        // Calculate targets      
        float[][] nextQValues = new float[MINI_BATCH_SIZE][];

        //Python: next_Qs = target_model.predict(next_states)
        for (int i = 0; i < MINI_BATCH_SIZE; i++)
        {
            if(nextStates[i] != null)
            {
                nextQValues[i] = dqn.targetNet.FeedForward(nextStates[i]);
            }
        }

        //Python: next_Q = np.amax(next_Qs, axis = 1) - Returns an array containing the max value for each row
        // Pass nextQValues to DetermineAction to get boolean action values
        bool[][] bNextQValues = new bool[MINI_BATCH_SIZE][];
        for (int i = 0; i < MINI_BATCH_SIZE; i++)
        {
            if(nextQValues[i] != null)
            {
                bNextQValues[i] = DetermineAction(nextQValues[i]);
            }
        }

        //Python: targets = rewards + np.invert(dones).astype(np.float32) * gamma * next_Q
        // TODO: Calculate targets

        // TODO: Update model

        //Python: loss = model.update(states, actions, targets)
        // TODO: Calculate Loss

        //Python: return loss
        // TODO: RESEARCH
        return true;
    }
    // TODO: Get a mini-batch of tuples from the experience buffer to train the agent
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
}
