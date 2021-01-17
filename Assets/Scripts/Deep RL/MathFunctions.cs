using System;
using UnityEngine;

[Serializable]
public class MathFunctions
{
    #region Activation Functions
    // Activation Functions
    public double Relu(double value) // Rectified Linear Units
    {
        return Math.Max(0, value);
    }
    public double LeakyRelu(double value) // Leaky Rectified Linear Units
    {
        return (0 >= value) ? 0.01d * value : value;
    }
    public double Sigmoid(double value) // Sigmoid
    {
        return 1 / (1 + Math.Exp(-value));
    }
    public double Tanh(double value) // Hyperbolic Tangent
    {
        return Math.Tanh(value);
    }
    public double[] Softmax(double[] actions) // Softmax (Requires an array of data since it creates a probability distribution over all elements)
    {
        // Initialize arrays
        double[] softAction = new double[actions.Length];
        double[] expAction = new double[actions.Length];
        double sum = 0.0d; // Used to track sum
        double max = Max(actions); // Returns the max action probability

        // Softmax calculation = e^zi / sum(e^zj)
        for (int i = 0; i < actions.Length; i++)
        {
            expAction[i] = Math.Exp(actions[i] - max); // Subtracting the action prob by the max action creates more stability
            sum += expAction[i]; // Calculate sum
        }

        for (int i = 0; i < actions.Length; i++)
            softAction[i] = expAction[i] / sum; // Calculate softmax output for each action

        return softAction; // Return array with softmax outputs
    }
    #endregion

    #region Activation Function Derivatives
    // Activation Function Derivatives
    public double ReluDerivative(double value) // Rectified Linear Unit Derivative
    {
        return 0 >= value ? 0 : 1;
    }
    public double LeakyReluDerivative(double value) // Leaky Rectified Linear Unit Derivative
    {
        return 0 >= value ? 0.01d : 1;
    }
    public double SigmoidDerivative(double value) // Sigmoid Derivative
    {
        return value * (1 - value);
    }

    public double TanhDerivative(double value) // Tanh Derivative
    {
        return 1 - (value * value);
    }
    public double SoftmaxDerivative(double value) // Softmax Derivative 
    {
        /* Note this is only the derivative for the output on the one hot encoded vector.
         All other vectors (AKA encoded to "0") will just multiply error by output since
         this is the same as value - 0.*/
        return value - 1; 
    }
    #endregion

    #region Max Functions
    /// <summary>
    ///  Returns index of max action value
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public int ArgMax(double[] action)
    {
        int argMax = 0;

        for (int i = 1; i < action.Length; i++) // Loop through each action
        {
            if (action[argMax] < action[i]) // Compare two indices to find max action
                argMax = i; // Set argmax to the new max index, otherwise keep the argmax the same this iteration
        }
        return argMax;
    }
    /// <summary>
    /// Returns the value of the max action.
    /// </summary>
    /// <param name="actions"></param>
    /// <returns></returns>
    public double Max(double[] actions)
    {
        double maxAct = 0;
        foreach (double act in actions) // Iterate through each action
        {
            if (act > maxAct) // Compare actions
                maxAct = act; // If act is greater than maxAct, set maxAct to act
        }
        return maxAct;
    }
    /// <summary>
    /// Returns the strongest action, the rest will be set to 0. Used for selected_Actions and QValues
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public double[] Amax(double[] action)
    {
        double[] sAction = new double[action.Length];
        int indexMax = 0;

        for (int i = 1; i < action.Length; i++) // Loop through each action
        {
            if (action[indexMax] > action[i]) // If action is less than the current max...
            {
                sAction[indexMax] = action[indexMax]; // Set sAction to the greater value at the respective index
                sAction[i] = 0; // Set the smaller value to zero at the respective index
            }
            else // if the action is greater than the current max...
            {
                sAction[indexMax] = 0; // Set the current max to zero
                sAction[i] = action[i]; // Set the max action at the respective index
                indexMax = i; // Update to track the index of the maximum value
            }
        }
        return sAction;
    }
    #endregion

    #region OneHot Encoding
    /// <summary>
    /// Multiply actions by a one hot encoded vector. This sets all actions to zero, except the greatest action which retains its value.
    /// </summary>
    /// <param name="oneHot"></param>
    /// <param name="actions"></param>
    /// <returns></returns>
    public double[] OneHotProduct(double[] oneHot, double[] actions)
    {
        double[] product = new double[actions.Length];

        for (int i = 0; i < actions.Length; i++) // Loop through each action
            product[i] = oneHot[i] * actions[i]; // Multiply action value by one hot vector

        return product;
    }
    /// <summary>
    /// One hot encode an array of actions. ONLY USE THIS IF ACTION TAKEN IS HIGHEST PROBABILITY ACTION. i.e. This will not work well with PPO since the action taken
    /// is not necessarily the most probable action. There is a much simpler/efficient way to calculate this if the action taken is already known. Just Initialize a new 
    /// array and set the element of the taken action to 1. *See RunPPO() in RLComponent.
    /// </summary>
    /// <param name="actions"></param>
    /// <returns></returns>
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
    #endregion

    #region Log Probabilities
    /// <summary>
    /// Calculate the negative of the log probabilities
    /// </summary>
    /// <param name="probs"></param>
    /// <returns></returns>
    public double[] LogProbs(double[] probs)
    {
        double[] LogProbs = new double[probs.Length];

        for (int i = 0; i < probs.Length; i++)
            LogProbs[i] = -1 * Math.Log(probs[i]); // Not to be confused with Log(1/prob) which is also referred to as the negative log, this is probably better described as the negative of the log probability

        return LogProbs;
    }
    #endregion

    #region Normalization, Variance, Mean and Standard Deviation Functions
    /// <summary>
    /// Normalizes a set of data using x = (x - mean(x)) / stdDev(x)
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public double[] Normalize(double[] data)
    {
        double[] normalized = new double[data.Length]; // Stores normalized data
        double mean = Mean(data); // Calculate the mean of the array
        double totalSquaredDifference = 0; // Track total squared difference

        for (int i = 0; i < data.Length; i++)
        {
            totalSquaredDifference += SquaredDifference(data[i], mean); // Important to calculate before subtracting by mean
            normalized[i] = data[i] - mean; // Subtract the value by the mean
        }

        double variance = Variance(totalSquaredDifference, normalized.Length); // Calculate the variance
        double stdDeviation = StdDeviation(variance); // Calculate standard deviation

        for (int i = 0; i < data.Length; i++)
            normalized[i] /= (stdDeviation + 1e-10); // Normalize the values by dividing by the standard deviation. A small value is added to avoid dividing by 0.

        return normalized;
    }
    /// <summary>
    /// Calculate and return the squared difference, used to calculate variance.
    /// </summary>
    /// <param name="dataPoint"></param>
    /// <param name="mean"></param>
    /// <returns></returns>
    public double SquaredDifference(double dataPoint, double mean)
    {
        double diff = dataPoint - mean;
        double sqrdDiff = diff * diff;
        return sqrdDiff;
    }
    /// <summary>
    /// Calculate and return the variance.
    /// </summary>
    /// <param name="totalSqrdDiff"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public double Variance(double totalSqrdDiff, double count)
    {
        double variance = totalSqrdDiff / count;
        return variance;
    }
    /// <summary>
    /// Calculate and return the standard deviation using the variance.
    /// </summary>
    /// <param name="variance"></param>
    /// <returns></returns>
    public double StdDeviation(double variance)
    {
        double stdDev = Math.Sqrt(variance);
        return stdDev;
    }
    /// <summary>
    /// Returns the mean of an array of values
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public double Mean(double[] values)
    {
        double total = 0; // Variable to hold total loss, used to average

        // Calculate average the losses for each action
        for (int i = 0; i < values.Length; i++)
            total += values[i]; // Calculate total

        return total / values.Length; // Return average
    }
    /// <summary>
    /// Recalculates the mean by including the newest datapoint.
    /// </summary>
    /// <param name="sampleSize"></param>
    /// <param name="oldMean"></param>
    /// <param name="dp"></param>
    /// <returns></returns>
    public double UpdateMean(int sampleSize, double oldMean, double dp)
    {
        double newMean;
        if (sampleSize == 1) // If sample size = 1 then the mean is equal to the datapoint
            newMean = dp;
        else
        {
            double sampleTotal = oldMean * (sampleSize - 1); // Calculate the total from the old mean
            sampleTotal += dp; // Add the new datapoint to the sample total
            newMean = sampleTotal / sampleSize; // Recalculate the mean
        }
        return newMean;
    }
    /// <summary>
    /// Scalar function (calculates zScore)
    /// </summary>
    /// <param name="dp"></param>
    /// <param name="mean"></param>
    /// <param name="stdDev"></param>
    /// <returns></returns>
    public double ZScore(double dp, double mean, double stdDev)
    {
        // Calculate a state's Z-score = (data point - mean) / standard deviation
        double zScore = (dp - mean) / (stdDev + 1e-10);
        return zScore;
    }
    #endregion
}
