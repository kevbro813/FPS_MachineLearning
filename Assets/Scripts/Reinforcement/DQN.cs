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
    public int[] mLayers = new int[] { 10, 10, 10, 3 };
    public int[] tLayers = new int[] { 10, 10, 10, 3 };
    public float[][] frames;


    private void Start()
    {
        agent = GetComponent<Agent>();
        env = GetComponent<Environment>();
    }
    private void Update()
    {
        RunGame();
    }
    public void InitAINeuralNets()
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
        for (int i = 1; i <= episodeMax; i++) // For each episode...
        {
            episodeCount = i; // Increase episode count. Will be used for display
            float reward = RunEpisode(agent, env); // Run the RunEpisode method passing in the agent and environment and returning the score (reward) for the episode.
            //rewards.Add(score); // Add the score to the list of reward scores. TODO: Sort functionality, Icomparable.

            // TODO: Save rewards file
        }
    }
    // Run one episode
    public float RunEpisode(Agent agent, Environment env)
    {
        // Reset the environment's state each episode
        env.ResetEnv();     

        bool isDone = false;
        while (!isDone)
        {
            env.normalizedStates = NormalizeState(env.stateMeans, env.states, env.stateCounter, env.stateVariance, env.stateStdDev);

            env.stateCounter++; // Where should this increment go?

            // Select an action using the main neural network
            float[] actions = agent.GetAction(env.normalizedStates);

            // Perform action, calculate next state, calculate reward
            env.Step(actions);

            // Update experience replay memory
            ExperienceReplay();

            // Train the model
            Learn();

            // Copy weights from main to target network periodically
            if (env.stateCounter % 100 == 0)
            {
                targetNet = mainNet;
            }

            isDone = true;
        }
        return 0;
    }
    public void ExperienceReplay()
    {
        // Buffer that stores (s, a, r, s') 4-tuples
        // state buffer is a buffer of x number of frames
        //frames[][]; // Frames rotate and fill in a loop. Need to prevent using end/beginning frames in the same state
        //actions[][];
        //rewards[][];
    }
    public void Learn()
    {
        // Train the model using a neural network (states form inputs)

    }

    // Scalar function used to normalize all input values that make up a state
    public float[] NormalizeState(float[] means, float[] states, float counter, float[] variances, float[] stdDevs)
    {
        float[] normalizedStates = new float[states.Length];

        // Iterate scalar function through each state input
        for (int i = 0; i < states.Length; i++)
        {
            // Update the mean with the new datapoint
            means[i] = UpdateMean(counter, means[i], states[i]);

            // Calculate the squared difference for the new datapoint
            float sqrdDiff = SquaredDifference(states[i], means[i]);

            // Calculate total squared difference from variance
            float totalSqrdDiff = variances[i] * counter;

            // Update the total squared difference
            totalSqrdDiff += sqrdDiff;

            counter++; // TODO: Need to move this. It should only update once per tick, not once per 

            // Recalculate Variance and Standard Deviation
            variances[i] = Variance(totalSqrdDiff, counter);
            stdDevs[i] = StdDeviation(variances[i]);

            // Normalize the current state values
            normalizedStates[i] = ScalarFunction(states[i], means[i], stdDevs[i]);
        }

        return normalizedStates;
    }
    public float ScalarFunction(float dp, float mean, float stdDev)
    {
        // Calculate a state's Z-score = (data point - mean) / standard deviation
        float zScore = (dp - mean) / stdDev;

        return zScore;
    }
    public float UpdateMean(float sampleSize, float mean, float dp)
    {
        float sampleTotal = sampleSize * mean;
        sampleTotal += dp;
        sampleSize++;
        mean = sampleTotal / sampleSize;

        return mean;
    }
    public float SquaredDifference(float dp, float mean)
    {
        float sqrdDiff = Mathf.Pow(dp - mean, 2);
        return sqrdDiff;
    }
    public float Variance(float totalSqrdDiff, float stateCounter)
    {
        float variance = totalSqrdDiff / stateCounter;
        return variance;
    }
    public float StdDeviation(float variance)
    {
        float stdDev = Mathf.Sqrt(variance);
        return stdDev;
    }
}
