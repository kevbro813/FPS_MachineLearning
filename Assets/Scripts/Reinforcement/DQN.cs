using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DQN : MonoBehaviour
{
    public NeuralNetwork mainNet;
    public NeuralNetwork targetNet;
    public bool initialized = false;
    public bool isTraining = true;
    public int episodeNum = 1;
    public int episodeMax = 10;
    public Environment env;
    public Agent agent;
    public int[] layers;
    public float epsilon = 1.0f; // Used in GetAction function, Epsilon is basically the chance for a random action, Epsilon gradually reduces until it reaches epsilon_min
    public float epsilon_min = 0.1f; // epsilon_min is the lowest value for epsilon, i.e. 0.1 means there is a 10% chance for a random action
    public float epsilon_change; // This is the rate at which the value of epsilon will reduce each update
    public float episodeReward = 0;
    public int epiSteps = 0;
    public bool isDone = false;
    float[] currentState;
    double[] currentAction;
    bool[] bAction;
    float[] nextFrame;
    int lastFrameIndex;
    float currentReward;
    public int layerQty;
    private void Start()
    {
        agent = GetComponent<Agent>();
        env = GetComponent<Environment>();

        InitQNets();
        epsilon_change = (epsilon - epsilon_min) / 500000;
        currentState = new float[env.framesPerState];
        currentAction = new double[agent.actionQty];
        bAction = new bool[agent.actionQty];
        nextFrame = new float[env.frameSize];
        isDone = false;

        env.InitEnv();
        agent.InitAgent();
    }
    private void FixedUpdate()
    {
        RunGame();
    }
    public void InitQNets()
    {
        layers = new int[] { 45, 45, 45, 7 };
        agent.actionQty = layers[layers.Length - 1];
        layerQty = layers.Length;

        NeuralNetwork mNet = new NeuralNetwork(layers);
        mNet.Mutate();
        InitMainNet(mNet);

        NeuralNetwork tNet = mNet;
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
        if (episodeNum <= episodeMax)
        {
            isDone = false;
        }

        if (episodeNum == episodeMax)
        {
            Debug.Log("Game Over.");
        }
        RunEpisode(agent, env); // Run the RunEpisode method passing in the agent and environment and returning the score (reward) for the episode.    
        //episodes.Add(episode); // Add the score to the list of rewards, neural nets and other data. TODO: Sort functionality, Icomparable.
    }
    // Run one episode
    public float RunEpisode(Agent agent, Environment env)
    {    
        if (!isDone)
        {
            // Copy weights from main to target network periodically
            if (env.stepCounter % 1000 == 0)
            {
                targetNet = mainNet;
            }

            // Get state from frame buffer
            currentState = env.GetState(env.frameBuffer, env.fbIndex); // **DONE

            // Input the state and return an action using Epsilon Greedy function (Explore and Exploit)
            currentAction = agent.EpsilonGreedy(currentState, epsilon); // **DONE

            // Convert action to binary which is used by the pawn
            bAction = agent.BinaryAction(currentAction); // **DONE

            // Perform the action
            agent.PerformAction(bAction); // **DONE

            env.stepCounter++; // Increment total steps
            epiSteps++; // Increment steps in episode

            if (epiSteps >= env.epiMaxSteps)
            {
                episodeNum++;
                isDone = true;
                epiSteps = 0;     
            }

            // Creates next frame
            nextFrame = env.GetNextFrame(); // **DONE

            // Add frame to frame buffer
            lastFrameIndex = env.UpdateFrameBuffer(nextFrame); // **DONE

            // Calculates the reward based on the state
            currentReward = env.CalculateReward(nextFrame); // ** IN PROGRESS

            // Add current reward to the episode total
            episodeReward += currentReward;

            // Update experience replay memory
            agent.ExperienceReplay(lastFrameIndex, currentAction, currentReward, isDone); // **DONE

            if (isTraining == true)
            {
                // Train the agent
                mainNet.weightsMatrix = agent.Train(agent.experienceBuffer, mainNet.weightsMatrix, mainNet.gradients, mainNet.nodeSignals, mainNet.neuronsMatrix); // **IN PROGRESS
            }

            // Keep track of time

            // Track training time

            // state = mext state

            // Recalculate epsilon
            epsilon = Mathf.Max(epsilon - epsilon_change, epsilon_min); // ** DONE
        }
        
        return episodeReward; // Need to return the reward total for the episode
    }
}
