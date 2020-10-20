using UnityEngine;
using System;

[Serializable]
public class Settings
{
    [Header("Identifying Info")] 
    public string agentName;
    public int agentID;
    [Space(10)]
    [Header("RL Algorithm")]
    public Algorithm algo;
    public enum Algorithm { Double_DQN, Proximal_Policy_Optimization };
    [Space(10)]
    [Header("Activation Functions")]
    public LayerActivations[] activations;
    public LayerActivations[] criticActivations;
    public LayerActivations[] actorActivations;
    [HideInInspector] public enum LayerActivations { Relu, LeakyRelu, Sigmoid, Tanh, Softmax, None }
    [Space(10)]

    [Header("Hyperparameters")]
    public int[] layers; // TODO: Make this available to change in inspector
    public int episodeMax;
    public int framesPerState; // The number of frames per state
    public int frameBufferSize; // The size of the frame buffer (Calculated when environment is initialized)
    public int epiMaxSteps; // Steps per episode
    public float epsilon; // Used in GetAction function, Epsilon is basically the chance for a random action, Epsilon gradually reduces until it reaches epsilon_min
    public float epsilonMin; // epsilon_min is the lowest value for epsilon, i.e. 0.1 means there is a 10% chance for a random action   
    public float epsDecayRate; // Used to decay epsilon
    public int expBufferSize; // The maximum size of the buffer (Can be viewed as the agent's memory)
    public int miniBatchSize; // Size of the mini-batch used to train the agent
    public int netCopyRate;
    // Research the following settings
    public float gamma; // Discount factor
    public double learningRate;
    public double actorLearningRate;
    public double criticLearningRate;
    public float beta1;
    public float beta2;
    public double epsilonHat; // I have seen set between 10^-8 and 10^-5 (AKA 1e-8 and 1e-5), also 1 or 0.1 have been suggested
    public double gradientThreshold;
    [Space(10)] 
    
    [Header("Environment Settings")]
    public float maxViewDistance;
    public float fieldOfView;
    public float collisionDetectRange;
    [Space(10)] 
    
    [Header("Other")] 
    public int autoSaveEpisode;
}
