using System;
using UnityEngine;

[Serializable]
public class MathFunctions
{
    // Activation Functions
    public double Relu(double value)
    {
        return Math.Max(0, value);
    }

    public double LeakyRelu(double value)
    {
        return (0 >= value) ? 0.01d * value : value;
    }

    public double Sigmoid(double value)
    {
        return 1 / (1 + Math.Exp(-value));
    }

    public double Tanh(double value)
    {
        return Math.Tanh(value);
    }

    // Activation Function Derivatives
    public double ReluDerivative(double value)
    {
        return 0 >= value ? 0 : 1;
    }
    public double LeakyReluDerivative(double value)
    {
        return 0 >= value ? 0.01d : 1;
    }
    public double SigmoidDerivative(double value)
    {
        return value * (1 - value);
    }

    public double TanhDerivative(double value)
    {
        return 1 - (value * value);
    }
    // Softmax action outputs
    public double[] Softmax(double[] actions)
    {
        double[] softAction = new double[actions.Length];
        double[] expAction = new double[actions.Length];
        double sum = 0.0d;
        double max = Max(actions);

        for (int i = 0; i < actions.Length; i++)
        {
            expAction[i] = Math.Exp(actions[i] - max);
            sum += expAction[i];
        }

        for (int i = 0; i < actions.Length; i++)
        {
            softAction[i] = expAction[i] / sum;
        }

        return softAction;
    }
    // Returns index of max action value
    public int ArgMax(double[] action)
    {
        int argMax = 0;

        for (int i = 1; i < action.Length; i++) // Loop through each action
        {
            if (action[argMax] < action[i])
            {
                argMax = i;
            }
        }
        return argMax;
    }
    public double Max(double[] actions)
    {
        double maxAct = 0;
        foreach (double act in actions)
        {
            if (act > maxAct)
            {
                maxAct = act;
            }
        }
        return maxAct;
    }
    // Softmax Derivative
    public double SoftmaxDerivative(double value)
    {
        return value * (1 - value);
    }
    public double AverageCost(RLComponent rl, double[] costs)
    {
        double avgCost; // Average loss for each action
        double totLoss = 0; // Variable to hold total loss, used to average

        // Calculate average the losses for each action
        for (int i = 0; i < rl.actionQty; i++)
        {
            totLoss += costs[i]; // Sum costs of 
        }
        avgCost = totLoss / rl.actionQty; // Calculate average loss
        return avgCost;
    }
    // Returns the strongest action, the rest will be set to 0. Used for selected_Actions and QValues
    public double[] Amax(double[] action)
    {
        double[] sAction = new double[action.Length];
        int indexMax = 0;

        for (int i = 1; i < action.Length; i++) // Loop through each action
        {
            if (action[indexMax] > action[i]) // If the 
            {
                sAction[indexMax] = action[indexMax];
                sAction[i] = 0;
            }
            else
            {
                sAction[indexMax] = 0;
                sAction[i] = action[i];
                indexMax = i;
            }
        }
        return sAction;
    }
    public double[] OneHotProduct(double[] oneHot, double[] actions)
    {
        double[] product = new double[actions.Length];

        for (int i = 0; i < actions.Length; i++)
        {
            product[i] = oneHot[i] * actions[i];
        }
        return product;
    }
    public double[] OneHot(double[] actions)
    {
        double[] oneHotActions = new double[actions.Length];
        int maxIndex = 0;

        for (int i = 1; i < actions.Length - 1; i++) // Loop through each action
        {
            if (actions[maxIndex] > actions[i]) // If the action 
            {
                oneHotActions[maxIndex] = 1;
                oneHotActions[i] = 0;
            }
            else
            {
                oneHotActions[maxIndex] = 0;
                oneHotActions[i] = 1;
                maxIndex = i;
            }
        }
        return oneHotActions;
    }
    // Normalize values
    public double Normalize(double data, int count, bool isMainNet, int lay, int node, double[][] mainMeans, double[][] mainVariances, double[][] targetMeans, double[][] targetVariances)
    {
        double totalSqrdDiff;
        double stdDev;
        if (isMainNet)
        {
            mainMeans[lay][node] = UpdateMean(count, mainMeans[lay][node], data);
            // Calculate the squared difference for the new datapoint
            double sqrdDiff = SquaredDifference(data, mainMeans[lay][node]);
            if (count == 1)
            {
                totalSqrdDiff = sqrdDiff;
            }
            else
            {
                // Calculate total squared difference from variance
                totalSqrdDiff = mainVariances[lay][node] * count;

                // Update the total squared difference
                totalSqrdDiff += sqrdDiff;
            }
            // Calculate Variance and Standard Deviation
            mainVariances[lay][node] = Variance(totalSqrdDiff, count);

            stdDev = StdDeviation(mainVariances[lay][node]);

            return ZScore(data, mainMeans[lay][node], stdDev);
        }
        else
        {
            targetMeans[lay][node] = UpdateMean(count, targetMeans[lay][node], data);
            // Calculate the squared difference for the new datapoint
            double sqrdDiff = SquaredDifference(data, targetMeans[lay][node]);
            if (count == 1)
            {
                totalSqrdDiff = sqrdDiff;
            }
            else
            {
                // Calculate total squared difference from variance
                totalSqrdDiff = targetVariances[lay][node] * count;

                // Update the total squared difference
                totalSqrdDiff += sqrdDiff;
            }

            // Calculate Variance and Standard Deviation
            targetVariances[lay][node] = Variance(totalSqrdDiff, count);

            stdDev = StdDeviation(targetVariances[lay][node]);

            // Normalize the current state values
            return ZScore(data, targetMeans[lay][node], stdDev);
        }
    }
    // Recalculates the mean by including the newest datapoint
    public double UpdateMean(int sampleSize, double mean, double dp)
    {
        if (sampleSize == 1) // If sample size = 1 then the mean is equal to the datapoint
        {
            mean = dp;
        }
        else
        {
            double sampleTotal = mean * (sampleSize - 1); // Calculate the total from the old mean
            sampleTotal += dp; // Add the new datapoint to the sample total
            mean = sampleTotal / sampleSize; // Recalculate the mean
        }
        return mean;
    }
    // Find the squared difference, used to calculate variance
    public double SquaredDifference(double dp, double mean)
    {
        double diff = dp - mean;
        double sqrdDiff = diff * diff;
        return sqrdDiff;
    }
    // Calculate the variance
    public double Variance(double totalSqrdDiff, double count)
    {
        double variance = totalSqrdDiff / count;
        return variance;
    }
    // Calculate the standard deviation
    public double StdDeviation(double variance)
    {
        double stdDev = Math.Sqrt(variance);
        return stdDev;
    }
    // Scalar function (calculates zScore)
    public double ZScore(double dp, double mean, double stdDev)
    {
        // Calculate a state's Z-score = (data point - mean) / standard deviation
        double zScore = (dp - mean) / (stdDev + .00001);

        return zScore;
    }
}
