using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
[Serializable]
public class PPO
{
    private Environment env;
    private Agent agent;
    private NeuralNetwork actorNet;
    private NeuralNetwork criticNet;
    private float ppoEpsilon = 0.2f; // TODO: Add to settings
    private int actionQty;
    private float clipMinimum;
    private float clipMaximum;
    private double entropyBonus = 0.005d; // TODO: Add to settings
    public double actorLoss;
    public double criticLoss;
    private double gamma;
    private int batchSize;

    public double[] discountRewards;
    public double[] rewards;
    public int[][] actions;
    public double[][] predictions;
    public double[] values;
    public bool[] dones;
    private double[] advantages;
    //private double[] probabilities;
    //private double[] oldProbabilities;
    private double[] ratios;
    private double[] p1;
    private double[] p2;
    public double[] actorTargets;


    public int updateStep;

    public void Init_PPO(int actQty, Environment e, Agent a, NeuralNetwork actor, NeuralNetwork critic)
    {
        env = e;
        agent = a;
        actorNet = actor;
        criticNet = critic;
        actionQty = actQty;
        clipMinimum = 1 - ppoEpsilon;
        clipMaximum = 1 + ppoEpsilon;
        actorLoss = 0;
        criticLoss = 0;
        gamma = RLManager.instance.settings.gamma;

        // PPO trains after every episode and uses the entire batch once. Therefore, the batch size will be the episode length - framesPerState + 1
        // This is due to the experience buffer not starting until there are enough frames.
        batchSize = RLManager.instance.settings.epiMaxSteps - RLManager.instance.settings.framesPerState + 1; 

        // Initialize arrays
        discountRewards = new double[batchSize];
        rewards = new double[batchSize];
        actions = new int[batchSize][];
        predictions = new double[batchSize][];
        values = new double[batchSize];
        dones = new bool[batchSize];
        advantages = new double[batchSize];

        //probabilities = new double[actionQty];
        //oldProbabilities = new double[actionQty];
        ratios = new double[actionQty];
        p1 = new double[actionQty];
        p2 = new double[actionQty];
        actorTargets = new double[actionQty];
    }
    public double PPOReplay()
    {
        UnpackBatch(); // Unpack Tuple with batch data

        // Compute Discounted Rewards 
        DiscountRewards(); // TODO: Check

        // Compute Advantages
        Advantages(); // TODO: Check

        // Train Agent
        Train(); // TODO: Check

        return criticLoss / batchSize;
    }
    private void Train()
    {
        int batchIndex = 0;
        for (int i = env.framesPerState - 1; i < env.frameBufferSize; i++)
        {
            double[] state = env.GetState(i);
            ClippedSurrogateObjective(state, batchIndex);
            UpdateActor(state, actorTargets);
            UpdateCritic(state, discountRewards[batchIndex]);
            batchIndex++;
        }
    }
    private void DiscountRewards()
    {
        double lambda = 0.95d; // Lambda
        double d = 1; // Done flag (1 if not done, 0 if done)
        double lastGAE = 0;
        double delta;
        for (int i = batchSize - 1; i >= 0; i--)
        {
            if (dones[i])
            {
                d = 0;
                delta = rewards[i] - values[i];
            }
            else
            {
                delta = rewards[i] + gamma * values[i + 1] - values[i];
            }
            
            lastGAE = delta + gamma * lambda * d * lastGAE;

            discountRewards[i] = lastGAE + values[i];
        }

        discountRewards = Normalize(discountRewards);
    }

    private void Advantages()
    {
        for (int i = 0; i < advantages.Length; i++)
        {
            advantages[i] = discountRewards[i] - values[i];
        }

        //advantages = Normalize(advantages);
        //Debug.Log(advantages[advantages.Length - 1]);
    }
    private void ClippedSurrogateObjective(double[] state, int batchIndex)
    {
        // Use actor network to calculate y Prediction
        double[] yPred = actorNet.FeedForward(state);

        // Use critic network to calculate stateValue
        double stateValue = criticNet.FeedForward(state)[0];

        // Calculate ratios
        for (int i = 0; i < actionQty; i++)
        {
            ratios[i] = Math.Exp(predictions[batchIndex][i] - yPred[i]);
        }
        //Debug.Log(actions[batchIndex][0] + "    " + actions[batchIndex][1] + "    " + actions[batchIndex][2] + "    " + actions[batchIndex][3] + "    " + actions[batchIndex][4]);

        for (int i = 0; i < actionQty; i++)
        {
            p1[i] = -advantages[batchIndex] * ratios[i]; // p1 = ratios * advantages
            p2[i] = -advantages[batchIndex] * Clip(ratios[i]); // p2 = clippedRatio * advantages
        }
        
        double total = 0;

        for (int i = 0; i < actionQty; i++) // loss = -mean(minimum(p1, p2) + entropyLoss * -(prob * K.log(prob + 1e-10)))
        {
            actorTargets[i] = Math.Max(p1[i], p2[i]); //+ entropyBonus; //* -(probabilities[i] * Math.Log(probabilities[i] + 1e-10)); // TODO: Check
            total += actorTargets[i];
        }
        //Debug.Log(actorTargets[0] + "    " + actorTargets[1] + "    " + actorTargets[2] + "    " + actorTargets[3] + "    " + actorTargets[4]);

        actorLoss = total / actionQty;

        CriticLoss(discountRewards[batchIndex], values[batchIndex]);
    }
    // MSE Cost
    private double CriticLoss(double target, double stateValue)
    {
        double error = stateValue - target;
        double loss = 0.5d * (error * error);

        criticLoss += loss;

        return criticLoss;
    }
    private double Clip(double ratio)
    {
        if (ratio >= clipMaximum)
            return clipMaximum;
        else if (ratio <= clipMinimum)
            return clipMinimum;
        else
            return ratio;
    }

    private double[] Normalize(double[] data)
    {
        double meanDiscountReward = RLManager.math.Mean(data);
        double totalSquaredDifference = 0;

        for (int i = 0; i < data.Length; i++)
        {
            totalSquaredDifference += RLManager.math.SquaredDifference(data[i], meanDiscountReward);
            data[i] -= meanDiscountReward;
        }

        double variance = RLManager.math.Variance(totalSquaredDifference, data.Length);
        double stdDeviation = RLManager.math.StdDeviation(variance);

        for (int i = 0; i < data.Length; i++)
        {
            data[i] /= (stdDeviation + 1e-10);
        }

        return data;
    }
    private void UnpackBatch()
    {
        for (int i = 0; i < batchSize; i++)
        {
            int[] actionOneHot = new int[actionQty];
            actionOneHot[agent.ppoExperienceBuffer[i].Item1] = 1;
            actions[i] = actionOneHot;
            rewards[i] = agent.ppoExperienceBuffer[i].Item2;
            predictions[i] = agent.ppoExperienceBuffer[i].Item3;
            values[i] = agent.ppoExperienceBuffer[i].Item4;
            dones[i] = agent.ppoExperienceBuffer[i].Item5;
        }
    }
    // Train Critic Net
    private void UpdateCritic(double[] states, double valueTarget)
    {
        double[] target = { valueTarget };
        criticNet.FeedForward(states);
        criticNet.Backpropagation(target);
    }
    // Train Actor Net
    private void UpdateActor(double[] states, double[] targets)
    {
        actorNet.FeedForward(states);
        actorNet.Backpropagation(targets);
    }
}
