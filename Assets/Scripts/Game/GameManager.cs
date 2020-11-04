using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject adminMenu;
    public UIManager ui;
    public string gameState = "pregame";
    public bool isAdminMenu;
    public bool isPaused;
    public bool isStartMenu;

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
    }
    // Update is called once per frame
    void Update()
    {
        // Main FSM
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
    // Pregame state
    public void DoPregame()
    {
        // Disable all AI movement
        Time.timeScale = 0;

        adminMenu.SetActive(true);
        isPaused = false;
    }
    // Active state (training/game running)
    public void DoActive()
    {
        RLManager.instance.isAgentInitialized = true;
        RLManager.instance.isSessionPaused = false;
        Time.timeScale = 1;
        adminMenu.SetActive(false);
    }
    // Admin menu state
    public void DoAdmin()
    {
        // Disable all AI movement
        Time.timeScale = 0;
        isPaused = false;
        RLManager.instance.isSessionPaused = true;
        adminMenu.SetActive(true);
    }
    // State used to transition back to active game
    public void DoContinue()
    {
        Time.timeScale = 1;
        isAdminMenu = false;
        isPaused = false;
        RLManager.instance.isSessionPaused = false;
        adminMenu.SetActive(false);
    }
    // Quit game
    public void DoQuit()
    {
        Application.Quit();
    }
    // State when training ession is over
    public void DoEndGame()
    {
        Time.timeScale = 0;
        isAdminMenu = false;
        isPaused = false;
        adminMenu.SetActive(false);
    }
    // Pause game state
    public void DoPause()
    {
        // Disable all AI movement
        Time.timeScale = 0;
        isAdminMenu = false;
        RLManager.instance.isSessionPaused = true;
        adminMenu.SetActive(false);
    }
}
