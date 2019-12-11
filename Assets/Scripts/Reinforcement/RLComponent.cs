using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RLComponent : MonoBehaviour
{
    public Agent agent; // Create an instance of the agent
    public Environment env; // Create an instance of the environment
    public List<float> rewards; // Create a list to store reward values
    public int episodeQty = 10;
    public int episodeCount = 0;
    public bool isTraining = true;

    private void Start()
    {
        agent = GetComponent<Agent>();
        env = GetComponent<Environment>();
    }
    private void Update()
    {
        NormalizeState();
    }
    public void RunSet() // Run a set of episodes
    {
        for (int i = 1; i <= episodeQty; i++) // For each episode...
        {
            episodeCount = i; // Increase episode count. Will be used for display
            float score = RunEpisode(agent, env); // Run the RunEpisode method passing in the agent and environment and returning the score (reward) for the episode.
            rewards.Add(score); // Add the score to the list of reward scores. TODO: Sort functionality, Icomparable.
            // TODO: Save rewards file
        }
    }
    // Run one episode
    public float RunEpisode(Agent agent, Environment env)
    {
        // Reset the environment's state each episode
        env.ResetEnv();

        NormalizeState();

        bool isDone = false;
        while (!isDone)
        {
            agent.GetAction(env.states); // The action comes from the agent based on the current state

            env.Step(agent.actions); // This function performs action and gets the next state, reward, etc.
            
            NormalizeState(); // Normalize next state

            if (isTraining)
            {
                isDone = agent.TrainAgent(env.states, agent.actions, rewards); // Function trains using a form of gradient descent
                
                // TODO: Current State = Next State (Set current state to next state for next iteration) - Not sure if I need to do this here

                // TODO: return reward score
            }
            isDone = true;
        }
        return 0;
    }
    public void NormalizeState()
    {
        // Iterate scalar function through each state input
        for (int i = 0; i < env.states.Length; i++)
        {
            // Update the mean with the new datapoint
            env.stateMeans[i] = UpdateMean(env.stateCounter, env.stateMeans[i], env.states[i]);
            
            // Calculate the squared difference for the new datapoint
            float sqrdDiff = SquaredDifference(env.states[i], env.stateMeans[i]);

            // Calculate total squared difference from variance
            float totalSqrdDiff = env.stateVariance[i] * env.stateCounter; 

            // Update the total squared difference
            totalSqrdDiff += sqrdDiff;

            env.stateCounter++; // TODO: Need to move this. It should only update once per tick, not once per 

            // Recalculate Variance and Standard Deviation
            env.stateVariance[i] = Variance(totalSqrdDiff, env.stateCounter);
            env.stateStdDev[i] = StdDeviation(env.stateVariance[i]);

            // Normalize the current state values
            env.normalizedStates[i] = ScalarFunction(env.states[i], env.stateMeans[i], env.stateStdDev[i]);
        }
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
