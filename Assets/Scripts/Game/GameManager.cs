using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject agentPrefab;
    public GameObject adminMenu;
    public GameObject objective; // Add in inspector
    public UIManager ui;
    public List<GameObject> agentObjectsList;
    public Transform spawnpoint; // Add in inspector
    public DQN dqn;
    public DQN dqnTester;
    public Transform agentShell;
    public string gameState = "pregame";
    public bool isAdminMenu;
    public bool isPaused;
    public bool isStartMenu;
    public bool isAgentLoaded;
    public MathFunctions math;
    public DefaultSettings defaultSettings;
    public Settings settings;
    public float spawnMin_X;
    public float spawnMax_X;
    public float spawnMin_Z;
    public float spawnMax_Z;
    public float costUpdateInEpochs;
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
    }
    void Start()
    {
        ui = GameObject.FindWithTag("UIManager").GetComponent<UIManager>();
        isAgentLoaded = false;
        LoadDefaultSettings();
    }
    public void LoadDefaultSettings()
    {
        settings.agentName = defaultSettings.agentName;
        settings.agentID = defaultSettings.agentID;
        settings.episodeMax = defaultSettings.episodeMax;
        settings.framesPerState = defaultSettings.framesPerState;
        settings.frameBufferSize = defaultSettings.frameBufferSize;
        settings.epiMaxSteps = defaultSettings.epiMaxSteps;
        settings.epsilon = defaultSettings.epsilon;
        settings.epsilonMin = defaultSettings.epsilonMin;
        settings.epsChangeFactor = defaultSettings.epsChangeFactor;
        settings.expBufferSize = defaultSettings.expBufferSize;
        settings.miniBatchSize = defaultSettings.miniBatchSize;
        settings.netCopyRate = defaultSettings.netCopyRate;
        settings.gamma = defaultSettings.gamma;
        settings.learningRate = defaultSettings.learningRate;
        settings.beta1 = defaultSettings.beta1;
        settings.beta2 = defaultSettings.beta2;
        settings.epsilonHat = defaultSettings.epsilonHat;
        settings.gradientThreshold = defaultSettings.gradientThreshold;
        settings.maxViewDistance = defaultSettings.maxViewDistance;
        settings.fieldOfView = defaultSettings.fieldOfView;
        settings.collisionDetectRange = defaultSettings.collisionDetectRange;
        settings.autoSaveEpisode = defaultSettings.autoSaveEpisode;
        settings.activations = defaultSettings.activations;
    }
    // Update is called once per frame
    void Update()
    {
        if (gameState == "pregame")
        {
            DoPregame();
        }
        if (gameState == "active")
        {
            DoActive();
            if (isPaused)
            {
                gameState = "pause";
            }
            else if (isAdminMenu)
            {
                gameState = "admin";
            }
        }
        if (gameState == "admin")
        {
            DoAdmin();
            if (!isAdminMenu)
            {
                gameState = "active";
            }
        }
        if (gameState == "end")
        {
            DoEndGame();
        }
        if (gameState == "pause")
        {
            DoPause();
            if (!isPaused)
            {
                gameState = "active";
            }
            if (isStartMenu)
            {
                gameState = "pregame";
            }
        }
        if (gameState == "continue")
        {
            DoContinue();
            gameState = "active";
        }
        if (gameState == "quit")
        {
            DoQuit();
        }
    }

    public void RandomSpawn()
    {
        float randomX = Random.Range(spawnMin_X, spawnMax_X);
        float randomZ = Random.Range(spawnMin_Z, spawnMax_Z);
        Vector3 randomSpawnVector = new Vector3(randomX, 5.0f, randomZ);
        spawnpoint.position = randomSpawnVector;

    }
    // Method to spawn AI
    public void SpawnAgent()
    {
        RandomSpawn();
        GameObject agentClone = Instantiate(agentPrefab, spawnpoint.position, spawnpoint.rotation, agentShell);
        agentObjectsList.Add(agentClone);
        dqn = agentClone.GetComponent<DQN>();
    }
    public void DoPregame()
    {
        // Disable all AI movement
        Time.timeScale = 0;

        adminMenu.SetActive(true);
        isPaused = false;
    }
    public void DoActive()
    {
        isAgentLoaded = true;
        Time.timeScale = 1;
        adminMenu.SetActive(false);
    }
    public void DoAdmin()
    {
        // Disable all AI movement
        Time.timeScale = 0;
        isPaused = false;
        adminMenu.SetActive(true);
    }
    public void DoContinue()
    {
        // Disable all AI movement
        Time.timeScale = 1;
        isAdminMenu = false;
        isPaused = false;
        adminMenu.SetActive(false);
    }
    public void DoQuit()
    {
        Application.Quit();
    }
    public void DoEndGame()
    {
        Time.timeScale = 0;
        isAdminMenu = false;
        isPaused = false;
        adminMenu.SetActive(false);
    }
    public void DoPause()
    {
        // Disable all AI movement
        Time.timeScale = 0;
        isAdminMenu = false;
        adminMenu.SetActive(false);
    }
}
