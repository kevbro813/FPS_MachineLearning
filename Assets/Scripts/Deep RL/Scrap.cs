using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scrap : MonoBehaviour
{

    //        // Calculate signals of last neuron layer (Error * Derivative)
    //        for (int neu = 0; neu<signals[signals.Length - 1].Length; neu++) // Loop through each neuron in the last signal layer
    //        {
    //            signals[signals.Length - 1][neu] = errors[neu] * OutputDerivative(actions[i][neu], dqn.outputActivation); // Set the signal value
    //}

    //        // Calculate gradients of last weights layer (last layer signal * previous layer neuron output)

    //        // Loop through each neuron in the last layer (represents the outbound connection of a weight in the last layer, and is how they are grouped)
    //        for (int neu = 0; neu<grads[grads.Length - 1].Length; neu++)
    //        {
    //            // Loop through all the weights (each weight connects to a neuron in the previous layer, this is how the weights are indexed according to the previous neuron it is attached to)
    //            for (int pn = 0; pn<grads[grads.Length - 1][neu].Length; pn++)
    //            {
    //                grads[grads.Length - 1][neu][pn] = neurons[neurons.Length - 2][pn] * signals[signals.Length - 1][neu]; // Gradient = (neuron output for current layer) * (next layer's signal)
    //            }
    //        }
    //        for (int bias = 0; bias<biasGrads[biasGrads.Length - 1].Length; bias++)
    //        {
    //            biasGrads[biasGrads.Length - 1][bias] = signals[signals.Length - 1][bias];
    //        }
    //        // Continue with all other layers

    //        // Calculate signals
    //        for (int lay = signals.Length - 2; lay > 0; lay--) // Iterate through all the signal layers beginning with second to last, stop on index 1 because index 0 signals are not required (this is the input layer)
    //        {

    //            for (int neu = 0; neu<signals[lay + 1].Length; neu++) // Iterate through each node in the signal layer
    //            {
    //                double sum = 0.0;
    //                // To calculate the sum of all 
    //                for (int pn = 0; pn<parameters[l].Length; pn++) // Iterate through all the previous neurons
    //                {
    //                    for (int nn = 0; nn<signals[lay + 1].Length; nn++) // Iterate through the neurons in the next layer (Represents outbound weight/node pairs)
    //                    {
    //                        // Sum the products of the next layer signal "layer + 1" with the corresponding weight. The sum is used to calculate the signals in the current "layer"
    //                        sum += parameters[l][nn][pn] * signals[lay + 1][nn];
    //                    }
    //                }
    //                signals[lay][neu] = HiddenDerivative(neurons[lay][neu], dqn.hiddenActivation) * sum; // Multiply the sum by the derivative to get the node signal
    //            }
    //            l--; // decrement l for next layer iteration
    //        }
    //        // Calculate weight gradients
    //        for (int lay = grads.Length - 2; lay >= 0; lay--) // Begin with the second to last gradient layer. Important to include gradients for layer 0 with the >=.
    //        {
    //            for (int neu = 0; neu<grads[lay].Length; neu++) // Iterate through all the neurons in the gradient layer
    //            {
    //                for (int pn = 0; pn<grads[lay][neu].Length; pn++) // Iterate through all the previous neurons which correspond to a weight
    //                {
    //                    grads[lay][neu][pn] = neurons[m - 1][pn] * signals[m][neu]; // Gradient is the product of the neuron's output and the signal from the next layer (the two ends a weight is connected to)    
    //                }
    //            }
    //            m--; // Decrement neuron/signal layer
    //        }
    //        for (int lay = biasGrads.Length - 2; lay > 0; lay--)
    //        {
    //            for (int bias = 0; bias<biasGrads[lay].Length; bias++)
    //            {
    //                biasGrads[lay][bias] = signals[lay][bias];
    //            }
    //        }

    //        biasGradients = biasGrads;

    //        return grads;

    //if(isMainNet == true)
    //{
    //    dqn.mainSteps++;
    //}
    //else
    //{
    //    dqn.targetSteps++;
    //}

    //if (isMainNet)
    //{
    //    mainNormalized[0][ipt] = Normalize(inputs[ipt], dqn.mainSteps, isMainNet, 0, ipt);
    //}
    //else
    //{
    //    targetNormalized[0][ipt] = Normalize(inputs[ipt], dqn.targetSteps, isMainNet, 0, ipt);
    //}

    //double normNode;
    //if (isMainNet)
    //{
    //    normNode = mainNormalized[lay - 1][wt];
    //}
    //else
    //{
    //    normNode = targetNormalized[lay - 1][wt];
    //}
    //neuronValue += weights[lay - 1][neu][wt] * normNode;

    //nSums[lay][neu] = neuronValue;

    //if (isMainNet)
    //{
    //    mainNormalized[lay][neu] = Normalize(nActs[lay][neu], dqn.mainSteps, isMainNet, lay, neu) + biases[lay][neu];
    //}
    //else
    //{
    //    targetNormalized[lay][neu] = Normalize(nActs[lay][neu], dqn.targetSteps, isMainNet, lay, neu) + biases[lay][neu];
    //}
}
