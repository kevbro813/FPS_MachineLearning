using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RLManager : MonoBehaviour
{
    public static RLManager instance;
    public static MathFunctions math;
    public GameObject agentPrefab;
    public List<GameObject> agentObjectsList;
    public RLComponent rlComponent;
    public RLComponent rlComponentTester;
    public Transform spawnpoint; // Add in inspector
    public Transform agentShell;
    public DefaultSettings defaultSettings;
    public Settings settings;
    public Settings loadSettings;
    public bool isAgentInitialized;
    public float costUpdateInEpochs;
    public float spawnMin_X;
    public float spawnMax_X;
    public float spawnMin_Z;
    public float spawnMax_Z;
    public bool isSessionPaused;

    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        math = new MathFunctions();
        LoadDefaultSettings();
    }
    private void Start()
    {
        rlComponentTester.Init_New_DDQN_Session(true, true, Settings.Algorithm.Double_DQN);
        isAgentInitialized = false;
    }
    private void LoadDefaultSettings()
    {
        settings.agentName = defaultSettings.agentName;
        settings.agentID = defaultSettings.agentID;
        settings.episodeMax = defaultSettings.episodeMax;
        settings.framesPerState = defaultSettings.framesPerState;
        settings.frameBufferSize = defaultSettings.frameBufferSize;
        settings.epiMaxSteps = defaultSettings.epiMaxSteps;
        settings.epsilon = defaultSettings.epsilon;
        settings.epsilonMin = defaultSettings.epsilonMin;
        settings.epsDecayRate = defaultSettings.epsDecayRate;
        settings.expBufferSize = defaultSettings.expBufferSize;
        settings.miniBatchSize = defaultSettings.miniBatchSize;
        settings.netCopyRate = defaultSettings.netCopyRate;
        settings.gamma = defaultSettings.gamma;
        settings.learningRate = defaultSettings.learningRate;
        settings.actorLearningRate = defaultSettings.actorLearningRate;
        settings.criticLearningRate = defaultSettings.criticLearningRate;
        settings.beta1 = defaultSettings.beta1;
        settings.beta2 = defaultSettings.beta2;
        settings.epsilonHat = defaultSettings.epsilonHat;
        settings.gradientThreshold = defaultSettings.gradientThreshold;
        settings.maxViewDistance = defaultSettings.maxViewDistance;
        settings.fieldOfView = defaultSettings.fieldOfView;
        settings.collisionDetectRange = defaultSettings.collisionDetectRange;
        settings.autoSaveEpisode = defaultSettings.autoSaveEpisode;
        settings.activations = defaultSettings.activations;
        settings.criticActivations = defaultSettings.criticActivations;
        settings.actorActivations = defaultSettings.actorActivations;
        settings.algo = defaultSettings.algo;
    }

    public void LoadSettings()
    {
        settings.agentName = loadSettings.agentName;
        settings.agentID = loadSettings.agentID;
        settings.episodeMax = loadSettings.episodeMax;
        settings.framesPerState = loadSettings.framesPerState;
        settings.frameBufferSize = loadSettings.frameBufferSize;
        settings.epiMaxSteps = loadSettings.epiMaxSteps;
        settings.epsilon = loadSettings.epsilon;
        settings.epsilonMin = loadSettings.epsilonMin;
        settings.epsDecayRate = loadSettings.epsDecayRate;
        settings.expBufferSize = loadSettings.expBufferSize;
        settings.miniBatchSize = loadSettings.miniBatchSize;
        settings.netCopyRate = loadSettings.netCopyRate;
        settings.gamma = loadSettings.gamma;
        settings.learningRate = loadSettings.learningRate;
        settings.actorLearningRate = loadSettings.actorLearningRate;
        settings.criticLearningRate = loadSettings.criticLearningRate;
        settings.beta1 = loadSettings.beta1;
        settings.beta2 = loadSettings.beta2;
        settings.epsilonHat = loadSettings.epsilonHat;
        settings.gradientThreshold = loadSettings.gradientThreshold;
        settings.maxViewDistance = loadSettings.maxViewDistance;
        settings.fieldOfView = loadSettings.fieldOfView;
        settings.collisionDetectRange = loadSettings.collisionDetectRange;
        settings.autoSaveEpisode = loadSettings.autoSaveEpisode;
        settings.activations = loadSettings.activations;
        settings.criticActivations = defaultSettings.criticActivations;
        settings.actorActivations = defaultSettings.actorActivations;
        settings.algo = loadSettings.algo;
    }
    public void RandomSpawn()
    {
        float randomX = Random.Range(spawnMin_X, spawnMax_X);
        float randomZ = Random.Range(spawnMin_Z, spawnMax_Z);
        Vector3 randomSpawnVector = new Vector3(randomX, 1.5f, randomZ);
        spawnpoint.position = randomSpawnVector;

    }
    // Method to spawn AI
    public void SpawnAgent()
    {
        RandomSpawn();
        GameObject agentClone = Instantiate(agentPrefab, spawnpoint.position, spawnpoint.rotation, agentShell);
        agentObjectsList.Add(agentClone);
        rlComponent = agentClone.GetComponent<RLComponent>();
    }
}
