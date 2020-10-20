using System;
using System.Collections;
using System.Collections.Generic;
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
    private double entropyLoss = 0.0001d; // TODO: Add to settings
    public double ppoLoss;
    private double gamma;
    private int batchSize;

    public double[] discountRewards;
    public double[] rewards;
    public int[][] actions;
    public double[][] predictions;
    public bool[] dones;
    private double[] advantages;
    private double[] probabilities;
    private double[] oldProbabilities;
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
        ppoLoss = 0;
        gamma = RLManager.instance.settings.gamma;
        batchSize = RLManager.instance.settings.epiMaxSteps; // PPO trains on each epoch of training data after every episode. Therefore, the batch size will be the episode length.

        // Initialize arrays
        discountRewards = new double[batchSize];
        rewards = new double[batchSize];
        actions = new int[batchSize][];
        predictions = new double[batchSize][];
        dones = new bool[batchSize];
        advantages = new double[batchSize];

        probabilities = new double[actionQty];
        oldProbabilities = new double[actionQty];
        ratios = new double[actionQty];
        p1 = new double[actionQty];
        p2 = new double[actionQty];
        actorTargets = new double[actionQty];
    }

    private void ActorTargets(double[] state, int batchIndex)
    {
        // Use actor network to calculate y Prediction
        double[] yPred = actorNet.FeedForward(state);

        // Use critic network to calculate stateValue
        double stateValue = criticNet.FeedForward(state)[0];

        // Calculate probabilities
        for (int i = 0; i < actionQty; i++)
        {
            probabilities[i] = yPred[i] * actions[batchIndex][i]; // prob = actions * prediction_picks
            oldProbabilities[i] = actions[batchIndex][i] * predictions[batchIndex][i]; // old_prob = actions * prediction_picks
        }

        // Calculate ratios
        for (int i = 0; i < actionQty; i++)
        {
            ratios[i] = probabilities[i] / (oldProbabilities[i] + 1e-10); // ratios = probabilities / (old_probabilities + 1e-10)
        }

        // Compute Advantages
        advantages[batchIndex] = discountRewards[batchIndex] - stateValue; // advantages = discountedRewards - value;

        for (int i = 0; i < actionQty; i++)
        {
            p1[i] = ratios[i] * advantages[batchIndex]; // p1 = ratios * advantages
            p2[i] = ClampRatio(ratios[i]) * advantages[batchIndex]; // p2 = clippedRatio * advantages
        }
        
        double total = 0;

        // loss = -mean(minimum(p1, p2) + entropyLoss * -(prob * K.log(prob + 1e-10)))
        for (int i = 0; i < actionQty; i++)
        {
            actorTargets[i] = -Math.Min(p1[i], p2[i]) + entropyLoss * -(probabilities[i] * Math.Log(probabilities[i] + 1e-10));
            total += actorTargets[i];
        }

        ppoLoss = total / actionQty;
    }
    public double PPOReplay()
    {
        UnpackBatch(agent.ppoExperienceBuffer); // Unpack Tuple with batch data

        // Compute Discounted Rewards 
        DiscountRewards();

        for (int i = env.framesPerState - 1; i < batchSize; i++)
        {
            double[] state = env.GetState(i);
            ActorTargets(state, i); // TODO: Check
            UpdateActor(state, actorTargets); // TODO: Check
            UpdateCritic(state, discountRewards[i]); // TODO: Check
        }
        return ppoLoss;
    }
    public void DiscountRewards()
    {
        double discount = 0;
        discountRewards = new double[batchSize];
        for (int i = rewards.Length - 1; i >= 0; i--)
        {
            if (dones[i])
            {
                discount = 0;
            }

            discount = discount * gamma + rewards[i];
            discountRewards[i] = discount;
        }

        NormalizeDiscountRewards();
    }
    private void NormalizeDiscountRewards()
    {
        double avgDiscountReward = AverageDiscountRewards();
        double totalSquaredDifference = 0;
        for (int i = 0; i < discountRewards.Length; i++)
        {
            discountRewards[i] -= avgDiscountReward;
            totalSquaredDifference += RLManager.math.SquaredDifference(discountRewards[i], avgDiscountReward);
        }

        double variance = RLManager.math.Variance(totalSquaredDifference, discountRewards.Length);
        double stdDeviation = RLManager.math.StdDeviation(variance);

        for (int i = 0; i < discountRewards.Length; i++)
        {
            discountRewards[i] /= stdDeviation;
        }
    }
    private double AverageDiscountRewards()
    {
        double total = 0;
        for (int i = 0; i < discountRewards.Length; i++)
        {
            total += discountRewards[i];
        }

        return total / discountRewards.Length;
    }
    // Clamp Ratios
    private double ClampRatio(double ratio)
    {
        double value = (double)Mathf.Clamp((float)ratio, clipMinimum, clipMaximum);

        return value;
    }
    private void UnpackBatch(Tuple<int, double, double[], bool>[] batch)
    {
        for (int i = 0; i < batchSize; i++)
        {
            int[] actionOneHot = new int[actionQty];
            actionOneHot[batch[i].Item1] = 1;
            actions[i] = actionOneHot;
            rewards[i] = batch[i].Item2;
            predictions[i] = batch[i].Item3;
            dones[i] = batch[i].Item4;
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
