using UnityEngine;
using System;

[Serializable]
public class DQN : MonoBehaviour
{
    [Header("Neural Networks")]
    public NeuralNet mainNet;
    public NeuralNet targetNet;
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
    public int stateSize; // Number of frames in a state
    public int frameSize; // The number of inputs that comprise each frame
    public double currentLearningRate;
    public float epsilonChange; // This is the rate at which the value of epsilon will reduce each update
    [Space(10)]

    public UIManager ui;
    public Transform tf;
    public Material mat;
    public Color red;
    public Color white;

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
        frameSize = GameManager.instance.settings.layers[0] / GameManager.instance.settings.framesPerState;
        stateSize = frameSize * GameManager.instance.settings.framesPerState;
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
        GameManager.instance.settings.layers = new int[] { 40, 20, 10, 5 };
        actionQty = GameManager.instance.settings.layers[GameManager.instance.settings.layers.Length - 1];
        layerQty = GameManager.instance.settings.layers.Length;

        mainNet = new NeuralNet(GameManager.instance.settings.layers);
        // TODO: Check all of the following
        mainNet.Mutate();
        targetNet = new NeuralNet(GameManager.instance.settings.layers);
        CopyNetwork();
    }

    public void RunGame()
    {
        if (episodeNum < GameManager.instance.settings.episodeMax)
        {
            isDone = false;
        }

        if (episodeNum % GameManager.instance.settings.autoSaveEpisode == 0 && !isSaved) // Autosave feature
        {
            string fileName = GameManager.instance.settings.agentName + "_as_e" + episodeNum + ".gd";
            string settingsName = GameManager.instance.settings.agentName + "_as_e" + episodeNum + "_settings.gd";
            SaveLoad.SaveNet(fileName, this);
            SaveLoad.SaveSettings(settingsName);
            isSaved = true;
        }

        if (episodeNum == GameManager.instance.settings.episodeMax)
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
            if (epochs % GameManager.instance.settings.netCopyRate == 0)
            {
                CopyNetwork();
            }

            // Get state from frame buffer
            double[] currentState = env.GetState(env.fbIndex - 1);

            // Perform the action
            double[] currentAction = agent.PerformAction(currentState, GameManager.instance.settings.epsilon, actionQty);

            epochs++; // Increment total steps
            epiSteps++; // Increment steps in episode

            if (epiSteps >= GameManager.instance.settings.epiMaxSteps)
            {
                episodeNum++;
                isDone = true;
                isSaved = false;
                epiSteps = 0;
                episodeReward = 0;
                GameManager.instance.RandomSpawn();
                tf.position = GameManager.instance.spawnpoint.position;
            }

            // Creates next frame
            double[] nextFrame = env.GetNextFrame();

            // Add frame to frame buffer
            int lastFrameIndex = env.UpdateFrameBuffer(nextFrame);

            // Calculates the reward based on the state
            double currentReward = env.CalculateReward(nextFrame);

            // Add current reward to the episode and total reward
            episodeReward += currentReward;
            totalReward += currentReward;

            // Update experience replay memory
            agent.ExperienceReplay(lastFrameIndex, currentAction, currentReward, isDone);

            if (isTraining == true && isConverged == false)
            {
                // Train the agent
                isConverged = mainNet.Train(this, env, agent);
            }

            // Recalculate epsilon
            GameManager.instance.settings.epsilon = Mathf.Max(GameManager.instance.settings.epsilon - epsilonChange, GameManager.instance.settings.epsilonMin);
        }
    }

    public void CopyNetwork()
    {
        targetNet.SetWeightsMatrix(mainNet.GetWeightsMatrix());
        targetNet.SetBiases(mainNet.GetBiases());
    }
}
