using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DQN : MonoBehaviour
{
    [Header("Neural Networks")]
    public NeuralNetwork mainNet;
    public NeuralNetwork targetNet;
    [Space(10)]

    [Header("Reinforcement Learning")]
    public Environment env;
    public Agent agent;
    [Space(10)]

    [Header("Activation Functions")]
    public string hiddenActivation = "relu";
    public string outputActivation = "softmax";

    [Header("Hyperparameters")]
    public int[] layers; // TODO: Make this available to change in inspector
    public int episodeMax;
    public int framesPerState; // The number of frames per state
    public int frameBufferSize; // The size of the frame buffer (Calculated when environment is initialized)
    public int epiMaxSteps; // Steps per episode
    public float epsilon; // Used in GetAction function, Epsilon is basically the chance for a random action, Epsilon gradually reduces until it reaches epsilon_min
    public float epsilonMin; // epsilon_min is the lowest value for epsilon, i.e. 0.1 means there is a 10% chance for a random action   
    public float epsChangeFactor; // Used to decay epsilon
    public int expBufferSize; // The maximum size of the buffer (Can be viewed as the agent's memory)
    public int miniBatchSize; // Size of the mini-batch used to train the agent
    public int netCopyRate;
    // Research the following settings
    public float gamma; // TODO: What should gamma be set to?   
    public double learningRate;
    public float beta1;
    public float beta2;
    public double epsilonHat; // I have seen set between 10^-8 and 10^-5 (AKA 1e-8 and 1e-5), also 1 or 0.1 have been suggested
    public double gradientThreshold;
    [Space(10)]

    [Header("Counters")]
    public int epochs;
    public int episodeNum;
    public int epiSteps;
    public int targetSteps;
    public int mainSteps;
    [Space(10)]

    [Header("Reward")]
    public float episodeReward;
    [Space(10)]

    [Header("State Booleans")]
    public bool isDone; // Done flag used to determine when the game is over
    public bool isTraining; // Toggle train function
    public bool isConverged;
    [Space(10)]

    [Header("Calculated Variables")]
    public int actionQty; // The number of actions the agent can perform
    public int layerQty; // Number of layers
    public int stateSize; // Number of frames in a state
    public int frameSize; // The number of inputs that comprise each frame
    public double currentLearningRate;
    public float epsilonChange; // This is the rate at which the value of epsilon will reduce each update
    [Space(10)]

    [Header("Environment Settings")]
    public float maxViewDistance;
    public float fieldOfView;
    public float collisionDetectRange;
    public UIManager ui;
    private void Start()
    {
        agent = new Agent();
        env = new Environment();
        InitQNets();
        agent.InitAgent(GetComponent<AIPawn>(), this);
        env.InitEnv(GetComponent<Transform>(), this);
        ui = GameManager.instance.ui;
        LoadSettings();
        // Initialize variables to starting values
        isDone = false; 
        isConverged = false;
        isTraining = true;
        episodeNum = 1;
        episodeReward = 0;
        epiSteps = 0;
        epsilon = 1.0f;
        epochs = 0;
        targetSteps = 0;
        mainSteps = 0;
        epsilonChange = (epsilon - epsilonMin) / epsChangeFactor;
        frameSize = layers[0] / framesPerState;
        stateSize = frameSize * framesPerState;
    }
    private void FixedUpdate()
    {
        RunGame();
    }
    public void LoadSettings()
    {
        episodeMax = int.Parse(ui.maxEpsisodeIpt.text);
        epiMaxSteps = int.Parse(ui.stepsEpsIpt.text);
        framesPerState = int.Parse(ui.frameStateIpt.text);
        frameBufferSize = int.Parse(ui.frameBufferIpt.text);
        epsilon = float.Parse(ui.epsilonIpt.text);
        epsilonMin = float.Parse(ui.epsMinIpt.text);
        epsilonChange = float.Parse(ui.epsChangeIpt.text);
        expBufferSize = int.Parse(ui.expBufferSizeIpt.text);
        miniBatchSize = int.Parse(ui.miniBatchInpt.text);
        netCopyRate = int.Parse(ui.netCopyRateIpt.text);
        gamma = float.Parse(ui.gammaIpt.text);
        learningRate = double.Parse(ui.learningRateIpt.text);
        beta1 = float.Parse(ui.beta1Ipt.text);
        beta2 = float.Parse(ui.beta2Ipt.text);
        epsilonHat = double.Parse(ui.epsHatIpt.text);
        gradientThreshold = double.Parse(ui.gradientThreshIpt.text);
        maxViewDistance = float.Parse(ui.maxViewIpt.text);
        fieldOfView = float.Parse(ui.fovIpt.text);
        collisionDetectRange = float.Parse(ui.colDetectIpt.text);
        Debug.Log("Settings Loaded");
    }
    public void InitQNets()
    {
        layers = new int[] { 36, 36, 36, 7 };
        actionQty = layers[layers.Length - 1];
        layerQty = layers.Length;

        mainNet = new NeuralNetwork(layers);
        mainNet.Mutate();

        targetNet= mainNet;
    }
    public void RunGame()
    {
        if (episodeNum < episodeMax)
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
    public void RunEpisode(Agent agent, Environment env)
    {    
        if (!isDone)
        {
            // Copy weights from main to target network periodically
            if (epochs % netCopyRate == 0)
            {
                targetNet = mainNet;
            }

            // Get state from frame buffer
            float[] currentState = env.GetState(env.frameBuffer, env.fbIndex);

            // Perform the action
            double[] currentAction = agent.PerformAction(currentState, epsilon, actionQty);

            epochs++; // Increment total steps
            epiSteps++; // Increment steps in episode

            if (epiSteps >= epiMaxSteps)
            {
                episodeNum++;
                isDone = true;
                epiSteps = 0;     
            }

            // Creates next frame
            float[] nextFrame = env.GetNextFrame();

            // Add frame to frame buffer
            int lastFrameIndex = env.UpdateFrameBuffer(nextFrame);

            // Calculates the reward based on the state
            float currentReward = env.CalculateReward(nextFrame);

            // Add current reward to the episode total
            episodeReward += currentReward;

            // Update experience replay memory
            agent.ExperienceReplay(lastFrameIndex, currentAction, currentReward, isDone);

            if (isTraining == true && isConverged == false)
            {
                // Train the agent
                isConverged = mainNet.Train(agent.experienceBuffer, mainNet.weightsMatrix, mainNet.neuronsSums, this, env, agent);
            }

            // Recalculate epsilon
            epsilon = Mathf.Max(epsilon - epsilonChange, epsilonMin);
        }
    }
}
