using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Settings
{
    // Identifying Info
    public int agentID;
    public string agentName;

    // Hyperparameters
    public int episodeMax;
    public int framesPerState;
    public int frameBufferSize;
    public int epiMaxSteps;
    public float epsilon;
    public float epsilonMin;
    public float epsChangeFactor;
    public int expBufferSize; 
    public int miniBatchSize; 
    public int netCopyRate;
    public float gamma; 
    public double learningRate;
    public float beta1;
    public float beta2;
    public double epsilonHat;
    public double gradientThreshold;
    // Environment Settings
    public float maxViewDistance;
    public float fieldOfView;
    public float collisionDetectRange;

    // Other Settings
    public int autoSaveEpisode;


    //// Other
    //public int[] layers;
    //// Activation functions
    //public string hiddenActivation;
    //public string outputActivation;
}
