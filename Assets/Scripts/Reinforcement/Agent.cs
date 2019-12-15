using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    public AIPawn aiPawn;
    public DQN dqn;
    public Environment env;
    public float[][][] experienceBuffer;
    public int bufferIndex = 0;
    public const int EXP_BUFFER_SIZE = 10000;

    private void Start()
    {
        env = GetComponent<Environment>();
        aiPawn = GetComponent<AIPawn>();
        dqn = GetComponent<DQN>();
        InitAgent();
    }
    public void InitAgent()
    {
        // Load the model the agent will use   
        experienceBuffer = new float[EXP_BUFFER_SIZE][][];
        bufferIndex = 0;
    }
    public float[] GetAction(float[] state) // States are passed to neural network and returns action
    {
        float[] actions = dqn.mainNet.FeedForward(state);
        return actions;
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
        // Train the model using a neural network (states form inputs)
        //dqn.targetNet.FeedForward(state);
        // Input = s
        // if (isDone) then target = r
        // else  target = r + gamma * max {Q (s',:)}
        // Then use gradient descent with momentum with the target for the state
        return true;        
    }

}
