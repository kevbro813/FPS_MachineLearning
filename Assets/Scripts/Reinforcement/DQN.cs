using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DQN : MonoBehaviour
{
    public NeuralNetwork mainNet;
    public NeuralNetwork targetNet;
    public bool initialized = false;
    public bool isTraining = true;
    public int episodeCount = 0;
    public int episodeMax = 10;
    public Environment env;
    public Agent agent;
    private int[] mLayers = new int[] { 50, 80, 50, 5 };
    private int[] tLayers = new int[] { 50, 80, 50, 5 };
    public int inputsPerState;
    public float epsilon = 1.0f;
    public float epsilon_min = 0.1f;
    public float epsilon_change;
    private void Start()
    {
        agent = GetComponent<Agent>();
        env = GetComponent<Environment>();
        inputsPerState = 10 * 5; // TODO: When you are done being lazy, make it two variables or const for input and frames per state
        epsilon_change = (epsilon - epsilon_min) / 500000;
    }
    private void Update()
    {
        RunGame();
    }
    public void InitQNets()
    {
        NeuralNetwork mNet = new NeuralNetwork(mLayers);
        mNet.Mutate();
        InitMainNet(mNet);

        NeuralNetwork tNet = new NeuralNetwork(tLayers);
        tNet.Mutate();
        InitTargetNet(tNet);
    }

    public void InitMainNet(NeuralNetwork net)
    {
        // Initialize Main Neural Network
        this.mainNet = net;
        initialized = true;
    }
    public void InitTargetNet(NeuralNetwork net)
    {
        // Initialize Target Neural Network
        this.targetNet = net;
        initialized = true;
    }
    public void RunGame()
    {
        InitQNets();
        for (int i = 1; i <= episodeMax; i++) // For each episode...
        {
            episodeCount = i; // Increase episode count. Will be used for display
            float episodeReward = RunEpisode(agent, env); // Run the RunEpisode method passing in the agent and environment and returning the score (reward) for the episode.
            //rewards.Add(reward); // Add the score to the list of reward scores. TODO: Sort functionality, Icomparable.

            // TODO: Save Episodes
        }
    }

    // Run one episode
    public float RunEpisode(Agent agent, Environment env)
    {
        // Reset the environment's state each episode
        env.ResetEnv();    
        
        // TODO: Create initial state

        bool isDone = false;
        while (!isDone)
        {
            // Copy weights from main to target network periodically
            if (env.stateCounter % 100 == 0)
            {
                targetNet = mainNet;
            }

            // Get state from fram buffer
            float[] currentState = env.GetState(env.frameBuffer); // **DONE

            // Input the state and return an action
            float[] currentAction = agent.GetAction(currentState, epsilon);

            // Perform the action
            agent.PerformAction(currentAction); // **DONE

            // Creates next frame
            float[] nextFrame = env.GetNextFrame(); // **NONE

            // Add frame to frame buffer
            env.frameBuffer = env.UpdateFrameBuffer(nextFrame); // **DONE

            // Calculates the reward based on the state
            float currentReward = env.CalculateReward();

            // Update experience replay memory
            agent.ExperienceReplay(env.frameBuffer, currentAction, currentReward); // **DONE

            env.stateCounter++;

            // Train the agent
            isDone = agent.Train(agent.experienceBuffer);

            // Keep track of time

            // Track training time

            // Increment steps in episode

            // state = mext state

            // Increment total_t

            // Recalculate epsilon
            epsilon = Mathf.Max(epsilon - epsilon_change, epsilon_min); // ** DONE

        }
        return 0; // Need to return the reward total for the episode
    }
}
