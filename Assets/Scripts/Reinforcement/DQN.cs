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
    public int[] mLayers;
    public int[] tLayers;
    public int inputsPerState;
    public float epsilon = 1.0f; // Used in GetAction function, Epsilon is basically the chance for a random action, Epsilon gradually reduces until it reaches epsilon_min
    public float epsilon_min = 0.1f; // epsilon_min is the lowest value for epsilon, i.e. 0.1 means there is a 10% chance for a random action
    public float epsilon_change; // This is the rate at which the value of epsilon will reduce each update
    public float episodeReward;
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
        mLayers = new int[] { 50, 80, 50, 9 };
        tLayers = new int[] { 50, 80, 50, 9 };

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
            //episodes.Add(episode); // Add the score to the list of rewards, neural nets and other data. TODO: Sort functionality, Icomparable.

            // TODO: Save Episodes
        }
    }

    // Run one episode
    public float RunEpisode(Agent agent, Environment env)
    {
        env.InitEnv();
        agent.InitAgent();
        episodeReward = 0;
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
            float[] currentState = env.GetState(env.frameBuffer, env.fbIndex); // **DONE

            // Input the state and return an action
            float[] currentAction = agent.GetAction(currentState, epsilon); // **DONE

            // Determine action function will return argmax between movement pairs, and convert to a binary action output
            bool[] bAction = agent.BinaryAction(currentAction); // **DONE

            // Perform the action
            agent.PerformAction(bAction); // **DONE

            // Creates next frame
            float[] nextFrame = env.GetNextFrame(); // **DONE
            
            // Add frame to frame buffer
            int lastFrameIndex = env.UpdateFrameBuffer(nextFrame); // **DONE

            // Calculates the reward based on the state
            float currentReward = env.CalculateReward(); // ** IN PROGRESS

            // Add current reward to the episode total
            episodeReward += currentReward; 

            // Update experience replay memory
            agent.ExperienceReplay(lastFrameIndex, currentAction, currentReward, isDone); // **DONE

            // Train the agent
            isDone = agent.Train(agent.experienceBuffer); // **IN PROGRESS

            env.stateCounter++; // Increment total_t

            // Keep track of time

            // Track training time

            // Increment steps in episode

            // state = mext state

            // Recalculate epsilon
            epsilon = Mathf.Max(epsilon - epsilon_change, epsilon_min); // ** DONE

        }
        return 0; // Need to return the reward total for the episode
    }
}
