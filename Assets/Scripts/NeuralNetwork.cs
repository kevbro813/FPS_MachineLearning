using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NeuralNetwork : IComparable<NeuralNetwork>
{
    public int[] neuralLayers; // Contains the number of neurons in each layer
    public float[][] neuronsMatrix; // Contains all the neurons, organized in layers
    public float[][][] weightsMatrix; // Contains all weights, organized by layers of neurons
    public float fitness; // Float value to track the fitness of the current network
    public AIType aiType;
    public enum AIType { Random, Fittest, Children, Fit, Survivor, Saved }

    // Neural Network Constructor
    public NeuralNetwork(int[] layers) 
    {
        // The number of layers are passed into this method using another array.

        // Create an array of neural layers with the size of the layers array being passed in.
        this.neuralLayers = new int[layers.Length]; 

        // For each layer...
        for(int currentLayer = 0; currentLayer < layers.Length; currentLayer++) 
        {
            // Set the number of neurons in each layer to the int value being passed in via the layers array.
            this.neuralLayers[currentLayer] = layers[currentLayer];
        }

        InitializeNeurons(); // Create neuron matrix
        InitializeWeights(); // Create weight matrix
    }
    // Creates a Neuron Matrix [layer][neuron]
    private void InitializeNeurons()
    {
        // Create an empty neuron list matrix.
        List<float[]> neuronsList = new List<float[]>(); 

        for (int layer = 0; layer < neuralLayers.Length; layer++) // For each neural layer...
        {
            // Add a float array to represent each neuron. The number of neurons is determined by the value contained
            // in the current layer of the neuralLayers array.
            neuronsList.Add(new float[neuralLayers[layer]]);
        }

        neuronsMatrix = neuronsList.ToArray(); // Convert the list of neurons to an array.
    }

    // Creates a Weights Matrix [layer][neuron][weight]
    private void InitializeWeights()
    {
        // Create a new list to contain all the weights. The list is organized in layers, with each 2D float array 
        // representing a layer. All the layers are temporarily contained in this list, then converted to an array.
        List<float[][]> weightsList = new List<float[][]>(); 

        // Iterate through each layer... (start with first hidden layer because the input layer does not have weights)
        for (int wLayer = 1; wLayer < neuralLayers.Length; wLayer++) 
        {
            // Create a weight matrix for each layer.
            List<float[]> layerWeightsList = new List<float[]>(); 

            // Determine how many neurons there are in the previous layer. 
            // This represents the number of weights required for each neuron.
            int neuronsInPreviousLayer = neuralLayers[wLayer - 1];

            // For each neuron in the current layer...
            for (int neuron = 0; neuron < neuronsMatrix[wLayer].Length; neuron++) 
            {
                // Create a new float array for each neuron in the previous layer.
                float[] neuronWeights = new float[neuronsInPreviousLayer]; 

                // For each weight in the current neuron...
                for (int weight = 0; weight < neuronsInPreviousLayer; weight++)
                {
                    // Set the weight to a random value between -0.5 and 0.5.
                    neuronWeights[weight] = UnityEngine.Random.Range(-0.5f, 0.5f); 
                }

                // Add the neuron with new weights to the weight matrix for the current layer.
                layerWeightsList.Add(neuronWeights);
            }

            // Add the current layer to the list of all weights.
            weightsList.Add(layerWeightsList.ToArray());
        }

        // Convert the weights list to an array once all neurons with weights have been iterated through.
        weightsMatrix = weightsList.ToArray();
    }

    // Calculates neuron values given a set of inputs
    public float[] FeedForward(float[] inputs)
    {
        // Iterate through all the inputs and add them to the first layer (input layer) in the neuronsMatrix.
        for (int input = 0; input < inputs.Length; input++)
        {
            neuronsMatrix[0][input] = inputs[input];
        }

        // Iterate through each layer (excluding input layer).
        for (int layer = 1; layer < neuralLayers.Length; layer++)
        {
            // Iterate through each neuron in the current layer.
            for (int neuron = 0; neuron < neuronsMatrix[layer].Length; neuron++)
            {
                float neuronValue = 0f; // Create a new value for the neuron. Changing this value can set a static bias. 

                // Iterate through each neuron in the previous layer that the current neuron is connected to.
                for (int preNeuron = 0; preNeuron < neuronsMatrix[layer - 1].Length; preNeuron++)
                {
                    // Set neuronValue by adding it to the previous neuron's weight multiplied by the value in the previous neuron.
                    neuronValue += weightsMatrix[layer - 1][neuron][preNeuron] * neuronsMatrix[layer - 1][preNeuron];
                }

                // Applies Tanh function to give a value between -1 and 1. Max function can be used for ReLU
                neuronsMatrix[layer][neuron] = (float)Math.Tanh(neuronValue); //Math.Max(0, neuronValue);
            }
        }

        return neuronsMatrix[neuronsMatrix.Length - 1]; // Return the last layer
    }

    // Mutation function will apply a mutation to weight values based on chance.
    public void Mutate()
    {
        // Iterate through all the layers.
        for (int layer = 0; layer < weightsMatrix.Length; layer++)
        {
            // Iterate through all the neurons.
            for (int neuron = 0; neuron < weightsMatrix[layer].Length; neuron++)
            {
                // Iterate through all the weights.
                for (int weight = 0; weight < weightsMatrix[layer][neuron].Length; weight++)
                {
                    // Create a float weight and set it to the current weight.
                    float weightValue = weightsMatrix[layer][neuron][weight]; 

                    float randomNumber = UnityEngine.Random.Range(0f, 1000f); // Generate a random number

                    // **Mutations**
                    if (randomNumber <= 2f) // Mutation #1 (0.2% chance)
                    {
                        weightValue *= -1f; // Flip the sign
                    }
                    else if (randomNumber <= 4f) // Mutation #2 (0.2% chance)
                    {

                        weightValue = UnityEngine.Random.Range(-0.5f, 0.5f); // Find another weight between -0.5 and 0.5
                    }
                    else if (randomNumber <= 6f) // Mutation #3 (0.2% chance)
                    {
                        float factor = UnityEngine.Random.Range(0f, 1f) + 1f; // Increase weight by 0% - 100%
                        weightValue *= factor;
                    }
                    else if (randomNumber <= 8f) // Mutation #4 (0.2% chance)
                    {
                        float factor = UnityEngine.Random.Range(-1f, 0f) - 1f; // Decrease weight by 0% - 100%
                        weightValue *= factor;
                    }
                    weightsMatrix[layer][neuron][weight] = weightValue; // Set the current weight to the mutated weight.
                }
            }
        }
    }
    public void SexualReproduction(NeuralNetwork netA, NeuralNetwork netB, NeuralNetwork netC)
    {
        // Pass in two neural nets
        // Iterate through all the layers.
        for (int layer = 0; layer < netC.weightsMatrix.Length; layer++)
        {
            // Iterate through all the neurons.
            for (int neuron = 0; neuron < netC.weightsMatrix[layer].Length; neuron++)
            {
                // Iterate through all the weights.
                for (int weight = 0; weight < netC.weightsMatrix[layer][neuron].Length; weight++)
                {
                    // Create a float weight and set it to the current weight.
                    float weightValue = netC.weightsMatrix[layer][neuron][weight];

                    float randomNumber = UnityEngine.Random.Range(0f, 1000f); // Generate a random number

                    // Reproduction
                    if (randomNumber < 500) // Parent A passes on weight
                    {
                        Debug.Log("Passed Parent A");
                        // Net A weight
                        weightValue = netA.weightsMatrix[layer][neuron][weight];
                    }
                    else if (randomNumber <= 1000) // Parent B passes on weight
                    {
                        Debug.Log("Passed Parent B");
                        // Net B weight
                        weightValue = netB.weightsMatrix[layer][neuron][weight];
                    }

                    netC.weightsMatrix[layer][neuron][weight] = weightValue; // Set the weight
                }
            }
        }
    }
    // Creates a deep copy of the neural network
    public NeuralNetwork(NeuralNetwork copyNetwork)
    {
        // Create a new array with the number of neurons in each layer
        this.neuralLayers = new int[copyNetwork.neuralLayers.Length];

        // For each layer...
        for (int layer = 0; layer < copyNetwork.neuralLayers.Length; layer++)
        {
            // Set the number of neurons in each layer to the int value being passed in 
            // via the copyNetwork.neuralLayers array.
            this.neuralLayers[layer] = neuralLayers[layer];
        }

        InitializeNeurons(); // Create a neuronsMatrix
        InitializeWeights(); // Create a weightsMatrix
        CopyWeights(copyNetwork.weightsMatrix); // Copy the copyNetwork's weightMatrix
    }

    // Copies the weights being passed into the funciton.
    private void CopyWeights(float[][][] copyWeights)
    {
        // Iterate through each layer.
        for (int layer = 0; layer < weightsMatrix.Length; layer++)
        {
            // Iterate through each neuron.
            for (int neuron = 0; neuron < weightsMatrix[layer].Length; neuron++)
            {
                // Iterate through each weight.
                for (int weight = 0; weight < weightsMatrix[layer][neuron].Length; weight++)
                {
                    // Set the current weight to the corresponding copy network weight.
                    weightsMatrix[layer][neuron][weight] = copyWeights[layer][neuron][weight];
                }
            }
        }
    }
   
    // Adjust Fitness
    public void AddFitness(float fit)
    {
        fitness += fit;
    }

    // Set Fitness for Neural Net
    public void SetFitness(float fit)
    {
        fitness = fit;
    }
    // Return the Neural Net's Fitness
    public float GetFitness()
    {
        return fitness;
    }
    // Sort neural networks with most fit at top
    public int CompareTo(NeuralNetwork other)
    {
        if (other == null) return 1;

        if (fitness > other.fitness)
            return 1;
        else if (fitness < other.fitness)
            return -1;
        else
            return 0;
    }
    // Set the AI type (Random, Survivor, Fit, Fittest)
    public void SetType(AIType type)
    {
        aiType = type;
    }
}
