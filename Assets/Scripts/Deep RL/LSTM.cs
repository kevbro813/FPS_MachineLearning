using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
/// <summary>
/// UNDER CONSTRUCTION
/// </summary>
public class LSTM
{
    private int inputQty; // Number of inputs to the layer
    private int outputQty; // Number of outputs from the layer


    public double[] outputs; // Array to store outputs (After activation function)
    private double[] inputs; // Array to store the inputs (values passed into the layer)
    private double[] cellStates;
    private double[] stateVariables;
    private double[] prevStateTotal;

    private double[][] inputWeights;
    private double[][] cellWeights;

    public Settings.LayerActivations activation; // The activation function used on the current layer
    // Initialize
    public LSTM()
    {
        inputWeights = new double[inputQty][];
        cellWeights = new double[inputQty][];
        // Initialize random weights
        for (int i = 0; i < inputQty; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                inputWeights[i][j] = (double)UnityEngine.Random.Range(-0.5f, 0.5f);
                cellWeights[i][j] = (double)UnityEngine.Random.Range(-0.5f, 0.5f);
            }
        }
    }
    // LSTM Cell
    public void LSTMCellForward(double[] input, double[] prevCellOutput, double[] prevCellState)
    {
        // Concatenate Input and previous cell output
        double[] concat = Concatenate(input, prevCellOutput);

        // Input (Pass through tanh)
        double[] inputs = InputLayer(concat);

        // Input Gate = Sigmoid(concat) * Tanh(concat)
        double[] inputGateOutput = InputGate(concat, inputs);

        // Forget Gate = Sigmoid 
        double[] forgetOutputs = ForgetGate(concat);

        // Add input gate output to state variable
        prevStateTotal = AddStateVariable(inputGateOutput, stateVariables);

        // Output Gate


    }

    private double[] ForgetState(double[] concat)
    {
        return new double[concat.Length];
    }
    private double[] AddStateVariable(double[] inputGate, double[] stateVariable)
    {
        double[] funcOutput = new double[inputGate.Length];

        for (int i = 0; i < inputGate.Length; i++)
        {
            funcOutput[i] = inputGate[i] + stateVariable[i];
        }
        return funcOutput;
    }
    private double[] InputGate(double[] concIn, double[] ins)
    {
        double[] gateOutput = new double[concIn.Length];
        for (int i = 0; i < concIn.Length; i++)
        {
            gateOutput[i] = RLManager.math.Sigmoid(concIn[i]);
            gateOutput[i] *= ins[i];
        }
        return gateOutput;
    }
    private double[] InputLayer(double[] concIn)
    {
        double[] inputs = new double[concIn.Length];
        for (int i = 0; i < concIn.Length; i++)
        {
            inputs[i] = RLManager.math.Tanh(concIn[i]);
        }
        return inputs;
    }
    private double[] ForgetGate(double[] concIn)
    {
        double[] forgetGate = new double[concIn.Length];
        for (int i = 0; i < concIn.Length; i++)
        {
            forgetGate[i] = RLManager.math.Sigmoid(concIn[i]);
        }
        return forgetGate;
    }
    private double[] Concatenate(double[] input, double[] cellState)
    {
        double[] concatenatedOutput = new double[input.Length];

        for (int i = 0; i < input.Length; i++)
        {
            concatenatedOutput[i] = double.Parse(input[i].ToString() + cellState[i].ToString());
        }
        return concatenatedOutput;
    }
}
