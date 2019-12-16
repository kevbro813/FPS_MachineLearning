using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Agent : MonoBehaviour
{
    public AIPawn aiPawn;
    public DQN dqn;
    public Environment env;
    public float[][][] experienceBuffer;
    public int bufferIndex = 0;
    public const int EXP_BUFFER_SIZE = 10000;
    public int actionQty;

    private void Start()
    {   
        env = GetComponent<Environment>();
        aiPawn = GetComponent<AIPawn>();
        dqn = GetComponent<DQN>();
        InitAgent();
        int seed = DateToInt(DateTime.Now);
        UnityEngine.Random.InitState(seed);
        
    }
    public int DateToInt(DateTime dateTime)
    {
        return dateTime.Year + dateTime.Month + dateTime.Day + dateTime.Hour + dateTime.Minute + dateTime.Second + dateTime.Millisecond;
    }
    public void InitAgent()
    {
        // Load the model the agent will use   
        experienceBuffer = new float[EXP_BUFFER_SIZE][][];
        bufferIndex = 0;
    }
    public float[] GetAction(float[] state, float eps) // States are passed to neural network and returns action
    {
        // Small probability for a random action based on epsilon, exploit/explore
        if(UnityEngine.Random.value < eps) // Random value needs to match np.random.random() - returns a half-open interval [0.0, 1.0)
        {
            // Random action
            // return random.choice(self.K) - return a random element from the 1-D array
            //int rand = UnityEngine.Random.Range(0, 5); // Range must match *Remember inclusive/exclusive
            // Create random action
            float[] randAction = new float[5]; // Create a new float array to hold the action values

            return null;
        }
        else
        {
            // Action via neural net
            //return np.argmax(self.predict([x])[0]) - return the highest
            return dqn.mainNet.FeedForward(state);
        }
    }

    public void ExperienceReplay(float[] frameBuffer, float[] action, float reward)
    {
        // Buffer that stores (s, a, r, s') tuples, but stored as 3 arrays since the frame buffer will account for both the current state and next state

        if (bufferIndex < EXP_BUFFER_SIZE)
        {
            experienceBuffer[bufferIndex][0] = frameBuffer;
            experienceBuffer[bufferIndex][1] = action;
            experienceBuffer[bufferIndex][2][0] = reward;
        }

        else
        {
            bufferIndex = 0;
            experienceBuffer[bufferIndex][0] = frameBuffer;
            experienceBuffer[bufferIndex][1] = action;
            experienceBuffer[bufferIndex][2][0] = reward;
        }

        bufferIndex++;
    }
    public void PerformAction(float[] action)
    {
        // Accept action outputs and apply them to respective pawn functions
        aiPawn.MoveForward(action[0]);
        aiPawn.MoveBack(action[1]);
        aiPawn.MoveRight(action[2]);
        aiPawn.MoveLeft(action[3]);
        aiPawn.Rotation(action[4]);
    }
    public bool Train(float[][][] expBuffer) // State, action, reward, next state, isDone are passed in
    {
        // Get a random batch from the experience buffer

        // Calculate targets      
        //next_Qs = target_model.predict(next_states)
        //next_Q = np.amax(next_Qs, axis = 1) - Returns an array containing the max value for each row
        //targets = rewards + np.invert(dones).astype(np.float32) * gamma * next_Q

        // Update model
        //loss = model.update(states, actions, targets)
        //return loss


        return true;        
    }

}
