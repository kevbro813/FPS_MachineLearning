using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Experimental : MonoBehaviour
{
    //public void AvgErrors()
    //{
    //    for (int i = 0; i < actionQty; i++)
    //    {
    //        double errorSum = 0;
    //        for (int j = 0; j < miniBatchSize; j++)
    //        {
    //            errorSum += errors[j][i];
    //        }
    //    }
    //}
    //public void SetNodeDeltas()
    //{
    //    for (int neuron = 0; neuron < dqn.layers[dqn.layers.Length - 1]; neuron++)
    //    {
    //        float deriv;
    //        if (dqn.mainNet.nodeSums[dqn.layers.Length - 1][neuron] > 0)
    //        {
    //            deriv = 1;
    //        }
    //        else
    //        {
    //            deriv = 0;
    //        }
    //        dqn.mainNet.nodeDeltas[dqn.layers.Length - 1][neuron] = -errors[dqn.layers.Length - 1][neuron] * deriv;
    //    }
    //}
    //public double[][][] SetGradients(double[][][] parameters, double[][] neurons, double[][][] gradients, double[][] errors, int layer)
    //{
    //    for (int neuron = 0; neuron < gradients[layer].Length; neuron++)
    //    {
    //        for (int weight = 0; weight < gradients[layer][neuron].Length; weight++)
    //        {
    //            for (int pNeuron = 0; pNeuron < neurons[layer - 1].Length; pNeuron++)
    //            {
    //                double pNeuronValue = neurons[layer - 1][pNeuron];
    //                gradients[layer][neuron][weight] = pNeuronValue * dqn.mainNet.nodeDeltas[layer][neuron];
    //            }
    //        }
    //    }

    //    return gradients;
    //}
    //// TODO: Softmax Activation function as an option rather than sigmoid or reLu
    //public double[] SoftmaxAction(double[] action)
    //{
    //    double[] softAction = new double[actionQty];

    //    return softAction;
    //}
    //// Normalize the "state" that is passed to the function
    //public float[] NormalizeState(float[] means, float[] states, float counter, float[] variances, float[] stdDevs) // TODO: Should be normalize frame, need to change all states array to frame 
    //{
    //    float[] normalizedStates = new float[states.Length];

    //    // Iterate scalar function through each state input
    //    for (int i = 0; i < states.Length; i++)
    //    {
    //        // Update the mean with the new datapoint
    //        //means[i] = UpdateMean(counter, means[i], states[i]);

    //        // Calculate the squared difference for the new datapoint
    //        float sqrdDiff = SquaredDifference(states[i], means[i]);

    //        // Calculate total squared difference from variance
    //        float totalSqrdDiff = variances[i] * counter;

    //        // Update the total squared difference
    //        totalSqrdDiff += sqrdDiff;

    //        counter++; // TODO: Is this incremented correctly?

    //        // Recalculate Variance and Standard Deviation
    //        variances[i] = Variance(totalSqrdDiff, counter);
    //        stdDevs[i] = StdDeviation(variances[i]);

    //        // Normalize the current state values
    //        normalizedStates[i] = ZScore(states[i], means[i], stdDevs[i]);
    //    }

    //    return normalizedStates;
    //}
    //// Scalar function (calculates zScore)
    //public float ZScore(float dp, float mean, float stdDev)
    //{
    //    // Calculate a state's Z-score = (data point - mean) / standard deviation
    //    float zScore = (dp - mean) / stdDev;

    //    return zScore;
    //}
    //// Recalculates the mean by including the newest datapoint
    //public double UpdateMean(int sampleSize, double mean, double dp)
    //{
    //    double sampleTotal = sampleSize * mean;
    //    sampleTotal += dp;
    //    sampleSize++;
    //    mean = sampleTotal / sampleSize;

    //    return mean;
    //}
    //// Find the squared difference, used to calculate variance
    //public float SquaredDifference(float dp, float mean)
    //{
    //    float sqrdDiff = Mathf.Pow(dp - mean, 2);
    //    return sqrdDiff;
    //}
    //// Calculate the variance
    //public float Variance(float totalSqrdDiff, float stateCounter)
    //{
    //    float variance = totalSqrdDiff / stateCounter;
    //    return variance;
    //}
    //// Calculate the standard deviation
    //public float StdDeviation(float variance)
    //{
    //    float stdDev = Mathf.Sqrt(variance);
    //    return stdDev;
    //}

    //for (int layer = 1; layer < neuralLayers.Length; layer++)
    //{
    //    // Iterate through each neuron in the current layer.
    //    for (int neuron = 0; neuron < neuronsMatrix[layer].Length; neuron++)
    //    {
    //        double neuronValue = 0f; // Create a new value for the neuron. Changing this value can set a static bias. 

    //        // Iterate through each neuron in the previous layer that the current neuron is connected to.
    //        for (int pNeuron = 0; pNeuron < neuronsMatrix[layer - 1].Length; pNeuron++)
    //        {
    //            // Set neuronValue by adding it to the previous neuron's weight multiplied by the value in the previous neuron.
    //            neuronValue += weightsMatrix[layer - 1][neuron][pNeuron] * neuronsMatrix[layer - 1][pNeuron];
    //        }

    //        nodeSums[layer][neuron] = neuronValue; // Save the raw sums before using the activation function (this is used to calculate node deltas)
    //    }
    //}
}
