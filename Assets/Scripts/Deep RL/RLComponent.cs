using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class RLComponent : MonoBehaviour
{
    [Header("Neural Networks")]
    public NeuralNetwork mainNet;
    public NeuralNetwork targetNet;
    public int[] layers;
    [Space(10)]

    [Header("Reinforcement Learning")]
    public DoubleDQN doubleDQN;
    public Environment env;
    public Agent agent;
    [Space(10)]

    [Header("Counters")]
    public int epochs;
    public int epiSteps;
    public int episodeNum;
    public int framesFirstState;
    [Space(10)]

    [Header("Reward")]
    public double episodeReward;
    public double totalReward;
    [Space(10)]

    [Header("State Booleans")]
    public bool isDone; // Done flag used to determine when the game is over
    public bool isTraining; // Toggle train function
    public bool isNetworkNew;
    public bool isStateReady;
    public bool isAgentActive;
    private bool isSaved; // Prevents saving every frame of a save episode
    [Space(10)]

    [Header("Calculated Variables")]
    public int actionQty; // The number of actions the agent can perform
    public int layerQty; // Number of layers
    public int inputsPerFrame; // The number of inputs that comprise each frame
    public int inputsPerState; // Number of frames in a state
    public double currentLearningRate;
    public float epsilonDecay; // This is the rate at which the value of epsilon will reduce each update
    public double cost;
    [Space(10)] 

    [Header("Other Variables")]
    [SerializeField] private float epsilon;
    [SerializeField] private float epsilonMin;
    [SerializeField] private float epsDecayRate;
    [SerializeField] private int framesPerState;
    [SerializeField] private int netCopyRate;
    [SerializeField] private int episodeMaxSteps;
    [SerializeField] private int episodeMax;
    [SerializeField] private int autoSaveEpisode;
    [SerializeField] private string agentName;

    [Space(10)]
    private Transform tf;
    private Material mat;
    private Color red;
    private Color white;

    private void Start()
    {
        tf = GetComponent<Transform>();
        mat = GetComponent<MeshRenderer>().material;
        red = new Color(255, 0, 0); 
        white = new Color(255, 255, 255);
    }
    private void Update()
    {
        if (isAgentActive)
        {
            RunSession();

            if (agent.isExploit)
            {
                mat.color = red;
            }
            else
            {
                mat.color = white;
            }
        }
    }
    /// <summary>
    /// Initialize new reinforcement learning training.
    /// </summary>
    /// <param name="isNN"></param>
    public void Init_New_Session(bool isNeuralNetworkNew, bool isTrainingSession)
    {
        isNetworkNew = isNeuralNetworkNew;
        isTraining = isTrainingSession;
        Init_Settings(); // Initialize settings from those stored in GameManager.settings (settings stored in game manager to make it easier to save)
        Init_Neural_Nets(); // Initialize main and target neural networks (used with doubleDQN and *duelingDQN)
        Init_Reinforcement_Learning(); // Initialize reinforcement learning components, i.e. agent, environment and DQN (or doubleDQN)
        Init_Training_Variables(); // Initialize variables as starting values
    }
    /// <summary>
    /// Initialize variables to starting values.
    /// </summary>
    private void Init_Training_Variables()
    {
        isDone = false;
        isStateReady = false;
        framesFirstState = 0;
        episodeReward = 0;
        totalReward = 0;
        epiSteps = 0;
        epochs = 0;
        episodeNum = 1;
        isAgentActive = true;
    }
    /// <summary>
    /// Save variable settings locally.
    /// </summary>
    private void Init_Settings()
    {
        epsilon = RLManager.instance.settings.epsilon;
        epsilonMin = RLManager.instance.settings.epsilonMin;
        epsDecayRate = RLManager.instance.settings.epsDecayRate;
        framesPerState = RLManager.instance.settings.framesPerState;
        netCopyRate = RLManager.instance.settings.netCopyRate;
        episodeMaxSteps = RLManager.instance.settings.epiMaxSteps;
        episodeMax = RLManager.instance.settings.episodeMax;
        autoSaveEpisode = RLManager.instance.settings.autoSaveEpisode;
        agentName = RLManager.instance.settings.agentName;
        epsilonDecay = (epsilon - epsilonMin) / epsDecayRate;
    }
    /// <summary>
    /// 
    /// </summary>
    private void Init_Reinforcement_Learning()
    {
        doubleDQN = new DoubleDQN();
        agent = new Agent();
        env = new Environment();
        doubleDQN.Init_Double_DQN(actionQty, env, agent, mainNet, targetNet);
        agent.Init_Agent(GetComponent<AIPawn>(), this);
        env.Init_Env(GetComponent<Transform>(), this);
    }
    /// <summary>
    /// Initialize neural networks. Creates a mainNet with random weights and biases, then copies and sets the targetNet with the same weights and biases.
    /// </summary>
    private void Init_Neural_Nets()
    {
        layers = new int[] { 3, 20, 20, 3 };
        actionQty = layers[layers.Length - 1];
        layerQty = layers.Length;
        inputsPerState = layers[0];
        inputsPerFrame = inputsPerState / framesPerState;

        if (isNetworkNew)
        {
            mainNet = new NeuralNetwork(layers);
        }

        targetNet = new NeuralNetwork(layers);
        CopyNetwork();
    }
    /// <summary>
    /// Runs a full training session
    /// </summary>
    private void RunSession()
    {
        if (episodeNum < episodeMax)
            isDone = false;

        AutoSave(); // Periodically auto saves

        if (episodeNum == episodeMax)
            Debug.Log("Game Over.");
        else
            RunEpisode(); // Run the RunEpisode method passing in the agent and environment and returning the score (reward) for the episode.
    }

    /// <summary>
    /// Runs a single episode.
    /// </summary>
    /// <param name="agent"></param>
    /// <param name="env"></param>
    private void RunEpisode()
    {
        if (!isDone)
        {
            Step(); // Increment one step and check for new episode and update target network periodically

            // Get state from frame buffer (the current state is actually fbIndex - 1 since fbIndex updates before this runs
            double[] currentState = env.GetState(env.fbIndex - 1);

            //Debug.Log(currentState[0] + "   " + currentState[1] + "   " + currentState[2]);

            int currentAction = agent.PerformAction(currentState, epsilon, actionQty); // Perform the action

            double currentReward = Reward(); // Calculate reward for Q(s, a)

            double[] nextFrame = env.GetNextFrame(); // Creates next frame

            int lastFrameIndex = env.AppendFrame(nextFrame); // Add frame to frame buffer

            env.UpdateFrameBufferCounters(); // Updates fbIndex and fbCount variables

            // Store state, next state, action, reward and done in a tuple (state and next state can be referenced using lastFrameIndex)
            ExperienceReplay(lastFrameIndex, currentAction, currentReward, isDone);

            Training(); // Train the neural network

            Epsilon(); // Epsilon update calculation
        }
    }

    private void Epsilon()
    {
        epsilon = Mathf.Max(epsilon - epsilonDecay, epsilonMin);
        RLManager.instance.settings.epsilon = epsilon;
    }
    private void Training()
    {
        if (isTraining == true) // Train the agent
        {
            if (epochs % 4 == 0) // Run training every 4* steps
                cost = doubleDQN.Train();
        }
    }
    private void Step()
    {
        NewEpisodeCheck(); // Check if the episode is over and start a new one

        epochs++; // Increment total steps
        epiSteps++; // Increment steps in episode

        // Copy weights from main to target network periodically
        if (epochs % netCopyRate == 0)
            CopyNetwork();

        // Update HUD display
        if (!GameManager.instance.adminMenu.activeSelf)
        {
            GameManager.instance.ui.UpdateHUD();
        }
    }
    private double Reward()
    {
        // Calculates the reward based on the state
        double reward = env.CalculateReward();

        // Add current reward to the episode and total reward
        episodeReward += reward;
        totalReward += reward;

        return reward;
    }
    private void ExperienceReplay(int index, int action, double reward, bool done)
    {
        if (!isStateReady)
        {
            framesFirstState++;
            if (framesFirstState >= framesPerState)
            {
                isStateReady = true;
            }
        }
        else
        {
            // Update experience replay memory
            agent.ExperienceReplay(index, action, reward, done);
            agent.UpdateExperienceBufferCounters(); // Update bufferIndex and bufferCount variables
        }
    }
    /// <summary>
    /// Check if the episode has elapsed and begin new episode if true.
    /// </summary>
    private void NewEpisodeCheck()
    {
        if (epiSteps >= episodeMaxSteps)
        {
            episodeNum++;
            isDone = true;
            isSaved = false;
            epiSteps = 0;
            isStateReady = false;
            framesFirstState = 0;
            episodeReward = 0;
            RLManager.instance.RandomSpawn();
            tf.position = RLManager.instance.spawnpoint.position;
            foreach (Layer lay in mainNet.layers)
            {
                //lay.t = 0; // Resetting t to zero will reset the learning rate each episode
            }
        }
    }
    /// <summary>
    /// Copy weights from main network to target network.
    /// </summary>
    private void CopyNetwork()
    {
        for (int i = 0; i < mainNet.layers.Length; i++)
        {
            for (int j = 0; j < targetNet.layers[i].weights.Length; j++)
            {
                for (int k = 0; k < targetNet.layers[i].weights[j].Length; k++)
                {
                    targetNet.layers[i].weights[j][k] = mainNet.layers[i].weights[j][k];
                    targetNet.layers[i].biases[j] = mainNet.layers[i].biases[j];
                }
            }
        }
    }
    /// <summary>
    /// Autosaves the main network every "x" episodes.
    /// </summary>
    private void AutoSave()
    {
        if (episodeNum % autoSaveEpisode == 0 && !isSaved)
        {
            string fileName = agentName + "_as_e" + episodeNum + ".gd";
            string settingsName = agentName + "_as_e" + episodeNum + "_settings.gd";
            SaveLoad.SaveNet(fileName, this);
            SaveLoad.SaveSettings(settingsName);
            isSaved = true;
        }
    }
}
