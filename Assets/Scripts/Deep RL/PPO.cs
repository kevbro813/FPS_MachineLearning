﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[Serializable]
public class PPO
{
    private Environment env;
    private Agent agent;
    private NeuralNetwork actorNet;
    private NeuralNetwork criticNet;
    private float ppoClip; // Amount to clip surrogate, usually 0.2
    private int actionQty; // Number of actions
    private float clipMinimum; // Determined using ppoClip, lower bound
    private float clipMaximum; // Determined using ppoClip, upper bound
    private double entropyBonus; // Bonus entropy added to promote random behaviour until learned behaviors strengthen.
    private double gamma; // Reward discount
    private double tau; // Discount used in GAE calculation
    private double delta; // "delta t" used in GAE calculation
    private int trainingEpochs; // Number of times a network will be trained on an episode's data
    private double lastGAE; // Used to temporarily save GAE value
    private int batchSize; // Size of the batch = frameBufferSize - framesPerState + 1
    private double[] rewards; // Stores a batch of rewards
    private int[] actions; // Stores a batch of actions taken during episode
    private double[][] predictions; // Stores action probabilities taken during episode
    private double[] values; // Save state values
    private bool[] dones; // Saves done flags
    private double[] advantages; // Advantage calculation used in Clipped Surrogate Objective Function
    private double[] newLogProbs; // The negative of the log probabilities taken during training and used in Clipped Surrogate Objective Function
    private double[][] oldLogProbs; // The negative of the log probabilities taken during the episode and used in Clipped Surrogate Objective Function
    private double[] ratios; // Ratios calculated for Clipped Surrogate Objective Function
    private double[] p1; // advantage * ratio
    private double[] p2; // advantage * clip(ratio)
    private double[] actorError; // The final output of the Clipped Surrogate Objective Function that is used in backpropagation
    private double[] returns; // Returns used to backpropagate critic network
    private int[][] oneHotActions; // Stores one hot actions for an episode

    public double actorLoss; // Loss for the actor network
    public double criticLoss; // Loss for the critic network
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
        clipMinimum = 1 - ppoClip;
        clipMaximum = 1 + ppoClip;
        actorLoss = 0;
        criticLoss = 0;
        gamma = RLManager.instance.settings.gamma;
        ppoClip = RLManager.instance.settings.ppoClip;
        entropyBonus = RLManager.instance.settings.entropyBonus;
        tau = RLManager.instance.settings.tau;
        trainingEpochs = RLManager.instance.settings.trainingEpochs;

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
        oldLogProbs = new double[batchSize][];
        ratios = new double[actionQty];
        p1 = new double[actionQty];
        p2 = new double[actionQty];
        actorError = new double[actionQty];
        oneHotActions = new int[batchSize][];
        actions = new int[batchSize];
    }
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
                delta = rewards[i] + gamma * values[i + 1] - values[i]; // delta = reward + gamma * (next state value) * 1(done) - (current state value)
            }
            
            lastGAE = delta + gamma * tau * d * lastGAE; // last GAE = delta + gamma * tau * done * lastGAE (future rewards have an effect on advantage estimation)
            advantages[i] = lastGAE; // Store advantages
            returns[i] = lastGAE + values[i]; // Returns calculation (used to update Critic Network)
        }
        advantages = RLManager.math.Normalize(advantages); // Normalize the advantages
    }
    /// <summary>
    /// Train the agent for "x" training epochs after an episode.
    /// </summary>
    private void Train()
    {
        for (int i = 0; i < trainingEpochs; i++) // Run training "x" number of times over the training data collected during one episode
        {
            int batchIndex = 0; // Used to iterate through stored batch data
            for (int j = env.framesPerState - 1; j < env.frameBufferSize; j++) // Begin iterating at the first frame that makes a full state and end with the last frame
            {
                double[] state = env.GetState(j); // Create a state from the frame buffer
                ClippedSurrogateObjective(state, batchIndex); // Run the clipped surrogate objective (Main part of PPO)
                UpdateActor(state, actorError, oneHotActions[batchIndex]); // Update the actor network
                UpdateCritic(state, returns[batchIndex]); // Update the critic network
                batchIndex++;
            }
        }
    }
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

        newLogProbs = RLManager.math.LogProbs(yPred); // Get the negative of the log probabilities (new calculated probabilities), negative logs needed for gradient ascent
        
        // ratio = new log / old log = E^(new log - old log)
        for (int i = 0; i < actionQty; i++)
        {
            ratios[i] = Math.Exp(newLogProbs[i] - oldLogProbs[batchIndex][i]);  // Old log probs are calculated during the episode to improve training efficiency
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
            oneHotActions[i] = agent.ppoExperienceBuffer[i].Item5;
            oldLogProbs[i] = agent.ppoExperienceBuffer[i].Item6;
            dones[i] = agent.ppoExperienceBuffer[i].Item7;
        }
    }
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
    /// <param name="targets"></param>
    /// <param name="actions"></param>
    private void UpdateActor(double[] states, double[] error, int[] actions)
    {
        actorNet.FeedForward(states);
        actorNet.Backpropagation(error, actions);
    }
}