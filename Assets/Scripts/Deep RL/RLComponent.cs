using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Transactions;
using System.Reflection;

[Serializable]
public class RLComponent : MonoBehaviour
{
    #region Variables
    [Header("Neural Networks")]
    public NeuralNetwork mainNet;
    public NeuralNetwork targetNet;
    public NeuralNetwork actorNet;
    public NeuralNetwork criticNet;
    public int[] dqnLayers; // Layer info for DQN network (Neurons per layer)
    public int[] actorLayers; // Layer info for actor network (PPO)
    public int[] criticLayers; // Layer info for critic network (PPO)
    [Space(10)]

    [Header("Reinforcement Learning")]
    public Settings.Algorithm algo; // The RL algorithm being used
    public DoubleDQN doubleDQN; // Double DQN algorithm
    public PPO ppo; // Proximal Policy Optimization algorithm
    public Environment env; // Environment (Frames, States and Rewards)
    public Agent agent; // Agent (Actions, Experience Replay)

    [Space(10)]

    [Header("Counters")]
    public int epochs; // Tracks total epochs (aka steps)
    public int epiSteps; // Steps per episode
    public int episodeNum; // Episode number
    public int framesFirstState; // Tracks the number of frames when an episode begins to prevent incomplete states
    [Space(10)]

    [Header("Reward")]
    public double episodeReward;
    public double totalReward;
    [Space(10)]

    [Header("State Booleans")]
    public bool isDone; // Done flag used to determine when the game is over
    public bool isTraining; // Toggle train function
    public bool isNetworkNew; // Determines if the network is new, this is set to false when loading a network from file
    public bool isStateReady; // Prevents training before enough frames are collected to create a state
    public bool isAgentActive; // Indicates if an agent is active
    private bool isSaved; // Prevents saving every frame of a save episode
    [Space(10)]

    [Header("Calculated Variables")]
    public int actionQty; // The number of actions the agent can perform
    public int layerQty; // Number of layers
    public int inputsPerFrame; // The number of inputs that comprise each frame
    public int inputsPerState; // Number of frames in a state
    public float epsilonDecay; // This is the rate at which the value of epsilon will reduce each update
    public double cost; // Cost to be displayed
    [Space(10)] 

    [Header("Other Variables")]
    [SerializeField] private float epsilon; // Tracks the probability of a random action in epsilon greedy
    [SerializeField] private float epsilonMin; // Minimum for epsilon
    [SerializeField] private float epsDecayRate; // Rate that epsilon will decay to minimum. Represents the number of epochs
    [SerializeField] private int framesPerState; // Frames per state
    [SerializeField] private int netCopyRate; // The target network will copy weights from the main network at this interval
    [SerializeField] private int episodeMaxSteps; // Maximum number of steps in an episode
    [SerializeField] private int episodeMax; // Maximum number of episodes
    [SerializeField] private int autoSaveEpisode; // The network will autosave at this interval
    [SerializeField] private string agentName; // The name of the agent
    private string saveLocation; // The location where files will be saved

    [Space(10)]
    private Transform tf;
    private Material mat;
    private Color red;
    private Color white;

    [Space(10)]
    [Header("Test Specific")]
    private Turret turret;


    #endregion

    #region Start/Update Methods
    private void Start()
    {
        tf = GetComponent<Transform>();
        mat = GetComponent<MeshRenderer>().material;
        red = new Color(255, 0, 0); 
        white = new Color(255, 255, 255);
        turret = GameObject.FindWithTag("Turret").GetComponent<Turret>();
    }
    private void Update()
    {
        if (isAgentActive && !RLManager.instance.isSessionPaused) // Check that the agent is active and the game is not paused
        {
            RunSession();

            // Change the agent's color depending on explore vs exploit
            if (agent.isExploit)
                mat.color = red;
            else
                mat.color = white;
        }
    }
    #endregion

    #region Double DQN Initialization
    /// <summary>
    /// Initialize new Double DQN Session
    /// </summary>
    /// <param name="isNN"></param>
    public void Init_New_DDQN_Session(bool isNeuralNetworkNew, bool isTrainingSession, int[] dqnNetStruct)
    {
        isNetworkNew = isNeuralNetworkNew; // Is this a new neural network or one being loaded
        isTraining = isTrainingSession; // Is training activate? (If training is off, the agent will still operate but will not improve)
        algo = Settings.Algorithm.Double_DQN; // Use Double DQN
        Init_Settings(); // Initialize settings from those stored in GameManager.settings (settings stored in game manager to make it easier to save)
        Init_DoubleDQN_Nets(dqnNetStruct); // Initialize main and target neural networks (used with doubleDQN and *duelingDQN)
        Init_DoubleDQN_Learning(); // Initialize reinforcement learning components, i.e. agent, environment and DQN (or doubleDQN)
        Init_Training_Variables(); // Initialize variables as starting values
    }
    /// <summary>
    /// Initialize Double DQN Neural networks. Creates a mainNet with random weights and biases, then copies and sets the targetNet with the same weights and biases.
    /// </summary>
    private void Init_DoubleDQN_Nets(int[] netStructure)
    {
        // Set neural network structure and associated variables
        dqnLayers = netStructure;
        actionQty = dqnLayers[dqnLayers.Length - 1];
        layerQty = dqnLayers.Length;
        inputsPerState = dqnLayers[0];
        inputsPerFrame = inputsPerState / framesPerState;

        if (isNetworkNew) // Network is new
        {
            mainNet = new NeuralNetwork(dqnLayers); // Initialize a new main network
        }

        targetNet = new NeuralNetwork(dqnLayers); // Initialize a new target network
        CopyNetwork(); // Set target network to main network values
    }
    /// <summary>
    /// Initialize agent, environment and Double DQN classes for DDQN learning.
    /// </summary>
    private void Init_DoubleDQN_Learning()
    {
        agent = new Agent();
        agent.Init_Agent(GetComponent<AIPawn>(), this, actionQty); // Initialize agent variables

        env = new Environment();
        env.Init_Env(GetComponent<Transform>(), this); // Initialize environment variables

        doubleDQN = new DoubleDQN();
        doubleDQN.Init_Double_DQN(actionQty, env, agent, mainNet, targetNet); // Initialize DDQN variables
    }
    #endregion

    #region Proximal Policy Optimization Initialization
    /// <summary>
    /// Initialize new PPO session
    /// </summary>
    /// <param name="isNeuralNetworkNew"></param>
    /// <param name="isTrainingSession"></param>
    /// <param name="actorNetStruct"></param>
    /// <param name="criticNetStruct"></param>
    public void Init_New_PPO_Session(bool isNeuralNetworkNew, bool isTrainingSession, int[] actorNetStruct, int[] criticNetStruct)
    {
        isNetworkNew = isNeuralNetworkNew; // Is this a new neural network or one being loaded
        isTraining = isTrainingSession; // Is training activate? (If training is off, the agent will still operate but will not improve)
        algo = Settings.Algorithm.Proximal_Policy_Optimization; // Use PPO
        Init_Settings(); // Initialize settings from those stored in GameManager.settings (settings stored in game manager to make it easier to save)
        Init_PPO_Nets(actorNetStruct, criticNetStruct); // Initialize main and target neural networks (used with doubleDQN and *duelingDQN)
        Init_PPO_Learning(); // Initialize reinforcement learning components, i.e. agent, environment and DQN (or doubleDQN)
        Init_Training_Variables(); // Initialize variables as starting values

    }
    /// <summary>
    /// Initialize PPO Neural networks. Create two unique networks for actor and critic.
    /// </summary>
    /// <param name="actorNetStructure"></param>
    /// <param name="criticNetStructure"></param>
    private void Init_PPO_Nets(int[] actorNetStructure, int[] criticNetStructure)
    {
        // Set neural network structure and associated variables
        actorLayers = actorNetStructure;
        criticLayers = criticNetStructure; // output must be "1"
        actionQty = actorLayers[actorLayers.Length - 1];
        layerQty = actorLayers.Length;
        inputsPerState = actorLayers[0];
        inputsPerFrame = inputsPerState / framesPerState;

        if (isNetworkNew) // Network is new, if network is loaded then there is no need to initialize as new.
        {
            actorNet = new NeuralNetwork(actorLayers, true);
            criticNet = new NeuralNetwork(criticLayers, false);
        }
    }
    /// <summary>
    /// Initialize agent, environment and PPO classes for PPO learning.
    /// </summary>
    private void Init_PPO_Learning()
    {
        agent = new Agent();
        agent.Init_Agent(GetComponent<AIPawn>(), this, actionQty); // Initialize agent variables

        env = new Environment();
        env.Init_Env(GetComponent<Transform>(), this); // Initialize environment variables

        ppo = new PPO();
        ppo.Init_PPO(actionQty, env, agent, actorNet, criticNet); // Initialize DDQN variables
    }
    #endregion

    #region Universal Initialization
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
        
        RLManager.instance.UpdateObjectiveLocation(); // Set an initial location for the objective
        RLManager.instance.UpdateTurretLocation();
    }
    /// <summary>
    /// Save variable settings locally.
    /// </summary>
    public void Init_Settings()
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
        saveLocation = RLManager.instance.settings.saveLocation;

        if (algo == Settings.Algorithm.Proximal_Policy_Optimization) // These settings are PPO specific
        {
            RLManager.instance.settings.frameBufferSize = episodeMaxSteps; // Frame buffer should be the same size as the number of steps in the episode
            RLManager.instance.settings.expBufferSize = episodeMaxSteps - framesPerState + 1; // Experience buffer does not start until there are enough frames, thus subtracting framesPerState and adding 1
        }
    }
    #endregion

    #region Universal Session/Episode Loops
    /// <summary>
    /// Runs a full training session
    /// </summary>
    private void RunSession()
    {
        AutoSave(); // Periodically auto saves

        if (episodeNum == episodeMax) // Check if the game is over
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
        if (!isDone) // If the training session is not done and we are not at the episode max
        {
            // Run the current algorithm
            if (algo == Settings.Algorithm.Double_DQN)
                RunDoubleDQN();
            else if (algo == Settings.Algorithm.Proximal_Policy_Optimization)
                RunPPO();
        }
        // If done and PPO
        else if (isDone && algo == Settings.Algorithm.Proximal_Policy_Optimization)
        {
            cost = ppo.PPOTraining(); // Run training after each episode
            isDone = false; // Reset the done flag to false
        }
        // If done and Double DQN
        else if (isDone && algo == Settings.Algorithm.Double_DQN)
        {
            isDone = false; // Reset the done flag to false
        }
    }
    #endregion

    #region Double DQN Main Loop
    /// <summary>
    /// Runs one epoch/step of Double DQN algorithm.
    /// </summary>
    private void RunDoubleDQN()
    {
        DDQNStep(); // Increment one step and check for new episode and update target network periodically

        // Get state from frame buffer (the current state is actually fbIndex - 1 since fbIndex updates before this runs
        double[] currentState = env.GetState(env.fbIndex - 1);

        int currentAction = agent.DQNAction(currentState, epsilon); // Perform the action

        double currentReward = Reward(); // Calculate reward for Q(s, a)

        double[] nextFrame = env.GetNextFrame(epiSteps); // Creates next frame

        int lastFrameIndex = env.AppendFrame(nextFrame); // Add frame to frame buffer

        env.UpdateFrameBufferCounters(); // Updates fbIndex and fbCount variables

        // Store state, next state, action, reward and done in a tuple (state and next state can be referenced using lastFrameIndex)
        ExperienceReplay(lastFrameIndex, currentAction, currentReward, isDone);

        DDQNTraining(); // Train the neural network

        Epsilon(); // Epsilon update calculation

        NewEpisodeCheck(); // Check if episode is finished
    }
    /// <summary>
    /// Double DQN - Updates counters and HUD and checks for done flag each step.
    /// </summary>
    private void DDQNStep()
    {
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
        DoneCheck();
    }
    /// <summary>
    /// Double DQN training runs every "X" episodes
    /// </summary>
    private void DDQNTraining()
    {
        if (isTraining == true) // Train the agent
        {
            if (epochs % 4 == 0) // Run training every 4 steps TODO: Add to settings
                cost = doubleDQN.Train();
        }
    }
    /// <summary>
    /// Runs experience replay for DDQN which stores data for training.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="action"></param>
    /// <param name="reward"></param>
    /// <param name="done"></param>
    private void ExperienceReplay(int index, int action, double reward, bool done)
    {
        // Check that a state is ready, AKA there are enough frames to create a state
        if (!isStateReady)
        {
            framesFirstState++;
            if (framesFirstState >= framesPerState)
            {
                isStateReady = true;
            }
        }
        if (isStateReady) // If the state is ready
        {
            // Update experience replay memory
            agent.ExperienceReplay(index, action, reward, done);
            agent.UpdateExperienceBufferCounters(); // Update bufferIndex and bufferCount variables
        }
    }
    #endregion

    #region PPO Main Loop
    /// <summary>
    /// Runs one epoch/step of the PPO algorithm. However, note that training happens after the episode since Advantage calculation requires iterating in reverse
    /// through each epoch of data.
    /// </summary>
    private void RunPPO()
    {
        // Increment Step
        PPOStep();

        // Add frame to frameBuffer
        double[] state = env.GetState(env.fbIndex - 1);

        // Get action probabilities from actor net
        double[] predictions = actorNet.FeedForward(state);

        // Calculate the state value using the critic net
        double value = criticNet.FeedForward(state)[0];
        
        int currentAction = agent.PPOAction(predictions); // Perform action and return one hot array

        double reward = Reward(); // Store reward

        // Store actions, rewards, predictions (actionProbs), values (value function) and dones as tuples
        PPOExperience(currentAction, reward, predictions, value, isDone); // TODO: Add one hot and old log probs to batch

        // Advance frame
        double[] nextFrame = env.GetNextFrame(epiSteps); // Creates next frame

        env.AppendFrame(nextFrame); // Add frame to frame buffer

        env.UpdateFrameBufferCounters(); // Updates fbIndex and fbCount variables

        NewEpisodeCheck(); // Check if episode is finished
    }

    /// <summary>
    /// PPO - Updates counters and HUD and checks for done flag each step.
    /// </summary>
    private void PPOStep()
    {
        epochs++; // Increment total steps
        epiSteps++; // Increment steps in episode

        // Update HUD display
        if (!GameManager.instance.adminMenu.activeSelf)
        {
            GameManager.instance.ui.UpdateHUD();
        }
        DoneCheck();
    }
    /// <summary>
    /// Run experience replay for PPO. This stores data for training that takes place at the end of the episode.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="reward"></param>
    /// <param name="prediction"></param>
    /// <param name="value"></param>
    /// <param name="oneHotAct"></param>
    /// <param name="oldLogProb"></param>
    /// <param name="done"></param>
    private void PPOExperience(int action, double reward, double[] prediction, double value, bool done)
    {
        // Check that a state is ready, AKA there are enough frames to create a state
        if (!isStateReady)
        {
            framesFirstState++;
            if (framesFirstState >= framesPerState)
            {
                isStateReady = true;
            }
        }
        if (isStateReady) // If the state is ready
        {
            // Update experience replay memory
            agent.PPOExperience(action, reward, prediction, value, done);
            agent.UpdateExperienceBufferCounters(); // Update bufferIndex and bufferCount variables
        }
    }
    #endregion

    #region General Methods
    /// <summary>
    /// Calculate the reward for the current epoch and update the episode reward and total reward. This returns the reward to be stored in experience buffer.
    /// </summary>
    /// <returns></returns>
    private double Reward()
    {
        // Calculates the reward based on the state
        double reward = env.CalculateReward();

        // Add current reward to the episode and total reward
        episodeReward += reward;
        totalReward += reward;

        return reward;
    }
    /// <summary>
    /// Calculates epsilon each epoch.
    /// </summary>
    private void Epsilon()
    {
        epsilon = Mathf.Max(epsilon - epsilonDecay, epsilonMin);
        RLManager.instance.settings.epsilon = epsilon;
    }
    /// <summary>
    /// Check if the episode is done and set the done flag to true.
    /// </summary>
    private void DoneCheck()
    {
        if (epiSteps >= episodeMaxSteps)
            isDone = true;
    }
    /// <summary>
    /// Check if the episode has elapsed and begin new episode if true.
    /// </summary>
    private void NewEpisodeCheck()
    {
        if (epiSteps >= episodeMaxSteps) // Check for a new episode
        {
            episodeNum++; // Increment episode number

            // Reset variables
            epiSteps = 0;
            framesFirstState = 0;
            episodeReward = 0;
            isSaved = false; // Prepares episode to be saved
            isStateReady = false;

            // Generate a random spawn
            RLManager.instance.RandomSpawn();

            // Random Objective
            RLManager.instance.UpdateObjectiveLocation();

            // Random Turret Location
            RLManager.instance.UpdateTurretLocation();

            tf.position = RLManager.instance.spawnpoint.position; // Set spawn position

            // Updates the average reward with the latest episode
            GameManager.instance.ui.UpdateEpisodeAverage(totalReward, episodeNum);

            if (algo == Settings.Algorithm.Proximal_Policy_Optimization)
            {
                // Clears out memory by saving new data over old
                agent.bufferIndex = 0;
                env.fbIndex = 0;
            }

            // Destroy projectile and reset timer
            ResetProjectiles();

            // The following can be used to reset learning rates each episode
            //foreach (Layer lay in mainNet.layers)
            //{
                //lay.t = 0; // Resetting t to zero will reset the learning rate each episode
            //}
        }
    }

    private void ResetProjectiles()
    {
        if (turret.projectile_tf) Destroy(turret.projectile_tf.gameObject);
        turret.timerStart = Time.time;
    }
    /// <summary>
    /// Copy weights from main network to target network.
    /// </summary>
    private void CopyNetwork()
    {
        // Iterate through each weight and bias and copy them
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
    /// Autosaves the main network and settings every "x" episodes.
    /// </summary>
    private void AutoSave()
    {
        if (episodeNum % autoSaveEpisode == 0 && !isSaved) // Check if the episode is an autosave episode and that the network is not already saved
        {
            string fileName = agentName + "_as_e" + episodeNum; // Indicate autosave
            string settingsName = agentName + "_as_e" + episodeNum + "_settings";

            // Save the network and settings
            SaveLoad.SaveNet(fileName, this, saveLocation);
            SaveLoad.SaveSettings(settingsName, saveLocation);

            isSaved = true; // Indicate the network and settings are saved
        }
    }
    #endregion
}
