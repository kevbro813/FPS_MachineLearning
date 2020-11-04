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
    public double dqnLearningRate = 0.0001d;
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
    public float ppoClip = 0.2f;
    public double entropyBonus = 0.0000005d;
    public double tau = 0.95d;
    public int trainingEpochs = 4;
    public int asyncAgents = 1;
    public string saveLocation = ""; // Using an empty saveLocation will default to Application.persistentDataPath
    public int[] dqnNetStructure = { 30, 60, 40, 30, 5 };
    public int[] actorNetStructure = { 30, 60, 40, 30, 5 };
    public int[] criticNetStructure = { 30, 60, 40, 30, 1 };
    public Settings.Algorithm algo = Settings.Algorithm.Proximal_Policy_Optimization;
    public Settings.LayerActivations[] dqnActivations = new Settings.LayerActivations[] { Settings.LayerActivations.Sigmoid, Settings.LayerActivations.Sigmoid, Settings.LayerActivations.Softmax };
    public Settings.LayerActivations[] criticActivations = new Settings.LayerActivations[] { Settings.LayerActivations.Relu, Settings.LayerActivations.Relu, Settings.LayerActivations.Linear };
    public Settings.LayerActivations[] actorActivations = new Settings.LayerActivations[] { Settings.LayerActivations.Tanh, Settings.LayerActivations.Tanh, Settings.LayerActivations.Softmax };
}
