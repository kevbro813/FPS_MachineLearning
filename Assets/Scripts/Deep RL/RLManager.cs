using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RLManager : MonoBehaviour
{
    public static RLManager instance;
    public static MathFunctions math; // Math functions needed for algorithms
    public GameObject agentPrefab; // Prefab of the agent to spawn
    public List<GameObject> agentObjectsList; // List of agents TODO: Will be used with async agent update
    public RLComponent rlComponent; // The active reinforcement learning component TODO: Will need to update for async agents
    public RLComponent rlComponentTester; // Used to test the neural network
    public Transform spawnpoint;  
    public Transform agentShell; // Empty shell to organize inspector
    public DefaultSettings defaultSettings; // Store default settings, these will be loaded at startup
    public Settings settings; // Current settings
    public Settings loadSettings; // Used to store settings that are to be loaded
    public bool isAgentInitialized; // Used to activate/deactivate menu buttons (save/resume)
    public float costUpdateInEpochs; // Update the cost display every "X" epochs
    public float spawnMin_X; // Min X bound
    public float spawnMax_X; // Max X bound
    public float spawnMin_Z; // Min Z bound
    public float spawnMax_Z; // Max Z bound
    public bool isSessionPaused; // Used to fully pause the session

    public RandomObjective randObj;
    public Vector3 objectiveLocation;

    public void UpdateObjectiveLocation()
    {
        randObj.RandomLocation(); // TODO: RANDOM OBJECTIVE
        objectiveLocation = randObj.objectiveLocation;
    }
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
        // Initialize the test 
        rlComponentTester.Init_New_DDQN_Session(true, true, settings.dqnNetStructure);
        isAgentInitialized = false; // Set to false to deactivate save and resume buttons at launch
    }
    /// <summary>
    /// Load default settings.
    /// </summary>
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
        settings.dqnLearningRate = defaultSettings.dqnLearningRate;
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
        settings.dqnActivations = defaultSettings.dqnActivations;
        settings.criticActivations = defaultSettings.criticActivations;
        settings.actorActivations = defaultSettings.actorActivations;
        settings.algo = defaultSettings.algo;
        settings.ppoClip = defaultSettings.ppoClip;
        settings.entropyBonus = defaultSettings.entropyBonus;
        settings.tau = defaultSettings.tau;
        settings.trainingEpochs = defaultSettings.trainingEpochs;
        settings.asyncAgents = defaultSettings.asyncAgents;
        settings.dqnNetStructure = defaultSettings.dqnNetStructure;
        settings.actorNetStructure = defaultSettings.actorNetStructure;
        settings.criticNetStructure = defaultSettings.criticNetStructure;
        settings.saveLocation = defaultSettings.saveLocation;
    }
    /// <summary>
    /// Load settings from file.
    /// </summary>
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
        settings.dqnLearningRate = loadSettings.dqnLearningRate;
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
        settings.dqnActivations = loadSettings.dqnActivations;
        settings.criticActivations = loadSettings.criticActivations;
        settings.actorActivations = loadSettings.actorActivations;
        settings.algo = loadSettings.algo;
        settings.ppoClip = loadSettings.ppoClip;
        settings.entropyBonus = loadSettings.entropyBonus;
        settings.tau = loadSettings.tau;
        settings.trainingEpochs = loadSettings.trainingEpochs;
        settings.asyncAgents = loadSettings.asyncAgents;
        settings.dqnNetStructure = loadSettings.dqnNetStructure;
        settings.actorNetStructure = loadSettings.actorNetStructure;
        settings.criticNetStructure = loadSettings.criticNetStructure;
        settings.saveLocation = loadSettings.saveLocation;
    }
    /// <summary>
    /// Calculates a random spawn location (Given x and z bounds)
    /// </summary>
    public void RandomSpawn()
    {
        float randomX = Random.Range(spawnMin_X, spawnMax_X);
        float randomZ = Random.Range(spawnMin_Z, spawnMax_Z);
        Vector3 randomSpawnVector = new Vector3(randomX, 1.5f, randomZ);
        spawnpoint.position = randomSpawnVector;
    }
    /// <summary>
    /// Method to spawn an agent.
    /// </summary>
    public void SpawnAgent()
    {
        RandomSpawn();
        GameObject agentClone = Instantiate(agentPrefab, spawnpoint.position, spawnpoint.rotation, agentShell);
        agentObjectsList.Add(agentClone);
        rlComponent = agentClone.GetComponent<RLComponent>();
    }
}
