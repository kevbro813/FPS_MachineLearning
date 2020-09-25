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
        GameManager.instance.settings.layers = new int[] { 6, 45, 10, 5 };
        actionQty = GameManager.instance.settings.layers[GameManager.instance.settings.layers.Length - 1];
        layerQty = GameManager.instance.settings.layers.Length;

        mainNet = new NeuralNetwork(GameManager.instance.settings.layers);
        targetNet = new NeuralNetwork(GameManager.instance.settings.layers);
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
            epochs++; // Increment total steps
            epiSteps++; // Increment steps in episode

            NewEpisodeCheck(); // Check if the episode is over and start a new one

            // Copy weights from main to target network periodically
            if (epochs % GameManager.instance.settings.netCopyRate == 0)
                CopyNetwork();

            // Get state from frame buffer (the current state is actually fbIndex - 1 since fbIndex updates before this runs
            double[] currentState = env.GetState(env.fbIndex - 1);

            // Perform the action
            int currentAction = agent.PerformAction(currentState, GameManager.instance.settings.epsilon, actionQty);

            // Creates next frame
            double[] nextFrame = env.GetNextFrame();

            // Add frame to frame buffer
            int lastFrameIndex = env.UpdateFrameBuffer(nextFrame);

            env.UpdateFrameBufferCounters(); // Updates fbIndex and fbCount variables

            // Calculates the reward based on the state
            double currentReward = env.CalculateReward();

            // Add current reward to the episode and total reward
            episodeReward += currentReward;
            totalReward += currentReward;

            // Update experience replay memory
            agent.ExperienceReplay(lastFrameIndex, currentAction, currentReward, isDone);

            agent.UpdateExperienceBufferCounters();

            if (isTraining == true && isConverged == false) // Train the agent
            {
                if (epochs % 4 == 0) // Run training every 4* steps
                    isConverged = Train(this, env, agent);
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
    public void CopyNetwork()
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
    public bool Train(DQN dqn, Environment env, Agent agent)
    {
        int miniBatchSize = GameManager.instance.settings.miniBatchSize; // Size of the mini batch used to train the target net

        // This Tuple array consists of one mini batch (random sample from experience replay buffer)
        Tuple<int, int, double, bool>[] miniBatch = agent.GetMiniBatch(miniBatchSize, env.fbIndex, GameManager.instance.settings.framesPerState);

        // Initialize arrays to hold batch data
        double[][] states = new double[miniBatchSize][];
        double[][] nextStates = new double[miniBatchSize][]; // Next State (The resulting state after an action)
        int[] actions = new int[miniBatchSize]; // The action performed
        double[] rewards = new double[miniBatchSize]; // Reward for the action
        bool[] dones = new bool[miniBatchSize]; // Boolean to indicate if the current mini batch is done (This is true on the last frame of an episode to prevent it from being used for training)

        // Unpack mini batches
        for (int i = 0; i < miniBatchSize; i++)
        {
            if (miniBatch[i] != null)
            {
                states[i] = env.GetState(miniBatch[i].Item1 - 1);
                nextStates[i] = env.GetState(miniBatch[i].Item1); // Next state ends with the last frame
                actions[i] = miniBatch[i].Item2;
                rewards[i] = miniBatch[i].Item3;
                dones[i] = miniBatch[i].Item4;
            }
        }

        double[][] batchOutputs = new double[miniBatchSize][];
        double[][] batchTargets = new double[miniBatchSize][];

        double[] outputTotal = new double[actionQty];
        double[] targetTotal = new double[actionQty];

        for (int i = 0; i < miniBatchSize; i++) // Iterate through each mini batch
        {
            //double[] qState = mainNet.FeedForward(states[i]); // Used for all targets that are not the highest q value
            batchOutputs[i] = mainNet.FeedForward(states[i]);
            double[] qNextState = mainNet.FeedForward(nextStates[i]); 
            int argMaxAction = GameManager.instance.math.ArgMax(qNextState); // Calculate argmax (returns highest q-value index)

            //double[] target = CalculateTargets(nextStates[i], qState, rewards[i], dones[i], argMaxAction, actions[i]); // Calculate the targets for the mini batch
            batchTargets[i] = CalculateTargets(nextStates[i], batchOutputs[i], rewards[i], dones[i], argMaxAction, actions[i]);

            mainNet.FeedForward(states[i]);
            mainNet.Backpropagation(batchTargets[i]);

            for (int j = 0; j < actionQty; j++)
            {
                outputTotal[j] += batchOutputs[i][j];
                targetTotal[j] += batchTargets[i][j];
            }
        }

        cost = Cost(outputTotal, targetTotal);

        return false;
    }
    // Calculate the Targets for each mini batch
    public double[] CalculateTargets(double[] nextStates, double[] qStates, double reward, bool done, int argMax, int action)
    {
        double[] qNextStateTarget = targetNet.FeedForward(nextStates);

        double[] targets = new double[actionQty];

        for (int i = 0; i < targets.Length; i++)
        {
            targets[i] = qStates[i];
        }

        //Debug.Log("qStates: " + targets[3] + " qNextStates: " + qNextStateTarget[3]);
        if (done == true)
            targets[action] = reward;
        else
            targets[action] = reward + (GameManager.instance.settings.gamma * qNextStateTarget[argMax]);

        return targets;
    }

    public double Cost(double[] actions, double[] targets)
    {
        double sum = 0;

        for (int i = 0; i < actionQty; i++)
        {
            double error = targets[i] - actions[i];
            sum += (error * error);
        }

        return sum / actionQty;
    }
}
