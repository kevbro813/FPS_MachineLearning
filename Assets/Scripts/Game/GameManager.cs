﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject agentPrefab;
    public bool isTraining = false;
    public List<GameObject> agentObjectsList;
    public Transform spawnpoint; // Add in inspector
    public GameObject objective; // Add in inspector
    public DQN dqn;
    public Transform agentShell;
    public string gameState = "pregame";
    public bool isAdminMenu;
    public bool isPaused;
    public bool isStartMenu;
    public GameObject adminMenu;
    public UIManager ui;
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
    void Start()
    {
        ui = GameObject.FindWithTag("UIManager").GetComponent<UIManager>();
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