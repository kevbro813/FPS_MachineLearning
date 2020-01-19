using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject aiObjectPrefab;
    public bool isTraining = false;
    public List<GameObject> aiObjectsList;
    public Transform spawnpoint; // Add in inspector
    public GameObject objective; // Add in inspector



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

    }
    // Method to spawn AI
    private void SpawnAI()
    {

    }
}
