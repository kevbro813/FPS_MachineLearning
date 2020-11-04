using UnityEngine;
using System;

[Serializable]
public class Settings
{
    [Header("Identifying Info")] 
    public string agentName; // Unique identifier for the agent
    public int agentID; // Unique ID number TODO: Will be used when with async agent update
    [Space(10)]
    [Header("RL Algorithm")]
    public Algorithm algo; // Enum for algorithm selection
    public enum Algorithm { Double_DQN, Proximal_Policy_Optimization };
    [Space(10)]
    [Header("Activation Functions")] // Each layer is represented by an index containing the activation function
    public LayerActivations[] dqnActivations; // DQN activation functions
    public LayerActivations[] criticActivations; // Critic network activation functions
    public LayerActivations[] actorActivations; // Actor network activation functions
    [HideInInspector] public enum LayerActivations { Relu, LeakyRelu, Sigmoid, Tanh, Softmax, Linear }
    [Space(10)]

    [Header("Hyperparameters")]
    public int[] dqnNetStructure; // Neural network structure for DQN networks (Two exact copies of the same weights and biases)
    public int[] actorNetStructure; // Neural network structure for PPO Actor network
    public int[] criticNetStructure; // Neural network structure for PPO Critic network
    public int episodeMax; // Maximum number of episodes for the training session
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
    public float gamma; // Discount factor for rewards
    public double dqnLearningRate; // Learning rate used for DQN networks
    public double actorLearningRate; // Learning rate used for Actor networks
    public double criticLearningRate; // Learning rate used for Critic networks
    public float beta1; // Used with Adam optimizer
    public float beta2; // Used with Adam optimizer
    public double epsilonHat; // I have seen set between 10^-8 and 10^-5 (AKA 1e-8 and 1e-5), also 1 or 0.1 have been suggested
    public double gradientThreshold; // ***NOT IN USE TODO: remove or make for dqn only
    [Space(10)] 
    
    [Header("Environment Settings")]
    public float maxViewDistance; // Maximum distance for raycasts
    public float fieldOfView; // Field of view ***NOT IN USE TODO: Will be used in agent vision update
    public float collisionDetectRange; // Detection range for collision penalty
    [Space(10)] 
    
    [Header("Other")] 
    public int autoSaveEpisode; // The neural networks will be saved in intervals of this value
    public string saveLocation; // Used to change the save location for neural networks and settings data

    [Header("PPO Only")] 
    public float ppoClip; // Used to clip the Surrogate Objective Function
    public double entropyBonus; // Entropy bonus is used to promote exploration
    public double tau; // Tau is a discount factor used with CSOF/PPO
    public int trainingEpochs; // Number of times the neural network will be trained on the data collected during an episode
    public int asyncAgents; // Number of asynchronous agents TODO: Will be used in async agents update
}
