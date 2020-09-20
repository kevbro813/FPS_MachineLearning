using UnityEngine;
using System;

[Serializable]
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

    [Header("Counters")]
    public int epochs;
    public int epiSteps;
    public int episodeNum;
    [Space(10)]

    [Header("Reward")]
    public double episodeReward;
    public double totalReward;
    [Space(10)]

    [Header("State Booleans")]
    public bool isDone; // Done flag used to determine when the game is over
    public bool isTraining; // Toggle train function
    public bool isConverged;
    private bool isSaved; // Prevents saving every frame of a save episode
    [Space(10)]

    [Header("Calculated Variables")]
    public int actionQty; // The number of actions the agent can perform
    public int layerQty; // Number of layers
    public int inputsPerFrame; // The number of inputs that comprise each frame
    public int inputsPerState; // Number of frames in a state
    public double currentLearningRate;
    public float epsilonChange; // This is the rate at which the value of epsilon will reduce each update
    [Space(10)]

    public UIManager ui;
    public Transform tf;
    public Material mat;
    public Color red;
    public Color white;

    public double cost;

    private void Start()
    {
        agent = new Agent();
        env = new Environment();
        InitQNets();
        agent.Init_Agent(GetComponent<AIPawn>(), this);
        env.Init_Env(GetComponent<Transform>(), this);
        ui = GameManager.instance.ui;
        // Initialize variables to starting values
        isDone = false;
        isConverged = false;
        isTraining = true;
        episodeNum = 1;
        episodeReward = 0;
        totalReward = 0;
        epiSteps = 0;
        epochs = 0;
        epsilonChange = (GameManager.instance.settings.epsilon - GameManager.instance.settings.epsilonMin) / GameManager.instance.settings.epsChangeFactor;
        inputsPerFrame = GameManager.instance.settings.layers[0] / GameManager.instance.settings.framesPerState;
        inputsPerState = inputsPerFrame * GameManager.instance.settings.framesPerState;
        tf = GetComponent<Transform>();
        mat = GetComponent<MeshRenderer>().material;
        red = new Color(255, 0, 0); // Brighten when close to objective
        white = new Color(255, 255, 255); // Brighten when close to objective
    }
    private void Update()
    {
        RunGame();

        if (agent.isExploit)
        {
            mat.color = red;
        }
        else
        {
            mat.color = white;
        }
    }
    public void InitQNets()
    {
        GameManager.instance.settings.layers = new int[] { 8, 64, 64, 5 };
        actionQty = GameManager.instance.settings.layers[GameManager.instance.settings.layers.Length - 1];
        layerQty = GameManager.instance.settings.layers.Length;

        mainNet = new NeuralNetwork(GameManager.instance.settings.layers);
        CopyNetwork();
    }
    public void RunGame()
    {
        if (episodeNum < GameManager.instance.settings.episodeMax)
            isDone = false;

        if (episodeNum % GameManager.instance.settings.autoSaveEpisode == 0 && !isSaved) // Autosave feature
        {
            string fileName = GameManager.instance.settings.agentName + "_as_e" + episodeNum + ".gd";
            string settingsName = GameManager.instance.settings.agentName + "_as_e" + episodeNum + "_settings.gd";
            SaveLoad.SaveNet(fileName, this);
            SaveLoad.SaveSettings(settingsName);
            isSaved = true;
        }

        if (episodeNum == GameManager.instance.settings.episodeMax)
            Debug.Log("Game Over.");
        else
            RunEpisode(agent, env); // Run the RunEpisode method passing in the agent and environment and returning the score (reward) for the episode.
    }
    // Run one episode
    public void RunEpisode(Agent agent, Environment env)
    {
        if (!isDone)
        {
            // Copy weights from main to target network periodically
            if (epochs % GameManager.instance.settings.netCopyRate == 0)
                CopyNetwork();

            // Get state from frame buffer (the current state is actually fbIndex - 1 since fbIndex updates before this runs
            double[] currentState = env.GetState(env.fbIndex - 1);

            // Perform the action
            int currentAction = agent.PerformAction(currentState, GameManager.instance.settings.epsilon, actionQty);

            epochs++; // Increment total steps
            epiSteps++; // Increment steps in episode

            NewEpisodeCheck(); // Check if the episode is over and start a new one

            // Creates next frame
            double[] nextFrame = env.GetNextFrame();

            // Add frame to frame buffer
            int lastFrameIndex = env.UpdateFrameBuffer(nextFrame);

            env.UpdateFrameBufferCounters(); // Updates fbIndex and fbCount variables

            // Calculates the reward based on the state
            double currentReward = env.CalculateReward(nextFrame);

            // Add current reward to the episode and total reward
            episodeReward += currentReward;
            totalReward += currentReward;
            //Debug.Log(isDone);
            // Update experience replay memory
            agent.ExperienceReplay(lastFrameIndex, currentAction, currentReward, isDone);

            //CalculateCost();

            agent.UpdateExperienceBufferCounters();

            if (isTraining == true && isConverged == false) // Train the agent
            {
                if (epiSteps % 4 == 0) // Run training every 4* steps
                    isConverged = mainNet.Train(this, env, agent);
            }

            // Recalculate epsilon
            GameManager.instance.settings.epsilon = Mathf.Max(GameManager.instance.settings.epsilon - epsilonChange, GameManager.instance.settings.epsilonMin);
        }
    }
    public void NewEpisodeCheck()
    {
        if (epiSteps >= GameManager.instance.settings.epiMaxSteps)
        {
            episodeNum++;
            isDone = true;
            isSaved = false;
            epiSteps = 0;
            episodeReward = 0;
            GameManager.instance.RandomSpawn();
            tf.position = GameManager.instance.spawnpoint.position;
            foreach (Layer lay in mainNet.layers)
            {
                //lay.t = 0; // Resetting t to zero will reset the learning rate each episode
            }
        }
    }
    public void CalculateCost()
    {
        Tuple<int, int, double, bool> miniBatch = agent.experienceBuffer[agent.bufferIndex]; // Get most recent tuple
        double[] nextState; // Next State (The resulting state after an action)
        int action; // The action performed
        double reward; // Reward for the action
        bool done; // Boolean to indicate if the current mini batch is done (This is true on the last frame of an episode to prevent it from being used for training)

        if (miniBatch != null)
        {
            nextState = env.GetState(miniBatch.Item1); // Next state ends with the last frame
            action = miniBatch.Item2;
            reward = miniBatch.Item3;
            done = miniBatch.Item4;

            //double[] targets = mainNet.CalculateTargets(nextState, reward, done, this);

            //cost = mainNet.Cost(action, targets, actionQty);
        }
    }
    public void CopyNetwork()
    {
        targetNet = mainNet;
    }
}
