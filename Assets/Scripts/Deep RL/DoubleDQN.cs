using System;
using UnityEngine;

[Serializable]
public class DoubleDQN
{
    private int miniBatchSize;
    private int framesPerState;
    private double gamma;
    private int actionQty;
    private Environment env;
    private Agent agent;
    private NeuralNetwork mainNet;
    private NeuralNetwork targetNet;
    private double[][] states;
    private double[][] nextStates;
    private int[] actions;
    private double[] rewards;
    private bool[] dones;
    private double[] targetQ;
    private double[] mainQ;
    private double[] qNextState;
    private double[] qNextStateTarget;
    private double[] targets;
    public void Init_Double_DQN(int actQty, Environment e, Agent a, NeuralNetwork main, NeuralNetwork target)
    {
        miniBatchSize = RLManager.instance.settings.miniBatchSize; // Size of the mini batch used to train the target net
        framesPerState = RLManager.instance.settings.framesPerState;
        gamma = RLManager.instance.settings.gamma;
        actionQty = actQty;
        env = e;
        agent = a;
        mainNet = main;
        targetNet = target;

        // Initialize arrays to hold batch data
        states = new double[miniBatchSize][]; // State
        nextStates = new double[miniBatchSize][]; // Next State (The resulting state after an action)
        actions = new int[miniBatchSize]; // The action performed
        rewards = new double[miniBatchSize]; // Reward for the action
        dones = new bool[miniBatchSize]; // Boolean to indicate if the current mini batch is done (This is true on the last frame of an episode to prevent it from being used for training)

        // Required for target calculation
        qNextState = new double[actionQty];
        qNextStateTarget = new double[actionQty];
        targets = new double[actionQty];
    }
    /// <summary>
    /// This method is the main training algorithm.
    /// </summary>
    public double Train()
    {
        // This Tuple array consists of one mini batch (random sample from experience replay buffer)
        Tuple<int, int, double, bool>[] miniBatch = agent.GetMiniBatch(miniBatchSize, env.fbIndex, framesPerState);

        UnpackMiniBatch(miniBatch); // Unpack the mini batches into arrays

        return DoubleDQNTraining(); // Take the mini batch and train using double DQN method, returns cost
    }

    private double DoubleDQNTraining()
    {
        double cost = 0;

        for (int i = 0; i < miniBatchSize; i++) // Iterate through each mini batch
        {
            targetQ = CalculateTargets(states[i], nextStates[i], actions[i], rewards[i], dones[i]);

            mainNet.Backpropagation(targetQ);
            
            cost = Cost(mainQ, targetQ);
        }

        return cost;
    }
    /// <summary>
    /// Calculate the targets used to train the network.
    /// </summary>
    /// <param name="states"></param>
    /// <param name="nextStates"></param>
    /// <param name="action"></param>
    /// <param name="reward"></param>
    /// <param name="done"></param>
    /// <returns></returns>
    private double[] CalculateTargets(double[] state, double[] nextState, int action, double reward, bool done)
    {
        qNextState = mainNet.FeedForward(nextState); // Calculate Q values for the next state using the main network
        qNextStateTarget = targetNet.FeedForward(nextState); // Calculate next state Q values using target network
        int argMax = RLManager.math.ArgMax(qNextState); // Calculate argmax (returns highest q-value index) of qNextState
        targets = new double[actionQty];
        mainQ = mainNet.FeedForward(state); // Calculate current state Q values using main network
        for (int i = 0; i < actionQty; i++)
        {
            targets[i] = mainQ[i];
        }
        if (done == false)
        {
            targets[action] = reward + gamma * qNextStateTarget[argMax];
        }
        else
            targets[action] = reward;

        return targets;
    }
    /// <summary>
    /// Calculate cost using Mean Squared Error.
    /// </summary>
    /// <param name="mainQs"></param>
    /// <param name="targetQs"></param>
    /// <returns></returns>
    private double Cost(double[] mainQs, double[] targetQs)
    {
        double sum = 0;

        for (int i = 0; i < actionQty; i++)
        {
            double error = targetQs[i] - mainQs[i];
            sum += (error * error);
        }

        return sum / actionQty;
    }
    private void UnpackMiniBatch(Tuple<int, int, double, bool>[] mb)
    {
        // Unpack mini batches
        for (int i = 0; i < miniBatchSize; i++)
        {
            if (mb[i] != null)
            {
                states[i] = env.GetState(mb[i].Item1 - 1);
                nextStates[i] = env.GetState(mb[i].Item1); // Next state ends with the last frame
                actions[i] = mb[i].Item2;
                rewards[i] = mb[i].Item3;
                dones[i] = mb[i].Item4;
            }
        }
    }
}
