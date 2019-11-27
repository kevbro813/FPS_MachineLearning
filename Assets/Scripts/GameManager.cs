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
    public int populationSize = 50;
    public int generationNumber = 0;
    public int[] layers = new int[] { 10, 10, 10, 3 };
    public List<NeuralNetwork> neuralNets;
    public List<NeuralNetwork> savedNets;
    public List<AIController> aiControllerList;
    public List<GameObject> aiObjectsList;
    public Transform spawnpoint; // Add in inspector
    public float genLifespan = 10f; // Lifespan of each generation
    public int averageFitness = 0;
    public bool isLoadingNet = false;
    public GameObject objective; // Add in inspector
    public bool isSexual;
    public Transform aiShell;
    public float obstacleCollisionPenalty = -1000f;
    public float objectiveReward = 10000f;
    public float inSightReward = 100f;
    public float distanceReward = 100f;
    public float movingForwardReward = 100f;
    public float movingLateralReward = 100f;
    public float killBoxPenalty = -1000f;
    public float rotationPenalty = -10f;
    public float targetDistReward = 10f;

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
    // Timer function. Use genLifespan to adjust how long each generation will last
    void Timer()
    {
        isTraining = false;
    }
    // Update is called once per frame
    void Update()
    {
        if (isTraining == false)
        {
            // If this is a new simulation then initialize the neural net.
            if (generationNumber == 0)
            {
                InitAINeuralNet();
            }
            // If this is a continuation of a simulation then...
            else
            {
                neuralNets.Sort(); // Sort the neural nets (Icomparable - CompareTo())
                if (isLoadingNet == true)
                {
                    neuralNets = savedNets;
                    isLoadingNet = false;
                }
                float totalFitness = 0;
                GameManager.instance.objective.GetComponent<Objective>().winners = 0; // Reset winners each generation
                // (0-24%)
                for (int i = 0; i < populationSize / 4; i++)
                {
                    neuralNets[i] = new NeuralNetwork(layers);
                    neuralNets[i].SetType(NeuralNetwork.AIType.Random);
                }
                int j = 0;
                // (25-49%) Mate the top 25% with a random between 50-100%.
                for (int i = populationSize / 4; i < populationSize / 2; i++)
                {
                    if (isSexual)
                    {
                        neuralNets[i].SexualReproduction(neuralNets[UnityEngine.Random.Range(populationSize / 2, populationSize - 1)], neuralNets[populationSize - 1 - j], neuralNets[i]);
                        j++;
                        neuralNets[i].SetType(NeuralNetwork.AIType.Children);
                    }
                    else
                    {
                        neuralNets[i].Mutate();
                        neuralNets[i].SetType(NeuralNetwork.AIType.Survivor);
                    }
                    neuralNets[i].Mutate();
                    
                }
                // (50-74%) 
                for (int i = populationSize / 2; i < (populationSize / 10) * 9; i++)
                {
                    neuralNets[i].Mutate();
                    neuralNets[i].SetType(NeuralNetwork.AIType.Fit);
                }
                // (90-100%) Keep the top 25% the same
                for (int i = (populationSize / 10) * 9; i < populationSize; i++)
                {
                    neuralNets[i].SetType(NeuralNetwork.AIType.Fittest);
                }

                float averageFitness = totalFitness / populationSize; // TODO: Calculate the average fitness

                for (int i = 0; i < populationSize; i++)
                {
                    neuralNets[i].SetFitness(0f); // Reset fitness of all neural networks with each generation
                }
            }
            generationNumber++;

            isTraining = true;
            Invoke("Timer", genLifespan); // Adds a timer between generation creation
            SpawnAI();
        }
    }
    // Method to spawn AI
    private void SpawnAI()
    {
        // Clear out the previous list by destroying any AI still alive
        if (aiObjectsList != null)
        {
            for (int i = 0; i < aiObjectsList.Count; i++)
            {
                GameObject.Destroy(aiObjectsList[i]);
            }
        }

        aiControllerList = new List<AIController>();

        // Loop to instantiate a generation of AI
        for (int i = 0; i < populationSize; i++)
        {
            GameObject aiObject = Instantiate(aiObjectPrefab, spawnpoint.position, spawnpoint.rotation, aiShell) as GameObject;

            AIController controller = aiObject.GetComponent<AIController>();

            controller.Init(neuralNets[i]); // Initialize the AI's neural net

            // Create lists of AI objects and controllers
            aiObjectsList.Add(aiObject);
            aiControllerList.Add(controller);
        }  
    }
    private void InitAINeuralNet()
    {
        // Population size must be a multiple of 10
        if (populationSize % 10 != 0)
        {
            populationSize = 10;
        }

        neuralNets = new List<NeuralNetwork>();

        // Initialize a new neural net
        for (int i = 0; i < populationSize; i++)
        {
            NeuralNetwork net = new NeuralNetwork(layers);
            net.Mutate();
            neuralNets.Add(net);
        }
        // TODO: Add ability to save and load neural nets
    }
}
