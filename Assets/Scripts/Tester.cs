using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        // DATA     ANSWERS
        // 0 0 0    => 0
        // 0 0 1    => 1
        // 0 1 0    => 1
        // 0 1 1    => 0
        // 1 0 0    => 1
        // 1 0 1    => 0
        // 1 1 0    => 0
        // 1 1 1    => 1

        NeuralNetwork net = new NeuralNetwork(new int[] { 3, 25, 25, 25, 1 });

        for (int i = 0; i < 20000; i++)
        {
            net.FeedForward(new double[] { 0, 0, 0 });
            net.Backpropagation(new double[] { 0 });

            net.FeedForward(new double[] { 0, 0, 1 });
            net.Backpropagation(new double[] { 1 });

            net.FeedForward(new double[] { 0, 1, 0 });
            net.Backpropagation(new double[] { 1 });

            net.FeedForward(new double[] { 0, 1, 1 });
            net.Backpropagation(new double[] { 0 });

            net.FeedForward(new double[] { 1, 0, 0 });
            net.Backpropagation(new double[] { 1 });

            net.FeedForward(new double[] { 1, 0, 1 });
            net.Backpropagation(new double[] { 0 });

            net.FeedForward(new double[] { 1, 1, 0 });
            net.Backpropagation(new double[] { 0 });

            net.FeedForward(new double[] { 1, 1, 1 });
            net.Backpropagation(new double[] { 1 });
        }

        UnityEngine.Debug.Log(net.FeedForward(new double[] { 0, 0, 0 })[0]);
        UnityEngine.Debug.Log(net.FeedForward(new double[] { 0, 0, 1 })[0]);
        UnityEngine.Debug.Log(net.FeedForward(new double[] { 0, 1, 0 })[0]);
        UnityEngine.Debug.Log(net.FeedForward(new double[] { 0, 1, 1 })[0]);
        UnityEngine.Debug.Log(net.FeedForward(new double[] { 1, 0, 0 })[0]);
        UnityEngine.Debug.Log(net.FeedForward(new double[] { 1, 0, 1 })[0]);
        UnityEngine.Debug.Log(net.FeedForward(new double[] { 1, 1, 0 })[0]);
        UnityEngine.Debug.Log(net.FeedForward(new double[] { 1, 1, 1 })[0]);
    }
}
