using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System;

public class DQN : MonoBehaviour
{
    [Header("Identifying Info")]
    public string agentName;
    public int agentID;
    [Space(10)]

    [Header("Neural Networks")]
    public NeuralNetwork mainNet;
    public NeuralNetwork targetNet;
    [Space(10)]

    [Header("Reinforcement Learning")]
    public Environment env;
    public Agent agent;
    [Space(10)]

    [Header("Activation Functions")]
    public string hiddenActivation;
    public string outputActivation;

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
    [Space(10)]

    [Header("Reward")]
    public float episodeReward;
    public float totalReward;
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
    [Space(10)]

    [Header("Other")]
    public int autoSaveEpisode;

    private UIManager ui;
    private Transform tf;
    private Material mat;
    private Color red;
    private Color white;

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
        totalReward = 0;
        epiSteps = 0;
        epochs = 0;
        epsilonChange = (epsilon - epsilonMin) / epsChangeFactor;
        frameSize = layers[0] / framesPerState;
        stateSize = frameSize * framesPerState;
        tf = GetComponent<Transform>();
        mat = GetComponent<MeshRenderer>().material;
        red = new Color(255, 0, 0); // Brighten when close to objective
        white = new Color(255, 255, 255); // Brighten when close to objective
    }
    private void FixedUpdate()
    {
        RunGame();
    }
    private void Update()
    {
        if (agent.isExploit)
        {
            mat.color = red;
        }
        else
        {
            mat.color = white;
        }
    }
    public void LoadSettings()
    {
        episodeMax = GameManager.instance.settings.episodeMax;
        epiMaxSteps = GameManager.instance.settings.epiMaxSteps;
        framesPerState = GameManager.instance.settings.framesPerState;
        frameBufferSize = GameManager.instance.settings.frameBufferSize;
        epsilon = GameManager.instance.settings.epsilon;
        epsilonMin = GameManager.instance.settings.epsilonMin;
        epsChangeFactor = GameManager.instance.settings.epsChangeFactor;
        expBufferSize = GameManager.instance.settings.expBufferSize;
        miniBatchSize = GameManager.instance.settings.miniBatchSize;
        netCopyRate = GameManager.instance.settings.netCopyRate;
        gamma = GameManager.instance.settings.gamma;
        learningRate = GameManager.instance.settings.learningRate;
        beta1 = GameManager.instance.settings.beta1;
        beta2 = GameManager.instance.settings.beta2;
        epsilonHat = GameManager.instance.settings.epsilonHat;
        gradientThreshold = GameManager.instance.settings.gradientThreshold;
        maxViewDistance = GameManager.instance.settings.maxViewDistance;
        fieldOfView = GameManager.instance.settings.fieldOfView;
        collisionDetectRange = GameManager.instance.settings.collisionDetectRange;

        agentID = GameManager.instance.settings.agentID;
        agentName = GameManager.instance.settings.agentName;
        autoSaveEpisode = GameManager.instance.settings.autoSaveEpisode;
    }
    public void InitQNets()
    {
        layers = new int[] { 36, 36, 36, 7 };
        actionQty = layers[layers.Length - 1];
        layerQty = layers.Length;

        mainNet = new NeuralNetwork(layers);
        mainNet.Mutate();

        targetNet = mainNet;
    }
    public void RunGame()
    {
        if (episodeNum < episodeMax)
        {
            isDone = false;
        }
        if (episodeNum % autoSaveEpisode == 0) // Autosave feature
        {
            string fileName = agentName + "_AS_e" + episodeNum + ".gd";
            string settingsName = agentName + "_AS_e" + episodeNum + "_settings.gd";
            SaveLoad.SaveNet(fileName, this);
            SaveLoad.SaveSettings(settingsName);
            Debug.Log(fileName);
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
                episodeReward = 0;
                tf.position = GameManager.instance.spawnpoint.position;
            }

            // Creates next frame
            float[] nextFrame = env.GetNextFrame();

            // Add frame to frame buffer
            int lastFrameIndex = env.UpdateFrameBuffer(nextFrame);

            // Calculates the reward based on the state
            float currentReward = env.CalculateReward(nextFrame);

            // Add current reward to the episode and total reward
            episodeReward += currentReward;
            totalReward += currentReward;

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
