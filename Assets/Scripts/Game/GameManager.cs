using System;
using System.Collections;
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
    public Settings settings;
    public List<GameObject> agentObjectsList;
    public Transform spawnpoint; // Add in inspector
    public DQN dqn;
    public Transform agentShell;
    public string gameState = "pregame";
    public bool isTraining;
    public bool isAdminMenu;
    public bool isPaused;
    public bool isStartMenu;
    public bool isAgentLoaded;
    public int agentIDNumber;

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
        settings = new Settings();
    }
    void Start()
    {
        ui = GameObject.FindWithTag("UIManager").GetComponent<UIManager>();
        DefaultSettings();
        isAgentLoaded = false;
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
                Debug.Log("paused");
                gameState = "pause";
            }
            else if (isAdminMenu)
            {
                Debug.Log("admin");
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
    public void DefaultSettings()
    {
        settings.episodeMax = 1000;
        settings.framesPerState = 4;
        settings.frameBufferSize = 10000;
        settings.epiMaxSteps = 5000;
        settings.epsilon = 1.0f;
        settings.epsilonMin = 0.1f;
        settings.epsChangeFactor = 500000;
        settings.expBufferSize = 5000;
        settings.miniBatchSize = 32;
        settings.netCopyRate = 1000;
        settings.gamma = 0.95f;
        settings.learningRate = 0.00000001d;
        settings.beta1 = 0.9f;
        settings.beta2 = 0.999f;
        settings.epsilonHat = 0.00001d;
        settings.gradientThreshold = 1.0f;
        settings.maxViewDistance = 100.0f;
        settings.fieldOfView = 45.0f;
        settings.collisionDetectRange = 10.0f;

        // TODO: Need to add to display
        settings.autoSaveEpisode = 10;

        // TODO: Need to add functionality
        //settings.hiddenActivation = ;
        //settings.outputActivation = ;
        //settings.layers = ;
    }
    // Method to spawn AI
    public void SpawnAgent()
    {
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
