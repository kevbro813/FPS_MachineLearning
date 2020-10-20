using UnityEngine;

[CreateAssetMenu(menuName = "Data/DefaultSettings")]
public class DefaultSettings : ScriptableObject
{
    public string agentName = "Default";
    public int agentID = 001;
    public int episodeMax = 1000;
    public int framesPerState = 4;
    public int frameBufferSize = 10000;
    public int epiMaxSteps = 5000;
    public float epsilon = 1.0f;
    public float epsilonMin = 0.1f;
    public float epsDecayRate = 500000;
    public int expBufferSize = 50000;
    public int miniBatchSize = 32;
    public int netCopyRate = 1000;
    public float gamma = 0.95f;
    public double learningRate = 0.0001d;
    public double actorLearningRate = 0.001d;
    public double criticLearningRate = 0.001d;
    public float beta1 = 0.9f;
    public float beta2 = 0.999f;
    public double epsilonHat = 0.001d;
    public double gradientThreshold = 1.0d;
    public float maxViewDistance = 100.0f;
    public float fieldOfView = 45.0f;
    public float collisionDetectRange = 12.0f;
    public int autoSaveEpisode = 10;

    public Settings.Algorithm algo = Settings.Algorithm.Double_DQN;
    public Settings.LayerActivations[] activations = new Settings.LayerActivations[] { Settings.LayerActivations.Sigmoid, Settings.LayerActivations.Sigmoid, Settings.LayerActivations.Softmax };
    public Settings.LayerActivations[] criticActivations = new Settings.LayerActivations[] { Settings.LayerActivations.Relu, Settings.LayerActivations.Relu, Settings.LayerActivations.None };
    public Settings.LayerActivations[] actorActivations = new Settings.LayerActivations[] { Settings.LayerActivations.Tanh, Settings.LayerActivations.Tanh, Settings.LayerActivations.Softmax };
}
