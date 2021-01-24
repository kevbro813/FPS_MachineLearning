using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[Serializable]
public class PPO
{
    #region Variables
    private Environment env;
    private Agent agent;
    private NeuralNetwork actorNet;
    private NeuralNetwork criticNet;
    [HideInInspector] public float ppoClip; // Amount to clip surrogate, usually 0.2
    [HideInInspector] public int actionQty; // Number of actions
    [HideInInspector] public float clipMinimum; // Determined using ppoClip, lower bound
    [HideInInspector] public float clipMaximum; // Determined using ppoClip, upper bound
    [HideInInspector] public double entropyBonus; // Bonus entropy added to promote random behaviour until learned behaviors strengthen.
    [HideInInspector] public double gamma; // Reward discount
    [HideInInspector] public double tau; // Discount used in GAE calculation
    [HideInInspector] public double delta; // "delta t" used in GAE calculation
    [HideInInspector] public int trainingEpochs; // Number of times a network will be trained on an episode's data
    [HideInInspector] public double lastGAE; // Used to temporarily save GAE value
    public int batchSize; // Size of the batch = frameBufferSize - framesPerState + 1
    [HideInInspector] public double[] rewards; // Stores a batch of rewards
    [HideInInspector] public int[] actions; // Stores a batch of actions taken during episode
    [HideInInspector] public double[][] predictions; // Stores action probabilities taken during episode
    [HideInInspector] public double[] values; // Save state values
    [HideInInspector] public bool[] dones; // Saves done flags
    [HideInInspector] public double[] advantages; // Advantage calculation used in Clipped Surrogate Objective Function
    [HideInInspector] public double[] newLogProbs; // The negative of the log probabilities taken during training and used in Clipped Surrogate Objective Function
    [HideInInspector] public double[] oldLogProbs; // The negative of the log probabilities taken during the episode and used in Clipped Surrogate Objective Function
    [HideInInspector] public double[] ratios; // Ratios calculated for Clipped Surrogate Objective Function
    [HideInInspector] public double[] p1; // advantage * ratio
    [HideInInspector] public double[] p2; // advantage * clip(ratio)
    [HideInInspector] public double[] actorError; // The final output of the Clipped Surrogate Objective Function that is used in backpropagation
    [HideInInspector] public double[] returns; // Returns used to backpropagate critic network
    [HideInInspector] public int[] oneHotActions; // Stores one hot actions for an episode
    [HideInInspector] public double actorLoss; // Loss for the actor network
    [HideInInspector] public double criticLoss; // Loss for the critic network
    #endregion

    #region Initialization
    /// <summary>
    /// Initialize new PPO training.
    /// </summary>
    /// <param name="actQty"></param>
    /// <param name="e"></param>
    /// <param name="a"></param>
    /// <param name="actor"></param>
    /// <param name="critic"></param>
    public void Init_PPO(int actQty, Environment e, Agent a, NeuralNetwork actor, NeuralNetwork critic)
    {
        // Initialize variables to starting values
        env = e;
        agent = a;
        actorNet = actor;
        criticNet = critic;
        actionQty = actQty;
        actorLoss = 0;
        criticLoss = 0;
        gamma = RLManager.instance.settings.gamma;
        ppoClip = RLManager.instance.settings.ppoClip;
        entropyBonus = RLManager.instance.settings.entropyBonus;
        tau = RLManager.instance.settings.tau;
        trainingEpochs = RLManager.instance.settings.trainingEpochs;
        clipMinimum = 1 - ppoClip;
        clipMaximum = 1 + ppoClip;

        // PPO trains after every episode and uses the entire batch once. Therefore, the batch size will be the episode length - framesPerState + 1
        // This is due to the experience buffer not starting until there are enough frames.
        batchSize = RLManager.instance.settings.epiMaxSteps - RLManager.instance.settings.framesPerState + 1; 

        // Initialize arrays
        rewards = new double[batchSize];
        predictions = new double[batchSize][];
        values = new double[batchSize];
        dones = new bool[batchSize];
        advantages = new double[batchSize];
        returns = new double[batchSize];
        newLogProbs = new double[actionQty];
        oldLogProbs = new double[actionQty];
        ratios = new double[actionQty];
        p1 = new double[actionQty];
        p2 = new double[actionQty];
        actorError = new double[actionQty];
        oneHotActions = new int[actionQty];
        actions = new int[batchSize];
    }
    #endregion

    #region PPO Training
    /// <summary>
    /// Runs one full training session. Returns the mean critic loss.
    /// </summary>
    /// <returns></returns>
    public double PPOTraining()
    {
        criticLoss = 0; // Reset loss training

        UnpackBatch(); // Unpack Tuple with batch data

        GeneralizedAdvantageEstimation(); // Compute Advantages

        Train(); // Train Agent

        return criticLoss / batchSize; // Return mean of critic loss for the episode
    }
    /// <summary>
    /// Unpack a batch of data collected from an episode. Converts tuples into individual arrays of batchSize.
    /// </summary>
    private void UnpackBatch()
    {
        for (int i = 0; i < batchSize; i++)
        {
            actions[i] = agent.ppoExperienceBuffer[i].Item1;
            rewards[i] = agent.ppoExperienceBuffer[i].Item2;
            predictions[i] = agent.ppoExperienceBuffer[i].Item3;
            values[i] = agent.ppoExperienceBuffer[i].Item4;
            dones[i] = agent.ppoExperienceBuffer[i].Item5;
        }
    }
    /// <summary>
    /// Generalized Advantage Estimation used to get Advantages and Returns
    /// </summary>
    private void GeneralizedAdvantageEstimation()
    {
        lastGAE = 0; // Saves the last GAE to use in next iteration
        for (int i = batchSize - 1; i >= 0; i--) // Reverse iteration through the batch of data collected over the last episode
        {
            double d; // Done flag (1 if not done, 0 if done)
            if (dones[i]) // If terminal episode 
            {
                d = 0; // Set to zero for lastGAE calculation
                delta = rewards[i] - values[i]; // delta = reward - value (gamma is multiplied by zero in the terminal state and there is no next state)
            }
            else // All other episodes
            {
                d = 1; // Set to one for lastGAE calculation
                delta = rewards[i] + gamma * values[i + 1] - values[i]; // delta = reward + gamma * next_state_value * done - current_state_value
            }

            lastGAE = delta + gamma * tau * d * lastGAE; // lastGAE = delta + gamma * tau * done * lastGAE (future rewards have an effect on advantage estimation)
            returns[i] = lastGAE + values[i]; // Returns calculation (used to update Critic Network)
        }
    }
    /// <summary>
    /// Train the agent for "x" training epochs after an episode.
    /// </summary>
    private void Train()
    {
        for (int i = 0; i < trainingEpochs * batchSize; i++) // Run training "x" number of times over the training data collected during one episode
        {
            int batchIndex = UnityEngine.Random.Range(0, batchSize - 1); // Fixed index out of range error by subtracting one from batchSize
            double[] state = env.GetState(batchIndex + RLManager.instance.settings.framesPerState - 1); // Create a state from the frame buffer
            ClippedSurrogateObjective(state, batchIndex); // Run the clipped surrogate objective (Main part of PPO)
            UpdateActor(state, actorError, oneHotActions); // Update the actor network
            UpdateCritic(state, returns[batchIndex]); // Update the critic network
        }
    }
    #endregion

    #region PPO Algorithm (Using Clipped Surrogate Objective)
    /// <summary>
    /// This is the main part of the PPO algorithm.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="batchIndex"></param>
    private void ClippedSurrogateObjective(double[] state, int batchIndex)
    {
        // Use actor network to calculate y Prediction
        double[] yPred = actorNet.FeedForward(state);

        // Use critic network to calculate state Value
        double stateValue = criticNet.FeedForward(state)[0];

        advantages[batchIndex] = returns[batchIndex] - values[batchIndex];

        oneHotActions = new int[actionQty];
        oneHotActions[actions[batchIndex]] = 1;

        newLogProbs = RLManager.math.LogProbs(yPred); // Get the negative of the log probabilities (new calculated probabilities), negative logs needed for gradient ascent
        oldLogProbs = RLManager.math.LogProbs(predictions[batchIndex]);

        // ratio = new log / old log = E^(new log - old log)
        for (int i = 0; i < actionQty; i++)
        {
            ratios[i] = Math.Exp(newLogProbs[i] - oldLogProbs[i]);  // Old log probs are calculated during the episode to improve training efficiency
        }

        // Calculate the output "actorError" of the objective function
        for (int i = 0; i < actionQty; i++)
        {
            p1[i] = advantages[batchIndex] * ratios[i]; // p1 = advantages * ratios
            p2[i] = advantages[batchIndex] * Clip(ratios[i]); // p2 = advantages * clippedRatio

            // loss = -mean(minimum(p1, p2) + entropyLoss * -(prob * K.log(prob + 1e-10)))
            actorError[i] = Math.Min(p1[i], p2[i]) + entropyBonus * -(yPred[actions[batchIndex]] * Math.Log(yPred[actions[batchIndex]] + 1e-10)); 
        }

        actorLoss = 0; // TODO: Actor loss calculation

        CriticLoss(returns[batchIndex], stateValue); // Calculate critic loss for display (just for show)
    }

    /// <summary>
    /// MSE loss is used for critic loss.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="stateValue"></param>
    /// <returns></returns>
    private double CriticLoss(double target, double stateValue)
    {
        double error = target - stateValue;
        double loss = 0.5d * (error * error);

        criticLoss += loss;

        return criticLoss;
    }
    /// <summary>
    /// Clips the ratio and returns it.
    /// </summary>
    /// <param name="ratio"></param>
    /// <returns></returns>
    private double Clip(double ratio)
    {
        if (ratio >= clipMaximum)
            return clipMaximum;
        else if (ratio <= clipMinimum)
            return clipMinimum;
        else
            return ratio;
    }
    #endregion

    #region Update Actor/Critic Neural Networks
    /// <summary>
    /// Train the critic network using the state value as the target in backpropagation.
    /// </summary>
    /// <param name="states"></param>
    /// <param name="valueTarget"></param>
    private void UpdateCritic(double[] states, double valueTarget)
    {
        double[] target = { valueTarget };
        criticNet.FeedForward(states);
        criticNet.Backpropagation(target);
    }
    /// <summary>
    /// Train the actor network using policy gradient. Error should already be calculated.
    /// </summary>
    /// <param name="states"></param>
    /// <param name="error"></param>
    /// <param name="actions"></param>
    private void UpdateActor(double[] states, double[] error, int[] actions)
    {
        actorNet.FeedForward(states);
        actorNet.Backpropagation(error, actions);
    }
    #endregion
}
